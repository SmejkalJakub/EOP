using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ITU_EOP
{
    /// <summary>
    /// Nejrozsáhlejší třída která řídí zobrazování hlavní části programu a to je zobrazování 
    /// </summary>
    public class ApplicationListViewModel : ViewModelBase, INotifyPropertyChanged
    {
        #region Variables
        bool timerState = true;

        bool timerStopedByCommand = false;

        bool notificationAllowed = true;

        int procrastinationTimer = 1;


        BitmapImage workOn = new BitmapImage(new Uri("pack://application:,,,/Icons/workOn.png"));
        BitmapImage workOff = new BitmapImage(new Uri("pack://application:,,,/Icons/workOff.png"));
        BitmapImage funOn = new BitmapImage(new Uri("pack://application:,,,/Icons/funOn.png"));
        BitmapImage funOff = new BitmapImage(new Uri("pack://application:,,,/Icons/funOff.png"));
        BitmapImage otherOn = new BitmapImage(new Uri("pack://application:,,,/Icons/othersOn.png"));
        BitmapImage otherOff = new BitmapImage(new Uri("pack://application:,,,/Icons/othersOff.png"));


        DateTime selectedDate = DateTime.Today;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        string[] discludedPrograms = { "Přepínání úloh", "Hledání", String.Empty, " ", "Uložit jako", "Nové oznámení", "Centrum akcí" };

        int wholeTime = 0;

        readonly DispatcherTimer applicationTimer = new DispatcherTimer();
        readonly DispatcherTimer statisticsTimer = new DispatcherTimer();
        readonly DispatcherTimer targetsTimer = new DispatcherTimer();
        readonly DispatcherTimer saveTimer = new DispatcherTimer();

        Dictionary<string, string> knownCategories = new Dictionary<string, string>();

        readonly TimeSpan oneSecond = new TimeSpan(0, 0, 1);

        readonly SimpleMessage refreshMessage;

        FileOperator fileOp = new FileOperator();

        Notfications notification = new Notfications();

        int mode;

        Dictionary<string, Category> categories = new Dictionary<string, Category>();

        bool firstSend = true;

        private ObservableCollection<Application> applications;
        public ObservableCollection<Application> Applications
        {
            get { return applications; }
            set
            {
                if (value != applications)
                {
                    applications = value;
                    OnPropertyChanged("Applications");
                }
            }
        }
        #endregion

        #region Commands
        public ICommand setCategory { get { return new RelayCommand<object>(setCategoryExecute); } }
        public ICommand deleteApplication { get { return new RelayCommand<object>(deleteApplicationExecute); } }

        private void setCategoryExecute(object obj)
        {
            var parameters = (object[])obj;
            var application = (Application)parameters[1];

            categories[application.Category.name].timeInCategory = categories[application.Category.name].timeInCategory.Add(-(application.timeInApp));


            application.Category = categories[parameters[0].ToString()];

            categories[application.Category.name].timeInCategory = categories[application.Category.name].timeInCategory.Add(application.timeInApp);

            knownCategories[application.name] = parameters[0].ToString();

            setCategoryIcon(application, parameters[0].ToString());

            saveLogCommand(selectedDate);

        }

        private void setCategoryIcon(Application application, string name)
        {
            if (name == "Work")
            {
                application.workIcon = workOn;
                application.funIcon = funOff;
                application.otherIcon = otherOff;
            }

            else if (name == "Fun")
            {
                application.workIcon = workOff;
                application.funIcon = funOn;
                application.otherIcon = otherOff;
            }

            else if (name == "Other")
            {
                application.workIcon = workOff;
                application.funIcon = funOff;
                application.otherIcon = otherOn;
            }
        }

        private void deleteApplicationExecute(object applicationName)
        {
            Application app = (Application)applicationName;
            wholeTime -= (int)app.timeInApp.TotalSeconds;
            Messenger.Default.Send(new TimeSpan(0, 0, wholeTime));

            Applications.Remove(Applications.Single(i => i.name == app.name));
            recountThePercentage();
            saveLogCommand(selectedDate);
        }
        #endregion


        public ApplicationListViewModel()
        {
            categories.Add("Work", new Category("Work", new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 0), "Práce"));
            categories.Add("Fun", new Category("Fun", new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 0), "Zábava"));
            categories.Add("Other", new Category("Other", new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 0), "Ostatní"));

            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory() + "\\targets\\" + DateTime.Today.Year + DateTime.Today.Month + DateTime.Today.Day + ".json")))
            {
                var targets = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, TimeSpan>>(File.ReadAllText(
                    Path.Combine(Directory.GetCurrentDirectory() + "\\targets\\" + DateTime.Today.Year + DateTime.Today.Month + DateTime.Today.Day + ".json")));

                foreach (KeyValuePair<string, TimeSpan> item in targets)
                {
                    var category = categories[item.Key];
                    category.target = item.Value;

                }
            }
            
            refreshMessage = new SimpleMessage { Type = SimpleMessage.MessageType.TimerTick };

            string categoriesFilePath = Path.Combine(Directory.GetCurrentDirectory() + "\\" + "categories.json");

            setTimers();

            if (!fileOp.checkFile(categoriesFilePath))
            {
                knownCategories.Add("EOP", "Other");
                File.Create(categoriesFilePath);
            }
            else
            {
                knownCategories = fileOp.loadFile(categoriesFilePath);
            }

            if (!fileOp.checkFile(DateTime.Now))
            {
                Applications = new ObservableCollection<Application>();
                applicationTimer.Start();
            }
            else
            {
                Applications = fileOp.loadFile(DateTime.Now, ref wholeTime);

                foreach (Application item in Applications)
                {
                    categories[item.Category.name].timeInCategory = categories[item.Category.name].timeInCategory.Add(item.timeInApp);
                }

                recountThePercentage();
                applicationTimer.Start();
            }

            Messenger.Default.Register<SimpleMessage>(this, ConsumeMessage);
            Messenger.Default.Register<DateTime>(this, "applicationToken", ChangeDay);
            Messenger.Default.Register<int>(this, setMode);
            Messenger.Default.Register<bool>(this, setNotification);

        }

        #region MessengerMethods
        private void setNotification(bool obj)
        {
            notificationAllowed = obj;
        }

        private void setMode(int type)
        {
            mode = type;
        }

        private void ChangeDay(DateTime date)
        {
            selectedDate = date;
            Applications.Clear();
            try
            {
                wholeTime = 0;
                if(mode == 0)
                {
                    Applications = fileOp.loadFile(date, ref wholeTime);
                }
                else if(mode == 1)
                {
                    Applications = fileOp.loadMonth(date, ref wholeTime);
                }
                else if(mode == 2)
                {
                    Applications = fileOp.loadYear(date, ref wholeTime);
                }
                foreach (Application item in Applications)
                {
                    if(knownCategories.ContainsKey(item.name))
                    {
                        setCategoryIcon(item, knownCategories[item.name]);
                    }
                    else
                    {
                        setCategoryIcon(item, item.Category.name);
                    }

                }
                recountThePercentage();
                Messenger.Default.Send(new TimeSpan(0, 0, wholeTime));

            }
            catch
            {
                Messenger.Default.Send(new TimeSpan(0, 0, wholeTime));

                return;
            }
        }
        #endregion

        void recountThePercentage()
        {
            foreach (Application item in applications)
            {
                item.percenTimeInApp = (int)((item.timeInApp.TotalSeconds / wholeTime) * 100);
            }
        }


        private void setTimers()
        {
            applicationTimer.Tick += new EventHandler(applicationTimer_Tick);
            applicationTimer.Interval = new TimeSpan(0, 0, 1);

            saveTimer.Tick += new EventHandler(saveLog);
            saveTimer.Tag = Applications;
            saveTimer.Interval = new TimeSpan(0, 0, 5);
            saveTimer.Start();

            statisticsTimer.Tick += new EventHandler(statisticsTimer_Tick);
            statisticsTimer.Interval = new TimeSpan(0, 0, 1);

            targetsTimer.Tick += new EventHandler(targetsTimer_Tick);
            targetsTimer.Interval = new TimeSpan(0, 0, 1);
        }

        private void targetsTimer_Tick(object sender, EventArgs e)
        {
            Messenger.Default.Send(categories);

            targetsTimer.Stop();
        }

        private void statisticsTimer_Tick(object sender, EventArgs e)
        {
            Messenger.Default.Send(applications);

            statisticsTimer.Stop();
        }

        public void saveLog(object sender, EventArgs e)
        {
            saveLogCommand(DateTime.Today);
        }

        private void saveLogCommand(DateTime _date)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(Applications);
            File.WriteAllText(fileOp.makePath(_date), json);


            string categoriesJson = Newtonsoft.Json.JsonConvert.SerializeObject(knownCategories);
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory() + "\\" + "categories.json"), categoriesJson);
        }

        private void ConsumeMessage(SimpleMessage message)
        {
            switch (message.Type)
            {
                case SimpleMessage.MessageType.StopTimer:
                    timerState = false;
                    Messenger.Default.Send(Applications);
                    saveTimer.Stop();
                    break;
                case SimpleMessage.MessageType.ForceStopTimer:
                    timerStopedByCommand = true;
                    break;
                case SimpleMessage.MessageType.ForceStartTimer:
                    timerStopedByCommand = false;
                    break;
                case SimpleMessage.MessageType.StartTimer:
                    timerState = true;
                    saveTimer.Start();
                    break;
                case SimpleMessage.MessageType.SwitchToStatisticsView:
                    statisticsTimer.Start();
                    break;
                case SimpleMessage.MessageType.SwitchToTargetsView:
                    targetsTimer.Start();
                    break;

            }
        }

        private void applicationTimer_Tick(object sender, EventArgs e)
        {
            if (firstSend)
            {
                Messenger.Default.Send(new TimeSpan(0, 0, wholeTime));
                firstSend = false;
            }
            

            string strTitle = getWindowName();

            if (discludedPrograms.Contains(strTitle))
            {
                return;
            }

            if (procrastinationTimer % 60 == 0)
            {
                notification.ShowNotification(new TimeSpan(0, 0, procrastinationTimer).ToString());
            }

            if(strTitle != "EOP" && !timerState)
            {
                Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.ApplicationDeactivated });
            }

            if (timerState && !timerStopedByCommand)
            {
                if (Applications.Any(p => p.name == strTitle))
                {
                    var application = Applications.Single(i => i.name == strTitle);
                    application.timeInApp = application.timeInApp.Add(oneSecond);
                    wholeTime++;

                    application.percenTimeInApp = (int)((application.timeInApp.TotalSeconds / wholeTime) * 100);

                    categories[application.Category.name].timeInCategory = categories[application.Category.name].timeInCategory.Add(oneSecond);

                    foreach (KeyValuePair<string, Category> item in categories)
                    {
                        if(item.Value.timeInCategory == item.Value.target && item.Value.target != new TimeSpan(0,0,0))
                        {
                            notification.ShowNotificationCategory(item.Value.translatedName);
                        }
                    }

                    recountThePercentage();


                    Messenger.Default.Send(refreshMessage);

                    if (notificationAllowed)
                    {
                        if (application.Category.name != "Work")
                        {
                            procrastinationTimer++;
                        }
                        else
                        {
                            procrastinationTimer = 1;
                        }
                    }
                }
                else
                {
                    if (knownCategories.ContainsKey(strTitle))
                    {
                        Applications.Add(new Application { name = strTitle, timeInApp = new TimeSpan(), Category = categories[knownCategories[strTitle]]});
                        var application = Applications.Single(i => i.name == strTitle);
                        setCategoryIcon(application, categories[knownCategories[strTitle]].name);
                    }
                    else
                    {

                        Applications.Add(new Application { name = strTitle, timeInApp = new TimeSpan(), Category = categories["Other"]});
                        knownCategories.Add(strTitle, "Other");
                        var application = Applications.Single(i => i.name == strTitle);
                        setCategoryIcon(application, "Other");
                    }


                }
            }
            
        }


        /// <summary>
        /// NEAUTORSKÉ
        /// Zdroj: https://docs.microsoft.com/cs-cz/dotnet/framework/wpf/data/how-to-implement-property-change-notification
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private string getWindowName()
        {
            var strTitle = string.Empty;
            var handle = GetForegroundWindow();

            var intLength = GetWindowTextLength(handle) + 1;
            var stringBuilder = new StringBuilder(intLength);

            if (GetWindowText(handle, stringBuilder, intLength) > 0)
            {
                strTitle = stringBuilder.ToString();
                strTitle = strTitle.Substring(strTitle.LastIndexOf("-") + 1);

                if (strTitle[0] == ' ')
                {
                    strTitle = strTitle.Remove(0, 1);
                }
            }
            return strTitle;
        }

    }



    /// <summary>
    /// NEAUTORSKÉ
    /// Autor: https://stackoverflow.com/a/53965139
    /// </summary>
    public class ArrayMultiValueConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}


/*
 * ITU projekt - TimeTracker
 * ViewModel pro hlavní okno aplikace sloužící jako "Obal" pro všechny ostatní viewModely
 * Jakub Smejkal, xsmejk28
 */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ITU_EOP
{

    public class MainViewModel : ViewModelBase, INotifyPropertyChanged
    {

        /// <summary>
        /// Hodnota zobrazení StackPanelu s datem a èasem
        /// </summary>
        private Visibility stackVisibility;
        public Visibility StackVisibility
        {
            get
            {
                return stackVisibility;
            }
            set
            {
                stackVisibility = value;
                OnPropertyChanged("StackVisibility");
            }
        }

        /// <summary>
        /// Instance tøídy pro pøepínání ViewModelu z MVVMLight nugget balíèku
        /// </summary>
        private readonly ViewModelLocator _locator = new ViewModelLocator();

        /// <summary>
        /// Ikony pro Pause Button a Notification Button nastavené ze Složky Icons
        /// </summary>
        BitmapImage pauseIcon = new BitmapImage(new Uri("pack://application:,,,/Icons/pauseIcon.png"));
        BitmapImage playIcon = new BitmapImage(new Uri("pack://application:,,,/Icons/playIcon.png"));

        BitmapImage notificationsOn = new BitmapImage(new Uri("pack://application:,,,/Icons/notificationsOn.png"));
        BitmapImage notificationsOff = new BitmapImage(new Uri("pack://application:,,,/Icons/notificationsOff.png"));


        readonly string[] monthsString = { "Leden", "Únor", "Bøezen", "Duben", "Kvìten", "Èerven", "Èervenec", "Srpen", "Záøí", "Øíjen", "Listopad", "Prosinec" };

        /// <summary>
        /// Binding pro Ikony Pause a Notification Button
        /// </summary>
        private ImageSource _PlayPauseImg;
        public ImageSource PlayPauseImg
        {
            get
            {
                return _PlayPauseImg;
            }
            set
            {
                if (value != null)
                {
                    _PlayPauseImg = value;
                    OnPropertyChanged("PlayPauseImg");
                }
            }
        }

        private ImageSource _NotificationImg;
        public ImageSource NotificationImg
        {
            get
            {
                return _NotificationImg;
            }
            set
            {
                if (value != null)
                {
                    _NotificationImg = value;
                    OnPropertyChanged("NotificationImg");
                }
            }
        }

        /// <summary>
        /// Enum pro mody zobrazení Den, Mìsíc, Rok
        /// </summary>
        enum modes
        {
            days,
            months,
            years
        }

        modes mode = modes.days;

        /// <summary>
        /// Pøíkazy využívané Tlaèítky ve View
        /// </summary>
        #region Commands
        public ICommand switchStateCommand { get { return new RelayCommand<object>(switchState); } }

        public ICommand PauseTimer { get; private set; }
        public ICommand Timer { get; private set; }
        public ICommand Targets { get; private set; }
        public ICommand Statistics { get; private set; }
        public ICommand Next { get; private set; }
        public ICommand Prev { get; private set; }
        public ICommand NotificationCommand { get; private set; }


        /// <summary>
        /// Zmìna ikony notification Buttonu
        /// </summary>
        private void NotificationChange()
        {
            notificationsStateOn = !notificationsStateOn;
            if (notificationsStateOn)
            {
                NotificationImg = notificationsOn;
            }
            else
            {
                NotificationImg = notificationsOff;
            }
            Messenger.Default.Send(notificationsStateOn);
        }


        /// <summary>
        /// Zmìna data na parametr obj
        /// </summary>
        /// <param name="obj"></param>
        private void changeDate(DateTime obj)
        {
            DateString = obj.ToShortDateString();
            Date = obj;
        }

        /// <summary>
        /// Zmìna celkového èasu na parametr obj
        /// </summary>
        /// <param name="obj"></param>
        private void setWholeTimer(TimeSpan obj)
        {
            WholeTime = obj;
            if(date == DateTime.Today)
            {
                TodayWholeTime = WholeTime;
            }
        }

        /// <summary>
        /// Zmìna stavu zobrazení Den, Mìsíc, Rok
        /// </summary>
        /// <param name="obj"></param>
        private void switchState(object obj)
        {
            string state = (string)obj;

            if (state != "Today")
            {
                if (state == "Day")
                {
                    mode = modes.days;
                    DateString = Date.ToShortDateString();
                }
                else if (state == "Month")
                {
                    mode = modes.months;
                    DateString = monthsString[Date.Month - 1];
                }
                else if (state == "Year")
                {
                    mode = modes.years;
                    DateString = Date.Year.ToString();
                }
                Messenger.Default.Send((int)mode);
                Messenger.Default.Send(Date, "applicationToken");
                if (!(mode == modes.days && Date == DateTime.Today))
                {
                    Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.StopTimer });

                }
            }
            else
            {
                Date = DateTime.Today;
                DateString = Date.ToShortDateString();

                mode = modes.days;

                Messenger.Default.Send((int)mode);
                Messenger.Default.Send(Date, "applicationToken");
                Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.StartTimer });
            }
        }

        /// <summary>
        /// Commandy pro pøepnutí na další nebo pøedchozí v øadì Den, Mìsíc, Rok
        /// </summary>
        private void PrevCommand()
        {
            Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.NotInStatisticsView });

            if (mode == modes.days)
            {
                Date = Date.AddDays(-1);
                DateString = Date.ToShortDateString();

            }

            else if (mode == modes.months)
            {
                Date = Date.AddMonths(-1);
                DateString = monthsString[Date.Month - 1];
            }

            else if (mode == modes.years)
            {
                Date = Date.AddYears(-1);

                DateString = Date.Year.ToString();

            }
            Messenger.Default.Send(Date, "applicationToken");
            Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.StopTimer });



        }

        private void NextCommand()
        {
            Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.NotInStatisticsView });

            if (mode == modes.days)
            {
                if (Date != DateTime.Today)
                {
                    Date = Date.AddDays(1);
                    Messenger.Default.Send(Date, "applicationToken");
                    Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.StopTimer });
                }
                if (Date == DateTime.Today)
                {
                    Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.StartTimer });
                }
                DateString = Date.ToShortDateString();
            }
            else if (mode == modes.months)
            {
                Date = Date.AddMonths(1);

                DateString = monthsString[Date.Month - 1];

                Messenger.Default.Send(Date, "applicationToken");
                Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.StopTimer });
            }
            else if (mode == modes.years)
            {
                Date = Date.AddYears(1);
                DateString = Date.Year.ToString();
                Messenger.Default.Send(Date, "applicationToken");
                Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.StopTimer });

            }


        }

        /// <summary>
        /// Zastavení mìøení èasu
        /// </summary>
        private void PauseTimerExecute()
        {
            if (timerOn)
            {
                PlayPauseImg = playIcon;
                Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.ForceStopTimer });
            }
            else
            {
                PlayPauseImg = pauseIcon;
                Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.ForceStartTimer });
            }
            timerOn = !timerOn;
        }

        /// <summary>
        /// Pøepnutí na Applist View
        /// </summary>
        private void ApplicationListExecute()
        {
            CurrentViewModel = _locator.AppList;
            Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.SwitchToApplicationListView });
            StackVisibility = Visibility.Visible;

        }

        /// <summary>
        /// Pøepnutí na Targets View
        /// </summary>
        private void TargetsExecute()
        {
            CurrentViewModel = _locator.Targets;
            Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.SwitchToTargetsView });
            StackVisibility = Visibility.Hidden;
        }

        /// <summary>
        /// Pøepnutí na Statistics View
        /// </summary>
        private void StatisticsExecute()
        {
            CurrentViewModel = _locator.Statistics;
            Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.NotInStatisticsView });

            Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.SwitchToStatisticsView });
            StackVisibility = Visibility.Visible;

        }
        #endregion

        bool timerOn = true;

        bool notificationsStateOn = true;

        /// <summary>
        /// Dnešní datum pro zobrazení v cílích
        /// </summary>
        private string today;
        public string Today
        {
            get
            {
                return today;
            }
            set
            {
                today = value;
                OnPropertyChanged("Today");
            }
        }

        /// <summary>
        /// Dnešní celkový èas pro zobrazení v cílích
        /// </summary>
        private TimeSpan todayWholeTime;
        public TimeSpan TodayWholeTime
        {
            get
            {
                return todayWholeTime;
            }
            set
            {
                todayWholeTime = value;
                OnPropertyChanged("TodayWholeTime");
            }
        }

        /// <summary>
        /// Konkrétní zvolené datum Den, Mìsíc, Rok
        /// </summary>
        private DateTime date;
        public DateTime Date
        {
            get
            {
                return date;
            }
            set
            {
                date = value;
                OnPropertyChanged("Date");
            }
        }

        /// <summary>
        /// Konkrétní celkový èas ve zvolené datum
        /// </summary>
        private TimeSpan wholeTime;
        public TimeSpan WholeTime
        {
            get
            {
                return wholeTime;
            }
            set
            {
                wholeTime = value;
                OnPropertyChanged("WholeTime");
            }
        }

        /// <summary>
        /// Binding pro zobrazení Data, nutno ve stringu kvùli zobrazení roku a mìsíce
        /// </summary>
        private string dateString;
        public string DateString
        {
            get
            {
                return dateString;
            }
            set
            {
                dateString = value;
                OnPropertyChanged("DateString");
            }
        }

        private readonly TimeSpan oneSecond = new TimeSpan(0, 0, 1);
        readonly FileOperator fileOp = new FileOperator();

        /// <summary>
        /// Momentální zobrazený ViewModel AppList, Targets, Statistics
        /// </summary>
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get
            {
                return _currentViewModel;
            }
            set
            {
                if (_currentViewModel == value)
                    return;
                _currentViewModel = value;
                OnPropertyChanged("CurrentViewModel");
            }
        }

        public MainViewModel()
        {
            fileOp.createDirectories();
            
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory() + "\\targets")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory() + "\\targets"));
            }

            StackVisibility = Visibility.Visible;
            Date = DateTime.Today;
            DateString = date.ToShortDateString();

            CurrentViewModel = _locator.AppList;

            Timer = new CustomRelayCommand(o => ApplicationListExecute());
            Targets = new CustomRelayCommand(o => TargetsExecute());
            Statistics = new CustomRelayCommand(o => StatisticsExecute());
            PauseTimer = new CustomRelayCommand(o => PauseTimerExecute());
            Next = new CustomRelayCommand(o => NextCommand());
            Prev = new CustomRelayCommand(o => PrevCommand());
            NotificationCommand = new CustomRelayCommand(o => NotificationChange());


            PlayPauseImg = pauseIcon;
            NotificationImg = notificationsOn;

            Today = DateTime.Today.ToShortDateString();


            Messenger.Default.Register<TimeSpan>(this, setWholeTimer);
            Messenger.Default.Register<SimpleMessage>(this, ConsumeMessage);
            Messenger.Default.Register<DateTime>(this, "token", changeDate);
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


        /// <summary>
        /// Zpracování jednoduché zprávy odeslané z jiného ViewModelu
        /// </summary>
        /// <param name="message"></param>
        private void ConsumeMessage(SimpleMessage message)
        {
            switch (message.Type)
            {
                case SimpleMessage.MessageType.TimerTick:
                    WholeTime = wholeTime.Add(oneSecond);
                    if (date == DateTime.Today && StackVisibility == Visibility.Visible)
                    {
                        TodayWholeTime = WholeTime;
                    }
                    break;
                case SimpleMessage.MessageType.ApplicationDeactivated:
                    switchState("Today");
                    Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.StartTimer });
                    break;
            }
        }
    }
}
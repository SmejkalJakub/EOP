using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ITU_EOP
{
    /// <summary>
    /// View Model pro Targets View
    /// </summary>
    public class TargetsViewModel : ViewModelBase
    {
        /// <summary>
        /// Slovník pro cíle v jednotlivých aplikacích
        /// </summary>
        Dictionary<string, TimeSpan> targets = new Dictionary<string, TimeSpan>()
        {
            {
                "Work", new TimeSpan(0,0,0)
            },
            {
                "Fun", new TimeSpan(0,0,0)
            },
            {
                "Other", new TimeSpan(0,0,0)
            }
        
        };

        /// <summary>
        /// Časovač pro ukládání cílů
        /// </summary>
        DispatcherTimer timer = new DispatcherTimer();


        private ObservableCollection<Category> categories = new ObservableCollection<Category>();
        public ObservableCollection<Category> Categories
        {
            get { return categories; }
            set
            {
                if (value != categories)
                {
                    categories = value;
                    OnPropertyChanged("Categories");

                }
            }
        }



        public TargetsViewModel()
        {
            timer.Tick += new EventHandler(saveTargets);
            timer.Interval = new TimeSpan(0, 0, 1);

            Messenger.Default.Register<Dictionary<string, Category>>(this, (cat) =>
            {
                Categories.Clear();
                foreach (KeyValuePair<string, Category> item in cat)
                {
                    Categories.Add(item.Value);
                }
                if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory() + "\\targets\\" + DateTime.Today.Year + DateTime.Today.Month + DateTime.Today.Day + ".json")))
                {
                    File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory() + "\\targets\\" + DateTime.Today.Year + DateTime.Today.Month + DateTime.Today.Day + ".json"), "");
                } 
                timer.Start();

            });

        }
        /// <summary>
        /// timer Event pro uložení cílů
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveTargets(object sender, EventArgs e)
        {
            saveCommand();
        }

        /// <summary>
        /// Uložení cílů pro dnešní den do databáze
        /// </summary>
        private void saveCommand()
        {
            foreach (Category item in Categories)
            {
                targets[item.name] = item.target;
            }
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(targets);
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory() + "\\targets\\" + DateTime.Today.Year + DateTime.Today.Month + DateTime.Today.Day + ".json"), json);
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
    }
}

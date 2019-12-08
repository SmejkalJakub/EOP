/*
 *  ITU projekt - TimeTracker 
 *  Zpracovani dat pro vzkresleni grafu
 *  Autor: Adam Grunwald, xgrunw00
*/
 using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITU_EOP
{
    /// <summary>
    /// ViewModel pro Statistics View využívající NuGet balíček LiveCharts https://www.nuget.org/packages/LiveCharts/
    /// </summary>
    public class StatisticsViewModel : ViewModelBase
    {
        /// <summary>
        /// Třídy pro zobrazení hodnot v grafech
        /// </summary>
        public SeriesCollection appsCollection { get; set; }

        public SeriesCollection categoriesCollection { get; set; }


        readonly TimeSpan zeroState = new TimeSpan(0, 0, 0);

        /// <summary>
        /// Seznam aplikací v záznamu
        /// </summary>
        private ObservableCollection<Application> applications;
        public ObservableCollection<Application> Applications
        {
            get { return applications; }
            set
            {
                if (value != applications)
                {
                    applications = value;
                }
            }
        }

        /// <summary>
        /// Seznam kategorií v záznamu
        /// </summary>
        private Dictionary<string, Category> categories = new Dictionary<string, Category>();
        public Dictionary<string, Category> Categories
        {
            get { return categories; }
            set
            {
                if (value != categories)
                {
                    categories = value;
                }
            }
        }

        public StatisticsViewModel()
        {

            if (!categories.ContainsKey("Work") && !categories.ContainsKey("Fun") && !categories.ContainsKey("Other"))
            {
                categories.Add("Work", new Category("Work", new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 0), "Práce"));
                categories.Add("Fun", new Category("Fun", new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 0), "Zábava"));
                categories.Add("Other", new Category("Other", new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 0), "Ostatní"));
            }

            appsCollection = new SeriesCollection();

            categoriesCollection = new SeriesCollection();


            /// Poslouchání na různých druzích zpráv z jiných ViewModelů
            Messenger.Default.Register<SimpleMessage>(this, ConsumeMessage);

            /// Kopírování dat do funkcí pro popis grafů
            Messenger.Default.Register<ObservableCollection<Application>>(this, (apps) =>
            {
               
                foreach (KeyValuePair<string, Category> item in categories)
                {
                    item.Value.timeInCategory = zeroState;
                }


                appsCollection.Clear();
               
                applications = apps;

                foreach (Application item in applications)
                {
                    categories[item.Category.name].timeInCategory = categories[item.Category.name].timeInCategory.Add(item.timeInApp);

                    appsCollection.Add(new PieSeries
                    {
                        Values = new ChartValues<ObservableValue> { new ObservableValue(item.timeInApp.TotalSeconds) },
                        Title = item.name,
                    });
                }
                loadCategories();
            });
        }

        private void loadCategories()
        {
            categoriesCollection.Clear();
            foreach (KeyValuePair<string, Category> item in categories)
            {
                if(item.Value.timeInCategory == new TimeSpan(0,0,0))
                {
                    continue;
                }
                categoriesCollection.Add(new PieSeries
                {
                    Values = new ChartValues<ObservableValue> { new ObservableValue(item.Value.timeInCategory.TotalSeconds) },
                    Title = item.Value.translatedName,
                });
            }
        }

        /// <summary>
        /// Funkce která je vyvolaná messengerem pro smazání grafů, aby nebyly překreslovány dvakrát
        /// </summary>
        /// <param name="message"></param>
        private void ConsumeMessage(SimpleMessage message)
        {
            switch (message.Type)
            {
                case SimpleMessage.MessageType.SwitchToApplicationListView:
                case SimpleMessage.MessageType.SwitchToTargetsView:
                case SimpleMessage.MessageType.NotInStatisticsView:
                    appsCollection.Clear();
                    categoriesCollection.Clear();
                    break;
            }
        }
    }
}
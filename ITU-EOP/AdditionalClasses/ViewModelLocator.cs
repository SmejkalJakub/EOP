/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:ITU_test"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;


/// <summary>
/// NEAUTORSKÉ
/// Vytvoøeno NuGet balíèkem MVVMLight
/// </summary>
namespace ITU_EOP
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private static MainViewModel _main;
        private static ApplicationListViewModel _appList;
        private static TargetsViewModel _targets;
        private static StatisticsViewModel _statistics;



        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {

        }

        public MainViewModel Main
        {
            get
            {
                if (_main == null)
                {
                    _main = new MainViewModel();
                }
                return _main;
            }
        }

        public ApplicationListViewModel AppList
        {
            get
            {
                if (_appList == null)
                {
                    _appList = new ApplicationListViewModel();
                }
                return _appList;
            }
        }

        public TargetsViewModel Targets
        {
            get
            {
                if (_targets == null)
                {
                    _targets = new TargetsViewModel();
                }
                return _targets;
            }
        }

        public StatisticsViewModel Statistics
        {
            get
            {
                if (_statistics == null)
                {
                    _statistics = new StatisticsViewModel();
                }
                return _statistics;
            }
        }
    }
}
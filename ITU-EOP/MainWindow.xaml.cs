using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ITU_EOP
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
        }

        private void calendar_MouseLeave(object sender, MouseEventArgs e)
        {
            calendar.Visibility = Visibility.Hidden;
            if(calendar.SelectedDate == DateTime.Today)
            {
                Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.StartTimer });

            }
        }

        private void calendarButton_Click(object sender, RoutedEventArgs e)
        {
            calendar.Visibility = Visibility.Visible;
        }

        private void calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            Messenger.Default.Send(0);
            Messenger.Default.Send((DateTime)(calendar.SelectedDate), "token");

            Messenger.Default.Send((DateTime)(calendar.SelectedDate), "applicationToken");
            Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.StopTimer });

        }
    }
}

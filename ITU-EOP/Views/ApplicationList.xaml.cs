using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;

namespace ITU_EOP
{
    /// <summary>
    /// Interakční logika pro ApplicationList.xaml
    /// </summary>
    public partial class ApplicationList : UserControl
    {

        public ApplicationList()
        {
            InitializeComponent();

            Messenger.Default.Register<SimpleMessage>(this, ConsumeMessage);
        }

        private void ConsumeMessage(SimpleMessage message)
        {
            switch (message.Type)
            {
                case SimpleMessage.MessageType.TimerTick:
                    applicationGrid.Items.Refresh();
                    break;
            }
        }

    }
}

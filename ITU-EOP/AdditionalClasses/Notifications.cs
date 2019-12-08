/*
* ITU projekt - TimeTracker
* Třída využívající Notifikací z WinForms pro zobrazení upozornění při prokrastinaci či splnění cíle
* Jakub Smejkal, xsmejk28
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace ITU_EOP
{
    class Notfications
    {
        private readonly NotifyIcon _notifyIcon;

        public Notfications()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
        }

        public void ShowNotification(string time)
        {
            _notifyIcon.Visible = true;
            _notifyIcon.ShowBalloonTip(3000, "Prokrastinujete", "Nebyli jste aktivní v žádné pracovní aplikaci již: " + time, ToolTipIcon.Warning);
        }

        public void ShowNotificationCategory(string category)
        {
            _notifyIcon.Visible = true;
            _notifyIcon.ShowBalloonTip(3000, "Gratulujeme", "Dokončili jste váš cíl v kategorii " + category, ToolTipIcon.Info);
        }

    }
}


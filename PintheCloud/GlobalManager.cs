using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PintheCloud
{
    class GlobalManager
    {
        // Set progress indicator for background working
        public static void SetProgressIndicator(PhoneApplicationPage page, bool value, string text = "")
        {
            App.progressIndicator.IsIndeterminate = value;
            App.progressIndicator.IsVisible = value;
            App.progressIndicator.Text = text;
            SystemTray.SetProgressIndicator(page, App.progressIndicator);
        } 
    }
}

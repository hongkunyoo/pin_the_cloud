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
            GlobalObjects.ProgressIndicator.IsIndeterminate = value;
            GlobalObjects.ProgressIndicator.IsVisible = value;
            GlobalObjects.ProgressIndicator.Text = text;
            SystemTray.SetProgressIndicator(page, GlobalObjects.ProgressIndicator);
        } 
    }
}

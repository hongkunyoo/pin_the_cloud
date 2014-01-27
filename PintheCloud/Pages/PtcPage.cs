using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

using System.Net;
using System.Windows;
using System.Windows.Controls;
//using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PintheCloud.Resources;
//using System.Threading.Tasks;

namespace PintheCloud.Pages
{
    public partial class PtcPage : PhoneApplicationPage
    {
        public static string SPLASH_PAGE = "/Pages/SplashPage.xaml";
        public static string EXPLORER_PAGE = "/Pages/ExplorerPage.xaml";
        public static string SETTINGS_PAGE = "/Pages/SettingsPage.xaml";

        public PtcPage()
        {
        }

        public void SetProgressIndicator(bool value, string text = "")
        {
            App.ProgressIndicator.IsIndeterminate = value;
            App.ProgressIndicator.IsVisible = value;
            App.ProgressIndicator.Text = text;
            SystemTray.SetProgressIndicator(this, App.ProgressIndicator);
        }
        
    }
}

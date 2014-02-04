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
using PintheCloud.Models;
using Windows.Devices.Geolocation;
//using System.Threading.Tasks;

namespace PintheCloud.Pages
{
    public partial class PtcPage : PhoneApplicationPage
    {
        public static string SPLASH_PAGE = "/Pages/SplashPage.xaml";
        public static string EXPLORER_PAGE = "/Pages/ExplorerPage.xaml";
        public static string SETTINGS_PAGE = "/Pages/SettingsPage.xaml";
        public static string MAP_VIEW_PAGE = "/Pages/MapViewPage.xaml";
        public static string SKY_DRIVE_PICKER_PAGE = "/Pages/SkyDrivePickerPage.xaml";


        public PtcPage()
        {
        }

        public void SetSystemTray(bool value, double opacity = 1)
        {
            SystemTray.Opacity = opacity;
            SystemTray.IsVisible = value;
        }

        public void SetProgressIndicator(bool value, string text = "")
        {
            App.ProgressIndicator.IsIndeterminate = value;
            App.ProgressIndicator.IsVisible = value;
            App.ProgressIndicator.Text = text;
            SystemTray.SetProgressIndicator(this, App.ProgressIndicator);
        }

        public bool GetLocationAccessConsent()
        {
            bool locationAccess = false;
            App.ApplicationSettings.TryGetValue<bool>(Account.LOCATION_ACCESS, out locationAccess);
            if (!locationAccess)  // First or not consented of access in location information.
            {
                MessageBoxResult result = MessageBox.Show(AppResources.LocationAccessMessage, AppResources.LocationAccess, MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                    App.ApplicationSettings[Account.LOCATION_ACCESS] = true;
                else
                    App.ApplicationSettings[Account.LOCATION_ACCESS] = false;
                App.ApplicationSettings.Save();
            }

            return (bool)App.ApplicationSettings[Account.LOCATION_ACCESS];
        }

        public bool GetGeolocatorPositionStatus()
        { 
            Geolocator geolocator = new Geolocator();
            if (geolocator.LocationStatus != PositionStatus.Disabled)
                return true;
            else
                return false;
        }
    }
}

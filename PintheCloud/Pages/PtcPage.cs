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
using System.Text.RegularExpressions;
using Windows.UI;
using PintheCloud.Utilities;
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
        public static string FILE_LIST_PAGE = "/Pages/FileListPage.xaml";

        protected string PREVIOUS_PAGE;

        public PtcPage()
        {
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (PhoneApplicationService.Current.State.ContainsKey("PREV_PAGE"))
                this.PREVIOUS_PAGE = (string)PhoneApplicationService.Current.State["PREV_PAGE"];
            PhoneApplicationService.Current.State["PREV_PAGE"] = this.NavigationService.CurrentSource.ToString();
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        public static void SetProgressIndicator(DependencyObject context, bool value, string text = "")
        {
            ProgressIndicator progressIndicator = new ProgressIndicator();
            progressIndicator.IsIndeterminate = value;
            progressIndicator.IsVisible = value;
            progressIndicator.Text = text;
            SystemTray.SetProgressIndicator(context, progressIndicator);
        }


        public void SetListUnableAndShowMessage(LongListSelector list, string message, TextBlock messageTextBlock)
        {
            list.Visibility = Visibility.Collapsed;
            messageTextBlock.Text = message;
            messageTextBlock.Visibility = Visibility.Visible;
        }


        public bool GetLocationAccessConsent()
        {
            bool locationAccess = false;
            App.ApplicationSettings.TryGetValue<bool>(Account.LOCATION_ACCESS_CONSENT, out locationAccess);
            if (!locationAccess)  // First or not consented of access in location information.
            {
                MessageBoxResult result = MessageBox.Show(AppResources.LocationAccessMessage, AppResources.LocationAccess, MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                    App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT] = true;
                else
                    App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT] = false;
                App.ApplicationSettings.Save();
            }

            return (bool)App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT];
        }
    }
}

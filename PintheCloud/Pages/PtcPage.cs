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
using PintheCloud.ViewModels;
//using System.Threading.Tasks;

namespace PintheCloud.Pages
{
    public partial class PtcPage : PhoneApplicationPage
    {
        protected const string SPLASH_PAGE = "/Pages/SplashPage.xaml";
        protected const string EXPLORER_PAGE = "/Pages/ExplorerPage.xaml";
        protected const string SETTINGS_PAGE = "/Pages/SettingsPage.xaml";
        protected const string FILE_LIST_PAGE = "/Pages/FileListPage.xaml";
        protected const string PREV_PAGE = "PREV_PAGE";

        protected const string EDIT_IMAGE_URI = "/Assets/pajeon/png/general_edit.png";
        protected const string VIEW_IMAGE_URI = "/Assets/pajeon/png/general_view.png";
        protected const string DELETE_APP_BAR_BUTTON_ICON_URI = "/Assets/pajeon/png/general_bar_delete.png";

        protected string PREVIOUS_PAGE = null;


        public PtcPage()
        {
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (PhoneApplicationService.Current.State.ContainsKey(PREV_PAGE))
                this.PREVIOUS_PAGE = (string)PhoneApplicationService.Current.State[PREV_PAGE];
            PhoneApplicationService.Current.State[PREV_PAGE] = this.NavigationService.CurrentSource.ToString().Split('?')[0];
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        public void SetProgressIndicator(bool value, string text = "")
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                ProgressIndicator progressIndicator = new ProgressIndicator();
                progressIndicator.IsIndeterminate = value;
                progressIndicator.IsVisible = value;
                progressIndicator.Text = text;
                SystemTray.SetProgressIndicator(this, progressIndicator);
            });
        }


        public void SetListUnableAndShowMessage(LongListSelector list, string message, TextBlock messageTextBlock)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                list.Visibility = Visibility.Collapsed;
                messageTextBlock.Text = message;
                messageTextBlock.Visibility = Visibility.Visible;
            });
        }


        // Get parameters from given spot view item
        public string GetParameterStringFromSpotViewItem(SpotViewItem spotViewItem)
        {
            // Go to File List Page with parameters.
            string spotId = spotViewItem.SpotId;
            string spotName = spotViewItem.SpotName;
            string accountId = spotViewItem.AccountId;
            string accountName = spotViewItem.AccountName;
            string parameters = "?spotId=" + spotId + "&spotName=" + spotName + "&accountId=" + accountId + "&accountName=" + accountName;
            return parameters;
        }


        public int GetPlatformIndex(string platform)
        {
            for (int i = 0; i < App.IStorageManagers.Length; i++)
            {
                if (platform.Equals(App.IStorageManagers[i].GetStorageName()))
                {
                    return i;
                }
            }
            throw new Exception("No Such Storage Name");
        }
    }
}

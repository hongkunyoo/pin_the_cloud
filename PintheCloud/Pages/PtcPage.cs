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
using PintheCloud.Managers;
//using System.Threading.Tasks;

namespace PintheCloud.Pages
{
    public partial class PtcPage : PhoneApplicationPage
    {
        protected const string SPLASH_PAGE = "/Pages/SplashPage.xaml";
        protected const string EXPLORER_PAGE = "/Pages/ExplorerPage.xaml";
        protected const string SETTINGS_PAGE = "/Pages/SettingsPage.xaml";
        protected const string FILE_LIST_PAGE = "/Pages/FileListPage.xaml";
        protected const string PREV_PAGE_KEY = "PREV_PAGE";
        protected const string PIVOT_KEY = "PIVOT_KEY";
        protected const string PLATFORM_KEY = "PLATFORM_KEY";

        protected string PREVIOUS_PAGE = null;

        //protected int PIVOT = 0;
        //protected int PLATFORM = 0;
        
        public PtcPage()
        {
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (PhoneApplicationService.Current.State.ContainsKey(PREV_PAGE_KEY))
                this.PREVIOUS_PAGE = (string)PhoneApplicationService.Current.State[PREV_PAGE_KEY];
            PhoneApplicationService.Current.State[PREV_PAGE_KEY] = this.NavigationService.CurrentSource.ToString().Split('?')[0];
            string CURRENT_PAGE = (string)PhoneApplicationService.Current.State[PREV_PAGE_KEY];

            int pivot = 0;
            if (PhoneApplicationService.Current.State.ContainsKey(PIVOT_KEY))
                pivot = (int)PhoneApplicationService.Current.State[PIVOT_KEY];

            EventManager.FireEvent(CURRENT_PAGE, this.PREVIOUS_PAGE, pivot);
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

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


        // Get parameters from given space view item
        public string GetParameterStringFromSpaceViewItem(SpaceViewItem spaceViewItem)
        {
            // Go to File List Page with parameters.
            string spaceId = spaceViewItem.SpaceId;
            string spaceName = spaceViewItem.SpaceName;
            string accountId = spaceViewItem.AccountId;
            string accountName = spaceViewItem.AccountName;
            string parameters = "?spaceId=" + spaceId + "&spaceName=" + spaceName + "&accountId=" + accountId + "&accountName=" + accountName;
            return parameters;
        }
    }
}

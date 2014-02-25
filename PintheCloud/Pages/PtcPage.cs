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
        protected const string PREV_PAGE_KEY = "PREV_PAGE";
        protected const string PIVOT_KEY = "PIVOT_KEY";
        protected const string PLATFORM_KEY = "PLATFORM_KEY";

        protected const string SPOT_VIEW_MODEL_KEY = "SPOT_VIEW_MODEL_KEY";
        protected const string FILE_OBJECT_VIEW_MODEL_KEY = "FILE_OBJECT_VIEW_MODEL_KEY";

        protected const string SELECTED_FILE_KEY = "SELECTED_FILE_KEY";

        protected const string EDIT_IMAGE_URI = "/Assets/pajeon/png/general_edit.png";
        protected const string VIEW_IMAGE_URI = "/Assets/pajeon/png/general_view.png";
        protected const string DELETE_APP_BAR_BUTTON_ICON_URI = "/Assets/pajeon/png/general_bar_delete.png";


        public PtcPage()
        {
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Get previous page and set it.
            string previousPage = null;
            if (PhoneApplicationService.Current.State.ContainsKey(PREV_PAGE_KEY))
                previousPage = (string)PhoneApplicationService.Current.State[PREV_PAGE_KEY];
            PhoneApplicationService.Current.State[PREV_PAGE_KEY] = this.NavigationService.CurrentSource.ToString().Split('?')[0];

            // Get previous pivot.
            int previousPivot = 0;
            if (PhoneApplicationService.Current.State.ContainsKey(PIVOT_KEY))
                previousPivot = (int)PhoneApplicationService.Current.State[PIVOT_KEY];

            EventHelper.FireEvent((string)PhoneApplicationService.Current.State[PREV_PAGE_KEY], previousPage, previousPivot);
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
                if (platform.Equals(App.IStorageManagers[i].GetStorageName()))
                    return i;
            throw new Exception("No Such Storage Name");
        }
    }
}

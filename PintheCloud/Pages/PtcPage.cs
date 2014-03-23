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
using Microsoft.Phone.Shell;
using PintheCloud.Resources;
using PintheCloud.Models;
using Windows.Devices.Geolocation;
using System.Text.RegularExpressions;
using Windows.UI;
using PintheCloud.Utilities;
using PintheCloud.ViewModels;
using PintheCloud.Managers;
using PintheCloud.Helpers;

namespace PintheCloud.Pages
{
    public partial class PtcPage : PhoneApplicationPage
    {
        protected const string PICK_FILE_OBJECT_VIEW_MODEL_KEY = "PICK_FILE_OBJECT_VIEW_MODEL_KEY";
        protected const string PIN_FILE_OBJECT_VIEW_MODEL_KEY = "PIN_FILE_OBJECT_VIEW_MODEL_KEY";

        protected const string PREV_PAGE_KEY = "PREV_PAGE";
        protected const string PIVOT_KEY = "PIVOT_KEY";

        protected const string SPOT_VIEW_MODEL_KEY = "SPOT_VIEW_MODEL_KEY";
        protected const string SELECTED_FILE_KEY = "SELECTED_FILE_KEY";

        protected const string NULL_PASSWORD = "null";
        public const double STATUS_BAR_HEIGHT = 32.0;


        public PtcPage()
        {
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

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


        public void SetListUnableAndShowMessage(LongListSelector list, TextBlock messageTextBlock, string message)
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

        public void SetStorageBarMenuItem(out ApplicationBarMenuItem[] AppBarMenuItems, EventHandler AppBarMenuItem_Click)
        {
            List<IStorageManager> storageList = StorageHelper.GetStorageList();
            AppBarMenuItems = new ApplicationBarMenuItem[storageList.Count];
            for (var i = 0; i < storageList.Count; i++)
            {
                AppBarMenuItems[i] = new ApplicationBarMenuItem();
                AppBarMenuItems[i].Text = storageList[i].GetStorageName();
                AppBarMenuItems[i].Click += AppBarMenuItem_Click;
            }
        }


        public bool GetLocationAccessConsent()
        {
            if (!((bool)App.ApplicationSettings[StorageAccount.LOCATION_ACCESS_CONSENT_KEY]))  // First or not consented of access in location information.
            {
                MessageBoxResult result = MessageBox.Show(AppResources.LocationAccessMessage, AppResources.LocationAccessCaption, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                    App.ApplicationSettings[StorageAccount.LOCATION_ACCESS_CONSENT_KEY] = true;
                else
                    App.ApplicationSettings[StorageAccount.LOCATION_ACCESS_CONSENT_KEY] = false;
                App.ApplicationSettings.Save();
            }
            return (bool)App.ApplicationSettings[StorageAccount.LOCATION_ACCESS_CONSENT_KEY];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PintheCloud.Managers;
using PintheCloud.Workers;
using PintheCloud.Resources;
using System.Windows.Media.Imaging;
using System.Net.NetworkInformation;
using PintheCloud.ViewModels;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.WindowsAzure.MobileServices;
using PintheCloud.Models;
using Newtonsoft.Json.Linq;

namespace PintheCloud.Pages
{
    public partial class SettingsPage : PtcPage
    {
        // Const Instances
        private const string EDIT_IMAGE_URI = "/Assets/pajeon/png/general_edit.png";
        private const string VIEW_IMAGE_URI = "/Assets/pajeon/png/general_view.png";
        private const int APPLICATION_PIVOT = 0;
        private const int MY_SPOT_PIVOT = 1;

        // Instances
        private bool IsSignIning = false;
        public SpaceViewModel MySpotViewModel = new SpaceViewModel();


        public SettingsPage()
        {
            InitializeComponent();
        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);



            /*** Application Pivot ***/

            // Set Nickname
            string nickName = null;
            if (!App.ApplicationSettings.TryGetValue<string>(Account.ACCOUNT_NICK_NAME, out nickName))
            {
                App.ApplicationSettings[Account.ACCOUNT_NICK_NAME] = AppResources.AtHere;
                App.ApplicationSettings.Save();
            }
            uiSpotNickNameTextBox.Text = (string)App.ApplicationSettings[Account.ACCOUNT_NICK_NAME];

            // Set SkyDrive Sign button
            bool isSignIn = false;
            App.ApplicationSettings.TryGetValue<bool>(Account.ACCOUNT_IS_SIGN_IN_KEYS[GlobalKeys.SKY_DRIVE_LOCATION_KEY], out isSignIn);
            this.SetSkyDriveSignButton(isSignIn);

            // Set location access consent checkbox
            bool isLocationAccessConsent = false;
            App.ApplicationSettings.TryGetValue<bool>(Account.LOCATION_ACCESS_CONSENT, out isLocationAccessConsent);
            if (isLocationAccessConsent)
                uiLocationAccessConsentCheckBox.IsChecked = true;
            else
                uiLocationAccessConsentCheckBox.IsChecked = false;



            /*** My Spot Pivot ***/

            // If Internet available, Set space list
            if (NetworkInterface.GetIsNetworkAvailable())
                if (!MySpotViewModel.IsDataLoaded)  // Mutex check
                    await this.SetMySpotPivotAsync(AppResources.Loading);
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        // Construct pivot item by page index
        private void uiSettingPivot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Set View model for dispaly,
            // One time loading.
            switch (uiSettingPivot.SelectedIndex)
            {
                case MY_SPOT_PIVOT:

                    // Set My Spot stuff enable
                    uiMySpotEditViewButton.Visibility = Visibility.Visible;
                    ApplicationBar.IsVisible = true;
                    break;

                default:

                    // Set My Spot stuff enable
                    uiMySpotEditViewButton.Visibility = Visibility.Collapsed;
                    ApplicationBar.IsVisible = false;
                    break;
            }
        }



        /*** Application ***/

        private async void uiSkyDriveSignInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Set process indicator
            this.IsSignIning = true;
            uiApplicationGrid.Visibility = Visibility.Collapsed;
            uiApplicationMessageGrid.Visibility = Visibility.Visible;

            App.IStorageManager = App.IStorageManagers[GlobalKeys.SKY_DRIVE_LOCATION_KEY];
            if (await App.IStorageManager.SignIn(this))
                this.SetSkyDriveSignButton(true);
            else
                MessageBox.Show(AppResources.BadSignInMessage, AppResources.BadSignInCaption, MessageBoxButton.OK);

            // Hide process indicator
            uiApplicationGrid.Visibility = Visibility.Visible;
            uiApplicationMessageGrid.Visibility = Visibility.Collapsed;
            this.IsSignIning = false;
        }


        private void uiSkyDriveSignOutButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Sign out
            MessageBoxResult result = MessageBox.Show(AppResources.SignOutMessage, AppResources.SignOutCaption, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                App.IStorageManager = App.IStorageManagers[GlobalKeys.SKY_DRIVE_LOCATION_KEY];
                App.IStorageManager.SignOut();
                this.SetSkyDriveSignButton(false);
            }
        }


        private void SetSkyDriveSignButton(bool isSignIn)
        {
            if (isSignIn)  // It is signed in
            {
                uiSkyDriveSignButton.Content = AppResources.SkyDriveSignOut;
                uiSkyDriveSignButton.Click += uiSkyDriveSignOutButton_Click;
                uiSkyDriveSignButton.Click -= uiSkyDriveSignInButton_Click;
            }
            else  // It haven't signed in
            {
                uiSkyDriveSignButton.Content = AppResources.SkyDriveSignIn;
                uiSkyDriveSignButton.Click -= uiSkyDriveSignOutButton_Click;
                uiSkyDriveSignButton.Click += uiSkyDriveSignInButton_Click;
            }
        }


        private void uiSpotNickNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string nickName = uiSpotNickNameTextBox.Text;
            if (!nickName.Trim().Equals("")) 
            {
                if (nickName.Length > 0)
                    uiSpotNickNameSetButton.IsEnabled = true;
                else
                    uiSpotNickNameSetButton.IsEnabled = false;
            }
        }


        private void uiSpotNickNameSetButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[Account.ACCOUNT_NICK_NAME] = uiSpotNickNameTextBox.Text;
            App.ApplicationSettings.Save();
            MessageBox.Show(AppResources.SetSpotNickNameMessage, AppResources.SetSpotNickNameCaption, MessageBoxButton.OK);
        }


        // If it is in sign in, unable back key.
        // Otherwise, do normal back key.
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            
            if (this.IsSignIning)
                e.Cancel = true;
        }


        private void uiLocationAccessConsentCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT] = true;
            App.ApplicationSettings.Save();
        }

        private void uiLocationAccessConsentCheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings.Remove(Account.LOCATION_ACCESS_CONSENT);
        }


        private void uiSkyDriveSetMainButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE] = GlobalKeys.PLATFORMS[GlobalKeys.SKY_DRIVE_LOCATION_KEY];
            App.ApplicationSettings.Save();
            MessageBox.Show(AppResources.MainCloudChangeMessage, AppResources.MainCloudChangeCpation, MessageBoxButton.OK);
        }


        private void uiDropboxSetMainButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE] = GlobalKeys.PLATFORMS[GlobalKeys.DROPBOX_LOCATION_KEY];
            App.ApplicationSettings.Save();
            MessageBox.Show(AppResources.MainCloudChangeMessage, AppResources.MainCloudChangeCpation, MessageBoxButton.OK);
        }



        /*** My Spot ***/

        // List select event
        private void uiMySpotList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected Space View Item
            SpaceViewItem spaceViewItem = uiMySpotList.SelectedItem as SpaceViewItem;

            // Set selected item to null for next selection of list item. 
            uiMySpotList.SelectedItem = null;

            // If selected item isn't null, goto File list page.
            if (spaceViewItem != null)
            {
                string parameters = App.SpaceManager.GetParameterStringFromSpaceViewItem(spaceViewItem);
                NavigationService.Navigate(new Uri(PtcPage.FILE_LIST_PAGE + parameters, UriKind.Relative));
            }
        }


        // Refresh space list.
        private async void uiAppBarRefreshMenuItem_Click(object sender, System.EventArgs e)
        {
            // If Internet available, Set space list
            if (NetworkInterface.GetIsNetworkAvailable())
                await this.SetMySpotPivotAsync(AppResources.Refreshing);
            else
                base.SetListUnableAndShowMessage(uiMySpotList, AppResources.InternetUnavailableMessage, uiMySpotMessage);
        }


        private async Task SetMySpotPivotAsync(string message)
        {
            // Show progress indicator 
            base.SetListUnableAndShowMessage(uiMySpotList, message, uiMySpotMessage);
            PtcPage.SetProgressIndicator(this, true);

            // Before go load, set mutex to true.
            MySpotViewModel.IsDataLoaded = true;

            // If there is my spaces, Clear and Add spaces to list
            // Otherwise, Show none message.
            JArray spaces = await App.SpaceManager.GetMySpaceViewItemsAsync();

            if (spaces != null)  // There are my spaces
            {
                this.MySpotViewModel.SetItems(spaces);
                uiMySpotList.DataContext = this.MySpotViewModel;
                uiMySpotList.Visibility = Visibility.Visible;
                uiMySpotMessage.Visibility = Visibility.Collapsed;
            }
            else  // No my spaces
            {
                base.SetListUnableAndShowMessage(uiMySpotList, AppResources.NoMySpotMessage, uiMySpotMessage);
            }

            // Hide progress indicator
            PtcPage.SetProgressIndicator(this, false);
        }


        private void uiMySpotEditViewButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string currentEditViewMode = ((BitmapImage)uiMySpotEditViewButtonImage.Source).UriSource.ToString();
            if (currentEditViewMode.Equals(EDIT_IMAGE_URI))
                uiMySpotEditViewButtonImage.Source = new BitmapImage(new Uri(VIEW_IMAGE_URI, UriKind.Relative));
            else if (currentEditViewMode.Equals(VIEW_IMAGE_URI))
                uiMySpotEditViewButtonImage.Source = new BitmapImage(new Uri(EDIT_IMAGE_URI, UriKind.Relative));
        }
    }
}
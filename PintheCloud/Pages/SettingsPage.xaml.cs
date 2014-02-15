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

namespace PintheCloud.Pages
{
    public partial class SettingsPage : PtcPage
    {
        // Instances
        private const int APPLICATION_PIVOT = 0;
        private const int MY_SPOT_PIVOT = 1;

        private bool IsSignIning = false;
        public SpaceViewModel MySpaceViewModel = new SpaceViewModel();


        public SettingsPage()
        {
            InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        { 

        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {

        }


        // Construct pivot item by page index
        private async void uiSettingPivot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Set View model for dispaly,
            // One time loading.
            switch (uiSettingPivot.SelectedIndex)
            {
                case APPLICATION_PIVOT:

                    // Set My Spot stuff unable
                    uiCurrentCloudKindText.Visibility = Visibility.Collapsed;
                    ApplicationBar.IsVisible = false;

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
                    App.ApplicationSettings.TryGetValue<bool>(Account.ACCOUNT_SKY_DRIVE_IS_SIGN_IN, out isSignIn);
                    this.SetSkyDriveSignButton(isSignIn);

                    // Set location access consent checkbox
                    bool isLocationAccessConsent = false;
                    App.ApplicationSettings.TryGetValue<bool>(Account.LOCATION_ACCESS_CONSENT, out isLocationAccessConsent);
                    if (isLocationAccessConsent)
                        uiLocationAccessConsentCheckBox.IsChecked = true;
                    else 
                        uiLocationAccessConsentCheckBox.IsChecked = false;

                    break;

                case MY_SPOT_PIVOT:

                    // Set My Spot stuff enable
                    uiCurrentCloudKindText.Visibility = Visibility.Visible;
                    ApplicationBar.IsVisible = true;

                    // If Internet available, Set space list
                    if (NetworkInterface.GetIsNetworkAvailable())
                        if (!MySpaceViewModel.IsDataLoaded)  // Mutex check
                            await this.SetMySpacePivotAsync(AppResources.Loading);
                    break;

                default:

                    // Set My Spot stuff enable
                    uiCurrentCloudKindText.Visibility = Visibility.Collapsed;
                    ApplicationBar.IsVisible = false;
                    break;
            }
        }



        /*** Application ***/

        private async void uiSkyDriveSignInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.IsSignIning = true;
            uiApplicationGrid.Visibility = Visibility.Collapsed;
            uiApplicationMessageGrid.Visibility = Visibility.Visible;
            App.IStorageManager = App.SkyDriveManager;
            if (await App.IStorageManager.SignIn(this))
                this.SetSkyDriveSignButton(true);
            else
                MessageBox.Show(AppResources.BadSignInMessage, AppResources.BadSignInCaption, MessageBoxButton.OK);
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
                App.IStorageManager = App.SkyDriveManager;
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
            if (uiSpotNickNameTextBox.Text.Length > 0)
                uiSpotNickNameSetButton.IsEnabled = true;
            else
                uiSpotNickNameSetButton.IsEnabled = false;
        }


        private void uiSpotNickNameSetButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[Account.ACCOUNT_NICK_NAME] = uiSpotNickNameTextBox.Text;
            App.ApplicationSettings.Save();
            MessageBox.Show(AppResources.SetSpotNickNameMessage, AppResources.SetSpotNickNameCaption, MessageBoxButton.OK);
        }


        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            // If it is in sign in, unable back key.
            // Otherwise, do normal back key.
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



        /*** My Spot ***/

        // List select event
        private void uiMySpaceList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected Space View Item
            SpaceViewItem spaceViewItem = uiMySpaceList.SelectedItem as SpaceViewItem;

            // If selected item isn't null, goto File list page.
            if (spaceViewItem != null)
            {
                string parameters = App.SpaceManager.GetParameterStringFromSpaceViewItem(spaceViewItem);
                NavigationService.Navigate(new Uri(PtcPage.FILE_LIST_PAGE + parameters, UriKind.Relative));
            }

            // Set selected item to null for next selection of list item. 
            uiMySpaceList.SelectedItem = null;
        }


        // Refresh space list.
        private async void uiAppBarRefreshMenuItem_Click(object sender, System.EventArgs e)
        {
            // If Internet available, Set space list
            if (NetworkInterface.GetIsNetworkAvailable())
                await this.SetMySpacePivotAsync(AppResources.Loading);
        }


        private void uiAppBarRemoveButton_Click(object sender, System.EventArgs e)
        {
            // TODO: 여기에 구현된 이벤트 처리기를 추가하십시오.
        }


        private async Task SetMySpacePivotAsync(string message)
        {
            // Show progress indicator 
            base.SetListUnableAndShowMessage(uiMySpaceList, message, uiMySpaceMessage);
            PtcPage.SetProgressIndicator(this, true);

            // Before go load, set mutex to true.
            MySpaceViewModel.IsDataLoaded = true;

            // If there is my spaces, Clear and Add spaces to list
            // Otherwise, Show none message.
            MobileServiceCollection<Space, Space> spaces = await App.SpaceManager.GetMySpaceViewItemsAsync();

            if (spaces != null)  // There are my spaces
            {
                this.MySpaceViewModel.SetItems(spaces);
                uiMySpaceList.DataContext = this.MySpaceViewModel;
                uiMySpaceList.Visibility = Visibility.Visible;
                uiMySpaceMessage.Visibility = Visibility.Collapsed;
            }
            else  // No my spaces
            {
                base.SetListUnableAndShowMessage(uiMySpaceList, AppResources.NoMySpaceMessage, uiMySpaceMessage);
                MySpaceViewModel.IsDataLoaded = false;  // Mutex
            }

            // Hide progress indicator
            PtcPage.SetProgressIndicator(this, false);
        }


        private void skyDriveAppBarButton_Click(object sender, EventArgs e)
        {
            uiCurrentCloudKindText.Text = AppResources.SkyDrive;
        }


        private void dropboxAppBarButton_Click(object sender, EventArgs e)
        {
            uiCurrentCloudKindText.Text = AppResources.Dropbox;
        }
    }
}
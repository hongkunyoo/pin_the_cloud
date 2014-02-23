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
using PintheCloud.Utilities;

namespace PintheCloud.Pages
{
    public partial class SettingsPage : PtcPage
    {
        // Const Instances
        private const string EDIT_IMAGE_URI = "/Assets/pajeon/png/general_edit.png";
        private const string VIEW_IMAGE_URI = "/Assets/pajeon/png/general_view.png";

        private const int APPLICATION_PIVOT_INDEX = 0;
        private const int MY_SPOT_PIVOT_INDEX = 1;

        // Instances
        public SpaceViewModel MySpotViewModel = new SpaceViewModel();
        private Button[] SignButtons = null;


        public SettingsPage()
        {
            InitializeComponent();


            /*** Application Pivot ***/

            // Set Nickname
            uiSpotNickNameTextBox.Text = (string)App.ApplicationSettings[Account.ACCOUNT_NICK_NAME_KEY];


            // Set SkyDrive Sign button
            this.SignButtons = new Button[] { uiSkyDriveSignButton, uiDropboxSignButton, uiGoogleDriveSignButton };
            for(int i=0 ; i<App.IStorageManagers.Length ; i++)
            {
                bool isSignIn = false;
                IStorageManager iStorageManager = App.IStorageManagers[i];
                App.ApplicationSettings.TryGetValue<bool>(iStorageManager.GetAccountIsSignInKey(), out isSignIn);
                this.SetSignButton(i, isSignIn);
            }


            // Set location access consent checkbox
            bool isLocationAccessConsent = false;
            App.ApplicationSettings.TryGetValue<bool>(Account.LOCATION_ACCESS_CONSENT_KEY, out isLocationAccessConsent);
            if (isLocationAccessConsent)
                uiLocationAccessConsentCheckBox.IsChecked = true;
            else
                uiLocationAccessConsentCheckBox.IsChecked = false;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
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
                case MY_SPOT_PIVOT_INDEX:

                    // Set My Spot stuff enable
                    uiMySpotEditViewButton.Visibility = Visibility.Visible;
                    ApplicationBar.IsVisible = true;


                    // If Internet available, Set space list
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        if (!MySpotViewModel.IsDataLoaded)  // Mutex check
                            this.SetMySpotPivotAsync(AppResources.Loading);
                    }
                    else
                    {
                        base.SetListUnableAndShowMessage(uiMySpotList, AppResources.InternetUnavailableMessage, uiMySpotMessage);
                    }
                    break;

                default:

                    // Set My Spot stuff enable
                    uiMySpotEditViewButton.Visibility = Visibility.Collapsed;
                    ApplicationBar.IsVisible = false;
                    break;
            }
        }



        /*** Application ***/

        private async void SignInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Get index
                Button signoutButton = (Button)sender;
                int platformIndex = base.GetPlatformIndex(signoutButton.Content.ToString().Split(' ')[0]);


                // Set process indicator
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiApplicationGrid.Visibility = Visibility.Collapsed;
                    uiApplicationMessageGrid.Visibility = Visibility.Visible;
                });

                // Sign in and await that task.
                IStorageManager iStorageManager = App.IStorageManagers[platformIndex];
                App.TaskManager.AddSignInTask(iStorageManager.SignIn(), platformIndex);
                await App.TaskManager.WaitSignInTask(platformIndex);

                // If sign in success, set list.
                // Otherwise, show bad sign in message box.
                if (iStorageManager.GetAccount() != null)
                {
                    this.SetSignButton(platformIndex, true);
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(AppResources.BadSignInMessage, AppResources.BadSignInCaption, MessageBoxButton.OK);
                    });
                }

                // Hide process indicator
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiApplicationGrid.Visibility = Visibility.Visible;
                    uiApplicationMessageGrid.Visibility = Visibility.Collapsed;
                });
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private void SignOutButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Sign out
            MessageBoxResult result = MessageBox.Show(AppResources.SignOutMessage, AppResources.SignOutCaption, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                // Get index
                Button signoutButton = (Button)sender;
                int platformIndex = base.GetPlatformIndex(signoutButton.Content.ToString().Split(' ')[0]);

                IStorageManager iStorageManager = App.IStorageManagers[platformIndex];
                App.TaskManager.AddSignOutTask(this.SignOut(platformIndex, iStorageManager), platformIndex);
            }
        }


        private async Task SignOut(int platformIndex, IStorageManager iStorageManager)
        {
            // Set process indicator
            base.Dispatcher.BeginInvoke(() =>
            {
                uiApplicationGrid.Visibility = Visibility.Collapsed;
                uiApplicationMessageGrid.Visibility = Visibility.Visible;
                uiApplicationMessage.Text = AppResources.DoingSignOut;
            });


            // Delete application settings before work for good UX and Wait signin task
            App.ApplicationSettings.Remove(iStorageManager.GetAccountIsSignInKey());
            await App.TaskManager.WaitSignInTask(platformIndex);

            // Signout
            iStorageManager.SignOut();


            // Hide process indicator
            base.Dispatcher.BeginInvoke(() =>
            {
                uiApplicationGrid.Visibility = Visibility.Visible;
                uiApplicationMessageGrid.Visibility = Visibility.Collapsed;
                uiApplicationMessage.Text = AppResources.DoingSignIn;
                this.SetSignButton(platformIndex, false);
            });
        }


        private void SetSignButton(int platformIndex, bool isSignIn)
        {
            if (isSignIn)  // It is signed in
            {
                this.SignButtons[platformIndex].Content = Account.PLATFORM_NAMES[platformIndex] + " " + AppResources.SignOutCaption;
                this.SignButtons[platformIndex].Click += SignOutButton_Click;
                this.SignButtons[platformIndex].Click -= SignInButton_Click;
            }
            else  // It haven't signed in
            {
                this.SignButtons[platformIndex].Content = Account.PLATFORM_NAMES[platformIndex] + " " + AppResources.SignIn;
                this.SignButtons[platformIndex].Click -= SignOutButton_Click;
                this.SignButtons[platformIndex].Click += SignInButton_Click;
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
            App.ApplicationSettings[Account.ACCOUNT_NICK_NAME_KEY] = uiSpotNickNameTextBox.Text;
            App.ApplicationSettings.Save();
            MessageBox.Show(AppResources.SetSpotNickNameMessage, AppResources.SetSpotNickNameCaption, MessageBoxButton.OK);
        }


        private void uiLocationAccessConsentCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT_KEY] = true;
            App.ApplicationSettings.Save();
        }

        private void uiLocationAccessConsentCheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings.Remove(Account.LOCATION_ACCESS_CONSENT_KEY);
        }


        private void uiSkyDriveSetMainButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY] = Account.StorageAccountType.SKY_DRIVE;
            App.ApplicationSettings.Save();
            MessageBox.Show(AppResources.SkyDrive + AppResources.MainCloudChangeMessage, AppResources.MainCloudChangeCpation, MessageBoxButton.OK);
        }


        private void uiDropboxSetMainButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY] = Account.StorageAccountType.DROPBOX;
            App.ApplicationSettings.Save();
            MessageBox.Show(AppResources.Dropbox + AppResources.MainCloudChangeMessage, AppResources.MainCloudChangeCpation, MessageBoxButton.OK);
        }


        private void uiGoogleDriveSetMainButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY] = Account.StorageAccountType.GOOGLE_DRIVE;
            App.ApplicationSettings.Save();
            MessageBox.Show(AppResources.GoogleDrive + AppResources.MainCloudChangeMessage, AppResources.MainCloudChangeCpation, MessageBoxButton.OK);
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
                string parameters = base.GetParameterStringFromSpaceViewItem(spaceViewItem);
                NavigationService.Navigate(new Uri(FILE_LIST_PAGE + parameters + "&platform=" + 
                    ((int)App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY]), UriKind.Relative));
            }
        }


        // Refresh space list.
        private void uiAppBarRefreshMenuItem_Click(object sender, System.EventArgs e)
        {
            // If Internet available, Set space list
            if (NetworkInterface.GetIsNetworkAvailable())
                this.SetMySpotPivotAsync(AppResources.Refreshing);
            else
                base.SetListUnableAndShowMessage(uiMySpotList, AppResources.InternetUnavailableMessage, uiMySpotMessage);
        }


        private async void SetMySpotPivotAsync(string message)
        {
            // Show progress indicator 
            base.SetListUnableAndShowMessage(uiMySpotList, message, uiMySpotMessage);
            base.SetProgressIndicator(true);

            // Before go load, set mutex to true.
            MySpotViewModel.IsDataLoaded = true;

            // If there is my spaces, Clear and Add spaces to list
            // Otherwise, Show none message.
            JArray spaces = await App.SpaceManager.GetMySpaceViewItemsAsync();

            if (spaces != null)  // There are my spaces
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    this.MySpotViewModel.SetItems(spaces);
                    uiMySpotList.DataContext = this.MySpotViewModel;
                    uiMySpotList.Visibility = Visibility.Visible;
                    uiMySpotMessage.Visibility = Visibility.Collapsed;
                });
            }
            else  // No my spaces
            {
                base.SetListUnableAndShowMessage(uiMySpotList, AppResources.NoMySpotMessage, uiMySpotMessage);
            }

            // Hide progress indicator
            base.SetProgressIndicator(false);
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
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
        private const string DELETE_APP_BAR_BUTTON_ICON_URI = "/Assets/pajeon/png/general_bar_delete.png";

        private const string DELETING_SPOT_IMAGE_URI = "DELETING_SPOT_IMAGE_URI";
        private const string DELET_FAIL_IMAGE_URI = "DELET_FAIL_IMAGE_URI";

        private const int APPLICATION_PIVOT_INDEX = 0;
        private const int MY_SPOT_PIVOT_INDEX = 1;

        // Instances
        private ApplicationBarIconButton DeleteAppBarButton = new ApplicationBarIconButton();
        private SpotViewModel MySpotViewModel = new SpotViewModel();
        private List<SpotViewItem> SelectedSpot = new List<SpotViewItem>();
        private Button[] SignButtons = null;


        public SettingsPage()
        {
            InitializeComponent();


            /*** Application Pivot ***/

            // Set Nickname
            uiSpotNickNameTextBox.Text = (string)App.ApplicationSettings[Account.ACCOUNT_NICK_NAME_KEY];


            // Set OneDrive Sign button
            this.SignButtons = new Button[] { uiOneDriveSignButton, uiDropboxSignButton, uiGoogleDriveSignButton };
            for(int i=0 ; i<App.IStorageManagers.Length ; i++)
            {
                IStorageManager iStorageManager = App.IStorageManagers[i];
                this.SetSignButton(i, iStorageManager.IsSignIn());
            }


            // Set location access consent checkbox
            bool isLocationAccessConsent = false;
            App.ApplicationSettings.TryGetValue<bool>(Account.LOCATION_ACCESS_CONSENT_KEY, out isLocationAccessConsent);
            if (isLocationAccessConsent)
                uiLocationAccessConsentCheckBox.IsChecked = true;
            else
                uiLocationAccessConsentCheckBox.IsChecked = false;



            /*** My Spot Pivot ***/

            // Set delete app bar button
            this.DeleteAppBarButton.Text = AppResources.Pin;
            this.DeleteAppBarButton.IconUri = new Uri(DELETE_APP_BAR_BUTTON_ICON_URI, UriKind.Relative);
            this.DeleteAppBarButton.IsEnabled = false;
            this.DeleteAppBarButton.Click += DeleteAppBarButton_Click;
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


                    // If Internet available, Set spot list
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
            //App.ApplicationSettings.Remove(iStorageManager.GetAccountIsSignInKey());
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
                this.SignButtons[platformIndex].Content = App.IStorageManagers[platformIndex].GetStorageName() + " " + AppResources.SignOutCaption;
                this.SignButtons[platformIndex].Click += SignOutButton_Click;
                this.SignButtons[platformIndex].Click -= SignInButton_Click;
            }
            else  // It haven't signed in
            {
                this.SignButtons[platformIndex].Content = App.IStorageManagers[platformIndex].GetStorageName() + " " + AppResources.SignIn;
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


        private void uiOneDriveSetMainButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY] = Account.StorageAccountType.ONE_DRIVE;
            App.ApplicationSettings.Save();
            MessageBox.Show(AppResources.OneDrive + AppResources.MainCloudChangeMessage, AppResources.MainCloudChangeCpation, MessageBoxButton.OK);
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
            // Get Selected Spot View Item
            SpotViewItem spotViewItem = uiMySpotList.SelectedItem as SpotViewItem;

            // Set selected item to null for next selection of list item. 
            uiMySpotList.SelectedItem = null;

            // If selected item isn't null, goto File list page.
            if (spotViewItem != null)
            {
                string parameters = base.GetParameterStringFromSpotViewItem(spotViewItem);
                NavigationService.Navigate(new Uri(FILE_LIST_PAGE + parameters + "&platform=" + 
                    ((int)App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY]), UriKind.Relative));
            }
        }


        // Refresh spot list.
        private void uiAppBarRefreshButton_Click(object sender, System.EventArgs e)
        {
            // If Internet available, Set spot list
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

            // If there is my spots, Clear and Add spots to list
            // Otherwise, Show none message.
            JArray spots = await App.SpotManager.GetMySpotViewItemsAsync();

            if (spots != null)  // There are my spots
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    this.MySpotViewModel.SetItems(spots);
                    uiMySpotList.DataContext = this.MySpotViewModel;
                    uiMySpotList.Visibility = Visibility.Visible;
                    uiMySpotMessage.Visibility = Visibility.Collapsed;
                });
            }
            else  // No my spots
            {
                base.SetListUnableAndShowMessage(uiMySpotList, AppResources.NoMySpotMessage, uiMySpotMessage);
            }

            // Hide progress indicator
            base.SetProgressIndicator(false);
        }


        private void uiMySpotEditViewButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Change edit view mode
            string currentEditViewMode = ((BitmapImage)uiMySpotEditViewButtonImage.Source).UriSource.ToString();
            if (currentEditViewMode.Equals(VIEW_IMAGE_URI))  // To View mode
            {
                // Change mode image and remove app bar buttons.
                if (this.SelectedSpot.Count > 0)
                {
                    this.SelectedSpot.Clear();
                    this.DeleteAppBarButton.IsEnabled = false;
                }
                ApplicationBar.Buttons.Remove(this.DeleteAppBarButton);
                uiMySpotEditViewButtonImage.Source = new BitmapImage(new Uri(EDIT_IMAGE_URI, UriKind.Relative));

                // Change select check image of each file object view item.
                foreach (SpotViewItem spotViewItem in this.MySpotViewModel.Items)
                {
                    if (spotViewItem.SelectCheckImage.Equals(FileObjectViewModel.CHECK_IMAGE_URI)
                        || spotViewItem.SelectCheckImage.Equals(FileObjectViewModel.CHECK_NOT_IMAGE_URI))
                        spotViewItem.SelectCheckImage = FileObjectViewModel.TRANSPARENT_IMAGE_URI;
                }
            }

            else if (currentEditViewMode.Equals(EDIT_IMAGE_URI))  // To Edit mode
            {
                // Change mode image and remove app bar buttons.
                ApplicationBar.Buttons.Add(this.DeleteAppBarButton);
                uiMySpotEditViewButtonImage.Source = new BitmapImage(new Uri(VIEW_IMAGE_URI, UriKind.Relative));

                // Change select check image of each file object view item.
                foreach (SpotViewItem spotViewItem in this.MySpotViewModel.Items)
                {
                    if (spotViewItem.SelectCheckImage.Equals(FileObjectViewModel.TRANSPARENT_IMAGE_URI))
                        spotViewItem.SelectCheckImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                }
            }
        }


        // Delete spots.
        private void DeleteAppBarButton_Click(object sender, EventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                foreach (SpotViewItem spotViewItem in this.SelectedSpot)
                    this.DeleteSpotAsync(spotViewItem);
                this.SelectedSpot.Clear();
                this.DeleteAppBarButton.IsEnabled = false;
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private void DeleteSpotAsync(SpotViewItem spotViewItem)
        {
            // Show Deleting message
            base.SetProgressIndicator(true);
            base.Dispatcher.BeginInvoke(() =>
            {
                spotViewItem.SelectCheckImage = DELETING_SPOT_IMAGE_URI;
            });


            // TODODelete


            // Hide Progress Indicator
            // If file list is empty, show empty message.
            base.SetProgressIndicator(false);
            if (this.MySpotViewModel.Items.Count - 1 < 1)
                base.SetListUnableAndShowMessage(uiMySpotList, AppResources.NoFileInSpotMessage, uiMySpotMessage);
        }
    }
}
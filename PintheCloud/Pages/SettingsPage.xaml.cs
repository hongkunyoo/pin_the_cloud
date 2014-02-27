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
                this.SetSignButton(i, App.IStorageManagers[i].IsSignIn());


            // Set location access consent checkbox
            if ((bool)App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT_KEY])
                uiLocationAccessConsentToggleSwitchButton.IsChecked = true;
            else
                uiLocationAccessConsentToggleSwitchButton.IsChecked = false;



            /*** My Spot Pivot ***/

            // Set delete app bar button
            this.DeleteAppBarButton.Text = AppResources.Pin;
            this.DeleteAppBarButton.IconUri = new Uri(DELETE_APP_BAR_BUTTON_ICON_URI, UriKind.Relative);
            this.DeleteAppBarButton.IsEnabled = false;
            this.DeleteAppBarButton.Click += DeleteAppBarButton_Click;

            // Set datacontext
            uiMySpotList.DataContext = this.MySpotViewModel;
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

                    // Set My Spot stuff enable and set list
                    uiMySpotEditViewButton.Visibility = Visibility.Visible;
                    ApplicationBar.IsVisible = true;
                    this.SetMySpotPivot();
                    break;

                default:

                    // Set My Spot stuff unable
                    uiMySpotEditViewButton.Visibility = Visibility.Collapsed;
                    ApplicationBar.IsVisible = false;
                    break;
            }
        }

        private void SetMySpotPivot()
        {
            // If Internet available, Set spot list
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                if (!MySpotViewModel.IsDataLoaded)  // Mutex check
                    this.SetMySpotListAsync(AppResources.Loading);
            }
            else
            {
                base.SetListUnableAndShowMessage(uiMySpotList, AppResources.InternetUnavailableMessage, uiMySpotMessage);
            }
        }

        /*** Application ***/

        private async void SignInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Set process indicator and Get index
                int platformIndex = 0;
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiProfileGrid.Visibility = Visibility.Collapsed;
                    uiProfileMessageGrid.Visibility = Visibility.Visible;
                    uiProfileMessage.Text = AppResources.DoingSignIn;

                    Button signoutButton = (Button)sender;
                    platformIndex = base.GetPlatformIndex(signoutButton.Content.ToString().Split(' ')[0]);
                });

                // Sign in and await that task.
                IStorageManager iStorageManager = App.IStorageManagers[platformIndex];                
                if (!iStorageManager.IsSigningIn())
                    App.TaskHelper.AddSignInTask(iStorageManager.GetStorageName(), iStorageManager.SignIn());
                bool result = await App.TaskHelper.WaitSignInTask(iStorageManager.GetStorageName());

                // If sign in success, set list.
                // Otherwise, show bad sign in message box.
                if (result)
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        this.SetSignButton(platformIndex, true);    
                    });
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
                    uiProfileGrid.Visibility = Visibility.Visible;
                    uiProfileMessageGrid.Visibility = Visibility.Collapsed;
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
            MessageBoxResult signOutResult = MessageBox.Show(AppResources.SignOutMessage, AppResources.SignOutCaption, MessageBoxButton.OKCancel);
            if (signOutResult == MessageBoxResult.OK)
            {
                // Set process indicator and get index
                uiProfileGrid.Visibility = Visibility.Collapsed;
                uiProfileMessageGrid.Visibility = Visibility.Visible;
                uiProfileMessage.Text = AppResources.DoingSignOut;
                Button signoutButton = (Button)sender;
                int platformIndex = base.GetPlatformIndex(signoutButton.Content.ToString().Split(' ')[0]);

                // Sign out
                IStorageManager iStorageManager = App.IStorageManagers[platformIndex];
                App.TaskHelper.AddSignOutTask(iStorageManager.GetStorageName(), this.SignOut(iStorageManager));

                // Hide process indicator
                ((FileObjectViewModel)PhoneApplicationService.Current.State[FILE_OBJECT_VIEW_MODEL_KEY]).IsDataLoaded = false;
                uiProfileGrid.Visibility = Visibility.Visible;
                uiProfileMessageGrid.Visibility = Visibility.Collapsed;
                this.SetSignButton(platformIndex, false);
            }
        }


        private async Task SignOut(IStorageManager iStorageManager)
        {
            bool result = await App.TaskHelper.WaitSignInTask(iStorageManager.GetStorageName());
            iStorageManager.SignOut();
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
            if (uiSpotNickNameTextBox.Text.Trim().Length > 0)
                uiSpotNickNameSetButton.IsEnabled = true;
            else
                uiSpotNickNameSetButton.IsEnabled = false;
        }


        private void uiSpotNickNameSetButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            uiSpotNickNameTextBox.Text = uiSpotNickNameTextBox.Text.Trim();
            App.ApplicationSettings[Account.ACCOUNT_NICK_NAME_KEY] = uiSpotNickNameTextBox.Text;
            App.ApplicationSettings.Save();
            MessageBox.Show(AppResources.SetSpotNickNameMessage, uiSpotNickNameTextBox.Text, MessageBoxButton.OK);
        }


        private void uiLocationAccessConsentToggleSwitchButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT_KEY] = true;
            App.ApplicationSettings.Save();
        }


        private void uiLocationAccessConsentToggleSwitchButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT_KEY] = false;
            App.ApplicationSettings.Save();
        }


        private void uiOneDriveSetMainButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY] = Account.StorageAccountType.ONE_DRIVE;
            App.ApplicationSettings.Save();
            MessageBox.Show(AppResources.SetMainCloudMessage, AppResources.OneDrive, MessageBoxButton.OK);
        }


        private void uiDropboxSetMainButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY] = Account.StorageAccountType.DROPBOX;
            App.ApplicationSettings.Save();
            MessageBox.Show(AppResources.SetMainCloudMessage, AppResources.Dropbox, MessageBoxButton.OK);
        }


        private void uiGoogleDriveSetMainButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY] = Account.StorageAccountType.GOOGLE_DRIVE;
            App.ApplicationSettings.Save();
            MessageBox.Show(AppResources.SetMainCloudMessage, AppResources.GoogleDrive, MessageBoxButton.OK);
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
                // If it is view mode, click is preview.
                // If it is edit mode, click is selection.
                string currentEditViewMode = ((BitmapImage)uiMySpotEditViewButtonImage.Source).UriSource.ToString();
                if (currentEditViewMode.Equals(EDIT_IMAGE_URI))  // View mode
                {
                    if (spotViewItem.SelectCheckImage.Equals(FileObjectViewModel.TRANSPARENT_IMAGE_URI))
                    {
                        string parameters = base.GetParameterStringFromSpotViewItem(spotViewItem);
                        NavigationService.Navigate(new Uri(EventHelper.FILE_LIST_PAGE + parameters + "&platform=" +
                            ((int)App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY]), UriKind.Relative));
                    }
                }
                else if (currentEditViewMode.Equals(VIEW_IMAGE_URI))  // Edit mode
                {
                    if (spotViewItem.SelectCheckImage.Equals(FileObjectViewModel.CHECK_NOT_IMAGE_URI))
                    {
                        this.SelectedSpot.Add(spotViewItem);
                        spotViewItem.SelectCheckImage = FileObjectViewModel.CHECK_IMAGE_URI;
                        this.DeleteAppBarButton.IsEnabled = true;
                    }

                    else if (spotViewItem.SelectCheckImage.Equals(FileObjectViewModel.CHECK_IMAGE_URI))
                    {
                        this.SelectedSpot.Remove(spotViewItem);
                        spotViewItem.SelectCheckImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                        if (this.SelectedSpot.Count < 1)
                            this.DeleteAppBarButton.IsEnabled = false;
                    }
                }
            }
        }


        // Refresh spot list.
        private void uiAppBarRefreshButton_Click(object sender, System.EventArgs e)
        {
            // If Internet available, Set spot list
            if (NetworkInterface.GetIsNetworkAvailable())
                this.SetMySpotListAsync(AppResources.Refreshing);
            else
                base.SetListUnableAndShowMessage(uiMySpotList, AppResources.InternetUnavailableMessage, uiMySpotMessage);
        }


        private async void SetMySpotListAsync(string message)
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
                    uiMySpotList.Visibility = Visibility.Visible;
                    uiMySpotMessage.Visibility = Visibility.Collapsed;

                    string currentEditViewMode = ((BitmapImage)uiMySpotEditViewButtonImage.Source).UriSource.ToString();
                    if (currentEditViewMode.Equals(EDIT_IMAGE_URI))  // View Mode
                        this.MySpotViewModel.SetItems(spots, false);
                    else if (currentEditViewMode.Equals(VIEW_IMAGE_URI))  // Edit Mode
                        this.MySpotViewModel.SetItems(spots, true);
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
                MessageBoxResult result = MessageBox.Show(AppResources.DeleteSpotMessage, AppResources.DeleteCaption, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    foreach (SpotViewItem spotViewItem in this.SelectedSpot)
                        this.DeleteSpotAsync(spotViewItem);
                    this.SelectedSpot.Clear();
                    this.DeleteAppBarButton.IsEnabled = false;
                }
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private async void DeleteSpotAsync(SpotViewItem spotViewItem)
        {
            // Show Deleting message
            bool deleteFileSuccess = true;
            base.SetProgressIndicator(true);
            base.Dispatcher.BeginInvoke(() =>
            {
                spotViewItem.SelectCheckImage = FileObjectViewModel.DELETING_IMAGE_URI;
            });

            // Delete
            List<FileObject> fileList = await App.BlobStorageManager.GetFilesFromSpotAsync(spotViewItem.AccountId, spotViewItem.SpotId);
            if (fileList.Count > 0)
            {
                foreach (FileObject fileObject in fileList)
                {
                    if (!await App.BlobStorageManager.DeleteFileAsync(fileObject.Id))
                        deleteFileSuccess = false;
                }
            }

            // If delete job success to all files, delete spot.
            // Otherwise, show delete fail image.
            if (deleteFileSuccess)
            {
                Spot spot = new Spot(spotViewItem.SpotName, 0, 0, spotViewItem.AccountId, spotViewItem.AccountName, spotViewItem.SpotDistance);
                spot.id = spotViewItem.SpotId;
                if (await App.SpotManager.DeleteSpotAsync(spot))
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        this.MySpotViewModel.Items.Remove(spotViewItem);
                        ((SpotViewModel)PhoneApplicationService.Current.State[SPOT_VIEW_MODEL_KEY]).IsDataLoaded = false;
                        if (this.MySpotViewModel.Items.Count < 1)
                            base.SetListUnableAndShowMessage(uiMySpotList, AppResources.NoFileInSpotMessage, uiMySpotMessage);
                    });
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        spotViewItem.SelectCheckImage = FileObjectViewModel.FAIL_IMAGE_URI;
                    });
                }
            }
            else
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    spotViewItem.SelectCheckImage = FileObjectViewModel.FAIL_IMAGE_URI;
                });
            }

            // Hide Progress Indicator
            base.SetProgressIndicator(false);
        }

        private async Task<bool> DeleteFilesAsync(List<FileObject> fileList)
        {
            bool deleteFileSuccess = true;
            foreach (FileObject fileObject in fileList)
            {
                if (!await App.BlobStorageManager.DeleteFileAsync(fileObject.Id))
                    deleteFileSuccess = false;
            }
            return deleteFileSuccess;
        }
    }
}
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
using System.Diagnostics;
using PintheCloud.Converters;
using System.Windows.Media;

namespace PintheCloud.Pages
{
    public partial class SettingsPage : PtcPage
    {
        // Const Instances
        private const int APPLICATION_PIVOT_INDEX = 0;
        private const int MY_SPOT_PIVOT_INDEX = 1;
        private const int DELETE_APP_BAR_BUTTON_INDEX = 1;

        private const string SIGN_IN_BUTTON_TEXT_FONT = "Segoe WP";
        private const string SIGN_NOT_IN_BUTTON_TEXT_FONT = "Segoe WP Light";

        private const string SIGN_IN_BUTTON_TEXT_COLOR = "404041";
        private const string SIGN_NOT_IN_BUTTON_TEXT_COLOR = "919FA6";

        private const double MAIN_PLATFORM_BUTTON_OPACITY = 0.2;
        private const double MAIN_NOT_PLATFORM_BUTTON_OPACITY = 0.8;

        private const string MAIN_PLATFORM_BUTTON_COLOR = "00a4bf";
        private const string MAIN_NOT_PLATFORM_BUTTON_COLOR = "e6e7e8";

        private const string SETTING_ACCOUNT_MAIN_CHECK_NOT_IMAGE_URI = "/Assets/pajeon/at_here/png/general_checkbox.png";
        private const string SETTING_ACCOUNT_MAIN_CHECK_IMAGE_URI = "/Assets/pajeon/at_here/png/general_checkbox_p.png";

        // Instances
        private ApplicationBarIconButton DeleteAppBarButton = new ApplicationBarIconButton();
        private SpotViewModel MySpotViewModel = new SpotViewModel();
        private Button[] SignButtons = null;
        private Button[] MainButtons = null;
        private Grid[] SignButtonGrids = null;
        private TextBlock[] SignButtonTextBlocks = null;
        private bool DeleteSpotButton = false;


        public SettingsPage()
        {
            InitializeComponent();


            /*** Application Pivot ***/

            // Set name
            uiDefaultSpotNameTextBox.Text = (string)App.ApplicationSettings[Account.ACCOUNT_DEFAULT_SPOT_NAME_KEY];

            // Set UIs
            this.SignButtons = new Button[] { uiOneDriveSignButton, uiDropboxSignButton, uiGoogleDriveSignButton };
            this.MainButtons = new Button[] { uiOneDriveMainButton, uiDropboxMainButton, uiGoogleDriveMainButton };
            this.SignButtonGrids = new Grid[] { uiOneDriveSignButtonGrid, uiDropboxSignButtonGrid, uiGoogleDriveSignButtonGrid };
            this.SignButtonTextBlocks = new TextBlock[] { uiOneDriveSignButtonText, uiDropboxSignButtonText, uiGoogleDriveSignButtonText };

            // Set location access consent checkbox
            if ((bool)App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT_KEY])
                uiLocationAccessConsentToggleSwitchButton.IsChecked = true;
            else
                uiLocationAccessConsentToggleSwitchButton.IsChecked = false;



            /*** My Spot Pivot ***/

            // Set delete app bar button and datacontext
            this.DeleteAppBarButton = ((ApplicationBarIconButton)ApplicationBar.Buttons[DELETE_APP_BAR_BUTTON_INDEX]);
            uiMySpotList.DataContext = this.MySpotViewModel;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Set Sign buttons and Set Main buttons.
            for (int i = 0; i < App.IStorageManagers.Length; i++)
            {
                this.SetSignButtons(i, App.IStorageManagers[i].IsSignIn());
                this.SetMainButtons(i);
            }
                

            // Set My Spot pivot list.
            this.SetMySpotPivot();
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
                    ApplicationBar.IsVisible = true;
                    this.SetMySpotPivot();
                    break;

                default:

                    // Set My Spot stuff unable
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
                // Set process indicator 
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiProfileGrid.Visibility = Visibility.Collapsed;
                    uiProfileMessageGrid.Visibility = Visibility.Visible;
                    uiProfileMessage.Text = AppResources.DoingSignIn;
                });

                // Get index
                Button signButton = (Button)sender;
                int platformIndex = base.GetPlatformIndexFromString(signButton.Tag.ToString());

                // Sign in
                IStorageManager iStorageManager = App.IStorageManagers[platformIndex];                
                if (!iStorageManager.IsSigningIn())
                    App.TaskHelper.AddSignInTask(iStorageManager.GetStorageName(), iStorageManager.SignIn());

                // If sign in success, set list.
                // Otherwise, show bad sign in message box.
                if (await App.TaskHelper.WaitSignInTask(iStorageManager.GetStorageName()))
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        this.SetSignButtons(platformIndex, true);
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
                Button signButton = (Button)sender;
                int platformIndex = base.GetPlatformIndexFromString(signButton.Tag.ToString());

                // Sign out
                IStorageManager iStorageManager = App.IStorageManagers[platformIndex];
                App.TaskHelper.AddSignOutTask(iStorageManager.GetStorageName(), this.SignOut(iStorageManager));

                // Hide process indicator
                ((FileObjectViewModel)PhoneApplicationService.Current.State[FILE_OBJECT_VIEW_MODEL_KEY]).IsDataLoaded = false;
                uiProfileGrid.Visibility = Visibility.Visible;
                uiProfileMessageGrid.Visibility = Visibility.Collapsed;
                this.SetSignButtons(platformIndex, false);
            }
        }


        private async Task SignOut(IStorageManager iStorageManager)
        {
            if(await App.TaskHelper.WaitSignInTask(iStorageManager.GetStorageName()))
                iStorageManager.SignOut();
        }


        private async void SetSignButtons(int platformIndex, bool isSignIn)
        {
            if (isSignIn)  // It is signed in
            {
                if (await App.TaskHelper.WaitSignInTask(App.IStorageManagers[platformIndex].GetStorageName()))
                {
                    this.SignButtonTextBlocks[platformIndex].Text = App.IStorageManagers[platformIndex].GetAccount().account_name;
                    this.SignButtonTextBlocks[platformIndex].Foreground = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(SIGN_IN_BUTTON_TEXT_COLOR));
                    this.SignButtonTextBlocks[platformIndex].FontFamily = new FontFamily(SIGN_IN_BUTTON_TEXT_FONT);
                    this.SignButtons[platformIndex].Click += this.SignOutButton_Click;
                    this.SignButtons[platformIndex].Click -= this.SignInButton_Click;
                    return;
                }
            }

            // It haven't signed in
            this.SignButtonTextBlocks[platformIndex].Text = AppResources.SignIn;
            this.SignButtonTextBlocks[platformIndex].Foreground = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(SIGN_NOT_IN_BUTTON_TEXT_COLOR));
            this.SignButtonTextBlocks[platformIndex].FontFamily = new FontFamily(SIGN_NOT_IN_BUTTON_TEXT_FONT);
            this.SignButtons[platformIndex].Click -= this.SignOutButton_Click;
            this.SignButtons[platformIndex].Click += this.SignInButton_Click;
        }


        private void SetMainButtons(int platformIndexindex)
        {
            // Set main button click event and image.
            Button mainButton = this.MainButtons[platformIndexindex];
            mainButton.Click += this.MainButton_Click;

            Image mainButtonImage = (Image)mainButton.Content;
            Grid signButtonGrid = this.SignButtonGrids[platformIndexindex];
            if (platformIndexindex == (int)App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY])
            {
                mainButtonImage.Source = new BitmapImage(new Uri(SETTING_ACCOUNT_MAIN_CHECK_IMAGE_URI, UriKind.Relative));
                signButtonGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(MAIN_PLATFORM_BUTTON_COLOR));
                signButtonGrid.Opacity = MAIN_PLATFORM_BUTTON_OPACITY;
            }
            else
            {
                mainButtonImage.Source = new BitmapImage(new Uri(SETTING_ACCOUNT_MAIN_CHECK_NOT_IMAGE_URI, UriKind.Relative));
                signButtonGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(MAIN_NOT_PLATFORM_BUTTON_COLOR));
                signButtonGrid.Opacity = MAIN_NOT_PLATFORM_BUTTON_OPACITY;
            }
        }


        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            // Set check image
            Button mainButton = (Button)sender;
            ((Image)mainButton.Content).Source = new BitmapImage(new Uri(SETTING_ACCOUNT_MAIN_CHECK_IMAGE_URI, UriKind.Relative));
            
            // Set Signbutton background
            int mainButtonPlatformIndex = base.GetPlatformIndexFromString(mainButton.Tag.ToString());
            Grid signButtonGrid = this.SignButtonGrids[mainButtonPlatformIndex];
            signButtonGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(MAIN_PLATFORM_BUTTON_COLOR));
            signButtonGrid.Opacity = MAIN_PLATFORM_BUTTON_OPACITY;

            // Save main platform index to app settings.
            App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY] = base.GetStorageAccountTypeFromInt(mainButtonPlatformIndex);
            App.ApplicationSettings.Save();

            // Set rest button image and backtround
            for (int i = 0; i < App.IStorageManagers.Length; i++)
            {
                if (i != mainButtonPlatformIndex)
                {
                    ((Image)this.MainButtons[i].Content).Source = new BitmapImage(new Uri(SETTING_ACCOUNT_MAIN_CHECK_NOT_IMAGE_URI, UriKind.Relative));

                    Grid restSignButtonGrid = (Grid)this.SignButtonGrids[i];
                    restSignButtonGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(MAIN_NOT_PLATFORM_BUTTON_COLOR));
                    restSignButtonGrid.Opacity = MAIN_NOT_PLATFORM_BUTTON_OPACITY;
                }
            }
        }


        private void uiDefaultSpotNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (uiDefaultSpotNameTextBox.Text.Trim().Length > 0)
                uiDefaultSpotNameSetButton.IsEnabled = true;
            else
                uiDefaultSpotNameSetButton.IsEnabled = false;
        }

        private void uiDefaultSpotNameTextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            uiDefaultSpotNameSetButton.Visibility = Visibility.Visible;
        }

        private void uiDefaultSpotNameTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            uiDefaultSpotNameSetButton.Visibility = Visibility.Collapsed;
        }

        private void uiDefaultSpotNameSetButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            uiDefaultSpotNameTextBox.Text = uiDefaultSpotNameTextBox.Text.Trim();
            App.ApplicationSettings[Account.ACCOUNT_DEFAULT_SPOT_NAME_KEY] = uiDefaultSpotNameTextBox.Text;
            App.ApplicationSettings.Save();
            MessageBox.Show(AppResources.SetDefaultSpotNameMessage, uiDefaultSpotNameTextBox.Text, MessageBoxButton.OK);
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
                if (!this.DeleteSpotButton)
                {
                    if (spotViewItem.DeleteImage.Equals(FileObjectViewModel.DELETE_IMAGE_URI))
                    {
                        string parameters = base.GetParameterStringFromSpotViewItem(spotViewItem);
                        NavigationService.Navigate(new Uri(EventHelper.FILE_LIST_PAGE + parameters + "&platform=" +
                            ((int)App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY]), UriKind.Relative));
                    }
                }
                else
                {
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        MessageBoxResult result = MessageBox.Show(AppResources.DeleteMySpotMessage, AppResources.DeleteMySpotCaption, MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                            this.DeleteSpotAsync(spotViewItem);
                    }
                    else
                    {
                        MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
                    }
                    this.DeleteSpotButton = false;
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
            base.Dispatcher.BeginInvoke(() =>
            {
                this.DeleteAppBarButton.IsEnabled = false;
            });

            // Before go load, set mutex to true.
            MySpotViewModel.IsDataLoaded = true;

            // If there is my spots, Clear and Add spots to list
            // Otherwise, Show none message.
            JArray spots = await App.SpotManager.GetMySpotViewItemsAsync();

            if (spots != null)  // There are my spots
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    this.DeleteAppBarButton.IsEnabled = true;
                    uiMySpotList.Visibility = Visibility.Visible;
                    uiMySpotMessage.Visibility = Visibility.Collapsed;
                    this.MySpotViewModel.SetItems(spots);
                });
            }
            else  // No my spots
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    base.SetListUnableAndShowMessage(uiMySpotList, AppResources.NoMySpotMessage, uiMySpotMessage);
                });
            }

            // Hide progress indicator
            base.SetProgressIndicator(false);
        }


        // Delete all spots.
        private void uiAppBarDeleteButton_Click(object sender, System.EventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBoxResult result = MessageBox.Show(AppResources.DeleteAllMySpotMessage, AppResources.DeleteAllMySpotCaption, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    List<SpotViewItem> items = new List<SpotViewItem>();
                    foreach (SpotViewItem spotViewItem in this.MySpotViewModel.Items)
                        items.Add(spotViewItem);
                    foreach (SpotViewItem spotViewItem in items)
                        this.DeleteSpotAsync(spotViewItem);
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
                this.DeleteAppBarButton.IsEnabled = false;
                spotViewItem.DeleteImage = FileObjectViewModel.DELETING_IMAGE_URI;
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
                        if (this.MySpotViewModel.Items.Count > 0)
                            this.DeleteAppBarButton.IsEnabled = true;
                        else
                            base.SetListUnableAndShowMessage(uiMySpotList, AppResources.NoMySpotMessage, uiMySpotMessage);
                    });
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        spotViewItem.DeleteImage = FileObjectViewModel.FAIL_IMAGE_URI;
                    });
                }
            }
            else
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    spotViewItem.DeleteImage = FileObjectViewModel.FAIL_IMAGE_URI;
                });
            }

            // Hide Progress Indicator
            base.SetProgressIndicator(false);
        }


        private void uiDeleteSpotButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DeleteSpotButton = true;
        }
    }
}
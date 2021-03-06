﻿using System;
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
using PintheCloud.Helpers;
using Windows.Storage;
using Windows.System;
using System.Threading;

namespace PintheCloud.Pages
{
    public partial class SettingsPage : PtcPage
    { 
        // Const Instances
        private const int MY_SPOT_PIVOT_INDEX = 2;
        private const int MY_PICK_PIVOT_INDEX = 3;

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

        private const string MY_SPOT_DELETE_BUTTON_IMAGE_URI = "/Assets/pajeon/at_here/png/upload_list_delete.png";
        private const string MY_SPOT_DELETE_BUTTON_PRESS_IMAGE_URI = "/Assets/pajeon/at_here/png/upload_list_delete_p.png";

        // Instances
        private SpotViewModel MySpotViewModel = new SpotViewModel();
        private FileObjectViewModel MyPickFileObjectViewModel = new FileObjectViewModel();
        private IReadOnlyList<StorageFile> LocalFileList;
        private Button[] SignButtons = null;
        private Button[] MainButtons = null;
        private Grid[] SignButtonGrids = null;
        private TextBlock[] SignButtonTextBlocks = null;
        private bool DeleteSpotButton = false;
        private bool LaunchLock = false;



        public SettingsPage()
        {
            InitializeComponent();

            // Set name
            uiDefaultSpotNameTextBox.Text = (string)App.ApplicationSettings[StorageAccount.ACCOUNT_DEFAULT_SPOT_NAME_KEY];

            // Set UI list
            this.SignButtons = new Button[] { uiOneDriveSignButton, uiDropboxSignButton, uiGoogleDriveSignButton };
            this.MainButtons = new Button[] { uiOneDriveMainButton, uiDropboxMainButton, uiGoogleDriveMainButton };
            this.SignButtonGrids = new Grid[] { uiOneDriveSignButtonGrid, uiDropboxSignButtonGrid, uiGoogleDriveSignButtonGrid };
            this.SignButtonTextBlocks = new TextBlock[] { uiOneDriveSignButtonText, uiDropboxSignButtonText, uiGoogleDriveSignButtonText };

            // Set Sign buttons and Set Main buttons.
            using (var itr = StorageHelper.GetStorageEnumerator())
            {
                while (itr.MoveNext())
                {
                    IStorageManager storage = itr.Current;
                    this.SetSignButtons(storage);
                    this.SetMainButtons(storage);
                }
            }

            // Set location access consent checkbox
            uiLocationAccessConsentToggleSwitchButton.IsChecked = (bool)App.ApplicationSettings[StorageAccount.LOCATION_ACCESS_CONSENT_KEY];

            // Set delete app bar button and datacontext
            uiMySpotList.DataContext = this.MySpotViewModel;
            uiMyPicktList.DataContext = this.MyPickFileObjectViewModel;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.LaunchLock = false;
            this.SetMySpotPivot(AppResources.Loading);
            this.SetMyPickPivot(AppResources.Loading);
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            // If it is signing, don't close app.
            // Otherwise, show close app message.
            IStorageManager iStorageManager = Switcher.GetCurrentStorage();
            if (iStorageManager.IsSigningIn())
            {
                e.Cancel = true;

                // If it is popup, close popup.
                if (iStorageManager.IsPopup())
                    EventHelper.TriggerEvent(EventHelper.POPUP_CLOSE);
            }
        }


        // Construct pivot item by page index
        private void uiSettingsPivot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Set View model for dispaly,
            // One time loading.
            switch (uiSettingsPivot.SelectedIndex)
            {
                case MY_SPOT_PIVOT_INDEX:

                    // Set My Spot stuff enable and set list
                    ApplicationBar.IsVisible = true;
                    this.SetMySpotPivot(AppResources.Loading);
                    break;

                case MY_PICK_PIVOT_INDEX:
                    // Set My Pick stuff enable and set list
                    ApplicationBar.IsVisible = true;
                    this.SetMyPickPivot(AppResources.Loading);
                    break;

                default:

                    // Set My Spot stuff unable
                    ApplicationBar.IsVisible = false;
                    break;
            }
        }



        /*** Cloud ***/

        private async void CloudSignInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Set process indicator
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiCloudPanel.Visibility = Visibility.Collapsed;
                    uiCloudMessageGrid.Visibility = Visibility.Visible;
                    uiCloudMessage.Text = AppResources.DoingSignIn;
                });

                // Set Current Storage
                Button signButton = (Button)sender;
                Switcher.SetStorageTo(signButton.Tag.ToString());


                try
                {
                    // Sign in
                    IStorageManager iStorageManager = StorageHelper.GetStorageManager(signButton.Tag.ToString());
                    if (!iStorageManager.IsSigningIn())
                        TaskHelper.AddSignInTask(iStorageManager.GetStorageName(), iStorageManager.SignIn());

                    // If sign in success, set list.
                    // Otherwise, show bad sign in message box.
                    await TaskHelper.WaitSignInTask(iStorageManager.GetStorageName());
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        this.SetSignButtons(Switcher.GetCurrentStorage());
                        this.MySpotViewModel.IsDataLoaded = false;
                        this.SetMySpotPivot(AppResources.Loading);
                    });
                }
                catch
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(AppResources.BadSignInMessage, AppResources.BadSignInCaption, MessageBoxButton.OK);
                    });
                }
                finally
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        uiCloudPanel.Visibility = Visibility.Visible;
                        uiCloudMessageGrid.Visibility = Visibility.Collapsed;
                    });
                }
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private void CloudSignOutButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Sign out
            MessageBoxResult signOutResult = MessageBox.Show(AppResources.CloudSignOutMessage, AppResources.CloudSignOutCaption, MessageBoxButton.OKCancel);
            if (signOutResult == MessageBoxResult.OK)
            {
                // Set process indicator and get index
                uiCloudPanel.Visibility = Visibility.Collapsed;
                uiCloudMessageGrid.Visibility = Visibility.Visible;
                uiCloudMessage.Text = AppResources.DoingSignOut;

                // Set Current Storage
                Button signButton = (Button)sender;
                Switcher.SetStorageTo(signButton.Tag.ToString());

                // Sign out
                IStorageManager iStorageManager = StorageHelper.GetStorageManager(signButton.Tag.ToString());
                StorageExplorer.RemoveKey(iStorageManager.GetStorageName());
                iStorageManager.SignOut();

                // After sign out, set list.
                this.MySpotViewModel.IsDataLoaded = false;
                this.SetMySpotPivot(AppResources.Loading);

                // Hide process indicator
                uiCloudPanel.Visibility = Visibility.Visible;
                uiCloudMessageGrid.Visibility = Visibility.Collapsed;
                this.SetSignButtons(iStorageManager);
                SetMainButtons(Switcher.GetMainStorage());
            }
        }


        private async void SetSignButtons(IStorageManager iStorageManager)
        {
            int platformIndex = Switcher.GetStorageIndex(iStorageManager.GetStorageName());
            try
            {
                if (!iStorageManager.IsSignIn()) throw new Exception();
                await TaskHelper.WaitSignInTask(iStorageManager.GetStorageName());
                StorageAccount storageAccount = await iStorageManager.GetStorageAccountAsync();
                if (storageAccount == null) throw new Exception();
                base.Dispatcher.BeginInvoke(() =>
                {
                    this.SignButtonTextBlocks[platformIndex].Text = storageAccount.UserName;
                    this.SignButtonTextBlocks[platformIndex].Foreground = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(SIGN_IN_BUTTON_TEXT_COLOR));
                    this.SignButtonTextBlocks[platformIndex].FontFamily = new FontFamily(SIGN_IN_BUTTON_TEXT_FONT);
                    this.SignButtons[platformIndex].Click += this.CloudSignOutButton_Click;
                    this.SignButtons[platformIndex].Click -= this.CloudSignInButton_Click;
                });
            }
            catch
            {
                // It haven't signed in
                base.Dispatcher.BeginInvoke(() =>
                {
                    this.SignButtonTextBlocks[platformIndex].Text = AppResources.SignIn;
                    this.SignButtonTextBlocks[platformIndex].Foreground = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(SIGN_NOT_IN_BUTTON_TEXT_COLOR));
                    this.SignButtonTextBlocks[platformIndex].FontFamily = new FontFamily(SIGN_NOT_IN_BUTTON_TEXT_FONT);
                    this.SignButtons[platformIndex].Click -= this.CloudSignOutButton_Click;
                    this.SignButtons[platformIndex].Click += this.CloudSignInButton_Click;
                });
            }
        }


        private void SetMainButtons(IStorageManager StorageManager)
        {
            int platformIndexindex = Switcher.GetStorageIndex(StorageManager.GetStorageName());

            // Set main button click event and image.
            Button mainButton = this.MainButtons[platformIndexindex];
            mainButton.Click += this.MainButton_Click;

            Image mainButtonImage = (Image)mainButton.Content;
            Grid signButtonGrid = this.SignButtonGrids[platformIndexindex];
            if (StorageManager.GetStorageName().Equals(Switcher.GetMainStorage().GetStorageName()))
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

            // Set Main and Current Platform
            Switcher.SetMainPlatform(mainButton.Tag.ToString());
            Switcher.SetStorageTo(mainButton.Tag.ToString());

            // Set Signbutton background
            Grid signButtonGrid = this.SignButtonGrids[Switcher.GetCurrentIndex()];
            signButtonGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(MAIN_PLATFORM_BUTTON_COLOR));
            signButtonGrid.Opacity = MAIN_PLATFORM_BUTTON_OPACITY;

            // Set rest button image and background
            for (var i = 0; i < StorageHelper.GetStorageList().Count; i++)
            {
                if (!StorageHelper.GetStorageList()[i].GetStorageName().Equals(Switcher.GetMainStorage().GetStorageName()))
                {
                    ((Image)this.MainButtons[i].Content).Source = new BitmapImage(new Uri(SETTING_ACCOUNT_MAIN_CHECK_NOT_IMAGE_URI, UriKind.Relative));
                    Grid restSignButtonGrid = (Grid)this.SignButtonGrids[i];
                    restSignButtonGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(MAIN_NOT_PLATFORM_BUTTON_COLOR));
                    restSignButtonGrid.Opacity = MAIN_NOT_PLATFORM_BUTTON_OPACITY;
                }
            }
        }



        /*** Profile ***/

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
            uiDefaultSpotNameTextBox.Text = uiDefaultSpotNameTextBox.Text.Trim();
        }

        private void uiDefaultSpotNameSetButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (uiDefaultSpotNameSetButton.IsEnabled)
            {
                uiDefaultSpotNameTextBox.Text = uiDefaultSpotNameTextBox.Text.Trim();
                App.ApplicationSettings[StorageAccount.ACCOUNT_DEFAULT_SPOT_NAME_KEY] = uiDefaultSpotNameTextBox.Text;
                App.ApplicationSettings.Save();
                MessageBox.Show(AppResources.SetDefaultSpotNameMessage, uiDefaultSpotNameTextBox.Text, MessageBoxButton.OK);
            }
        }


        private void uiLocationAccessConsentToggleSwitchButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[StorageAccount.LOCATION_ACCESS_CONSENT_KEY] = true;
            App.ApplicationSettings.Save();
        }


        private void uiLocationAccessConsentToggleSwitchButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ApplicationSettings[StorageAccount.LOCATION_ACCESS_CONSENT_KEY] = false;
            App.ApplicationSettings.Save();
        }



        /*** My Spot ***/

        private void SetMySpotPivot(string message)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                if (!this.MySpotViewModel.IsDataLoaded)
                    this.SetMySpotListAsync(message);
            }
            else
            {
                base.SetListUnableAndShowMessage(uiMySpotList, uiMySpotMessage, AppResources.InternetUnavailableMessage);
            }
        }


        // List select event
        private void uiMySpotList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected Spot View Item
            // Set selected item to null for next selection of list item. 
            SpotViewItem spotViewItem = uiMySpotList.SelectedItem as SpotViewItem;
            uiMySpotList.SelectedItem = null;
            if (spotViewItem != null)
            {
                // If it is not from delete button, go to spot page.
                // Otherwise, delete spot.
                if (!this.DeleteSpotButton)  // Not delete job.
                {
                    if (spotViewItem.DeleteImage.Equals(FileObjectViewModel.DELETE_IMAGE_URI))
                    {
                        string parameters = base.GetParameterStringFromSpotViewItem(spotViewItem);
                        NavigationService.Navigate(new Uri(EventHelper.EXPLORER_PAGE + parameters, UriKind.Relative));
                    }
                }
                else  // Delete job.
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


        private async void DeleteSpotAsync(SpotViewItem spotViewItem)
        {
            // Show Deleting message
            base.SetProgressIndicator(true);
            base.Dispatcher.BeginInvoke(() =>
            {
                spotViewItem.DeleteImage = FileObjectViewModel.ING_IMAGE_URI;
                spotViewItem.DeleteImagePress = false;
            });

            // If delete job success to all files, delete spot.
            // Otherwise, show delete fail image.
            SpotObject spot = App.SpotManager.GetSpotObject(spotViewItem.SpotId);
            try
            {
                await spot.DeleteFileObjectsAsync();
                await App.SpotManager.DeleteSpotAsync(spotViewItem.SpotId);
                base.Dispatcher.BeginInvoke(() =>
                {
                    this.MySpotViewModel.Items.Remove(spotViewItem);
                    ((SpotViewModel)PhoneApplicationService.Current.State[SPOT_VIEW_MODEL_KEY]).IsDataLoaded = false;
                    if (this.MySpotViewModel.Items.Count < 1)
                        base.SetListUnableAndShowMessage(uiMySpotList, uiMySpotMessage, AppResources.NoMySpotMessage);
                });
            }
            catch
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


        private void uiDeleteSpotButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((Image)((Button)sender).Content).Source = new BitmapImage(new Uri(MY_SPOT_DELETE_BUTTON_PRESS_IMAGE_URI, UriKind.Relative));
        }


        private void uiDeleteSpotButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((Image)((Button)sender).Content).Source = new BitmapImage(new Uri(MY_SPOT_DELETE_BUTTON_IMAGE_URI, UriKind.Relative));
        }


        // Refresh spot list.
        private void uiAppBarRefreshButton_Click(object sender, System.EventArgs e)
        {
            this.MySpotViewModel.IsDataLoaded = false;
            this.SetMySpotPivot(AppResources.Refreshing);
        }


        private async void SetMySpotListAsync(string message)
        {
            // Show progress indicator 
            base.SetListUnableAndShowMessage(uiMySpotList, uiMySpotMessage, message);
            base.SetProgressIndicator(true);

            try
            {
                // If there is my spots, Clear and Add spots to list
                // Otherwise, Show none message.
                List<SpotObject> spots = await App.SpotManager.GetMySpotList();
                if (spots.Count > 0)  // There are my spots
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        this.MySpotViewModel.IsDataLoaded = true;
                        uiMySpotList.Visibility = Visibility.Visible;
                        uiMySpotMessage.Visibility = Visibility.Collapsed;
                        this.MySpotViewModel.SetItems(spots);
                    });
                }
                else  // There are not my spots
                {
                    base.SetListUnableAndShowMessage(uiMySpotList, uiMySpotMessage, AppResources.NoMySpotMessage);
                }
            }
            catch
            {
                base.SetListUnableAndShowMessage(uiMySpotList, uiMySpotMessage, AppResources.BadLoadingSpotMessage);
            }

            // Hide progress indicator
            base.SetProgressIndicator(false);
        }
  

        private void uiPtcAccountSignOutButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Show select dialog for real signout.
            MessageBoxResult result = MessageBox.Show(AppResources.PtcSignOutMessage, AppResources.PtcSignOutCaption, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.Cancel) return;

            // If user say yes to sign out, do it and move to splash page.
            StorageExplorer.RemoveAllKeys();
            using (var itr = StorageHelper.GetStorageEnumerator())
            {
                while (itr.MoveNext())
                {
                    if (itr.Current.IsSignIn())
                        itr.Current.SignOut();
                }
            }
            App.AccountManager.SignOut();
            NavigationService.Navigate(new Uri(EventHelper.SPLASH_PAGE, UriKind.Relative));
        }



        /*** My Pick ***/

        private void SetMyPickPivot(string message)
        {
            // If Internet available, Set spot list
            if (!this.MyPickFileObjectViewModel.IsDataLoaded)  // Mutex check
                this.SetMyPickListAsync(message);
        }


        private async void SetMyPickListAsync(string message)
        {
            // Show progress indicator 
            base.SetListUnableAndShowMessage(uiMyPicktList, uiMyPickMessage, message);
            base.SetProgressIndicator(true);

            // Create or Open local folder
            StorageFolder folder = await ApplicationData.Current.LocalFolder
                .CreateFolderAsync(SpotObject.PREVIEW_FILE_LOCATION, CreationCollisionOption.OpenIfExists);
            if (folder != null)
            {
                // Get files from local folder.
                // If file is on, set items.
                // Otherwise, show error message.
                this.LocalFileList = await folder.GetFilesAsync();
                if (this.LocalFileList.Count > 0)
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        foreach (StorageFile storageFile in this.LocalFileList)
                        {
                            this.MyPickFileObjectViewModel.IsDataLoaded = true;
                            uiMyPicktList.Visibility = Visibility.Visible;
                            uiMyPickMessage.Visibility = Visibility.Collapsed;

                            FileObjectViewItem fileObjectViewItem = new FileObjectViewItem();
                            fileObjectViewItem.Name = storageFile.Name;
                            fileObjectViewItem.ThumnailType = storageFile.Name.Split('.').Last();
                            fileObjectViewItem.SelectFileImage = FileObjectViewModel.TRANSPARENT_IMAGE_URI;
                            this.MyPickFileObjectViewModel.Items.Add(fileObjectViewItem);
                        }
                    });
                }
                else
                {
                    base.SetListUnableAndShowMessage(uiMyPicktList, uiMyPickMessage, AppResources.NoFileInFolderMessage);
                }
            }

            // Hide progress indicator
            base.SetProgressIndicator(false);
        }


        // Launch files to other reader app.
        private async void uiMyPicktList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.LaunchLock) return;
            FileObjectViewItem fileObejctViewItem = uiMyPicktList.SelectedItem as FileObjectViewItem;
            uiMyPicktList.SelectedItem = null;
            if (fileObejctViewItem != null)
            {
                for (var i = 0; i < this.LocalFileList.Count; i++)
                {
                    if (this.LocalFileList[i].Name.Equals(fileObejctViewItem.Name))
                    {
                        this.LaunchLock = true;
                        await Launcher.LaunchFileAsync(this.LocalFileList[i]);
                        return;
                    }
                }
                MessageBox.Show(AppResources.BadLoadingFileMessage, AppResources.BadLoadingFileCaption, MessageBoxButton.OK);
            }
        }
    }
}
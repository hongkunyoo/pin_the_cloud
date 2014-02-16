using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Net.NetworkInformation;
using PintheCloud.Managers;
using System.Threading.Tasks;
using PintheCloud.Workers;
using PintheCloud.ViewModels;
using PintheCloud.Models;
using Microsoft.WindowsAzure.MobileServices;
using System.Collections.ObjectModel;
using Windows.Devices.Geolocation;
using PintheCloud.Resources;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using PintheCloud.Utilities;
using System.ComponentModel;

namespace PintheCloud.Pages
{
    public partial class ExplorerPage : PtcPage
    {
        // Static Instances
        public const int PICK_PIVOT = 0;
        public const int PIN_PIVOT = 1;


        // Const Instances
        private const string PIN_APP_BAR_BUTTON_ICON_URI = "/Assets/pajeon/png/general_bar_upload.png";
        private const int SETTING_APP_BAR_BUTTON = 0;
        private const int PIN_APP_BAR_BUTTON = 1;
        private const int SKY_DRIVE_APP_BAR_MENUITEMS = 1;
        private const int DROPBOX_APP_BAR_BUTTON = 2;


        // Instances
        private SpaceViewModel NearSpaceViewModel = new SpaceViewModel();
        private bool IsFileObjectLoaded = false;
        private bool IsFileObjectLoading = false;
        private Stack<FileObject> FolderTree = new Stack<FileObject>();
        private List<FileObject> SelectedFile = new List<FileObject>();
        private bool IsSignIning = false;


        public ExplorerPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Check if it is on the backstack from SplashPage and remove that.
            if (NavigationService.BackStack.Count() == 1)
                NavigationService.RemoveBackEntry();



            /*** Pick Pivot ***/

            // If it doesn't first enter, but still loading, do it once again.
            if (uiNearSpaceMessage.Visibility == Visibility.Visible)
            {
                if (uiNearSpaceMessage.Text.Equals(AppResources.Loading) || uiNearSpaceMessage.Text.Equals(AppResources.Refreshing))
                {
                    // If Internet available, Set space list
                    // Otherwise, show internet bad message
                    if (NetworkInterface.GetIsNetworkAvailable())
                        await this.SetExplorerPivotAsync(uiNearSpaceMessage.Text);
                    else
                        base.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.InternetUnavailableMessage, uiNearSpaceMessage);
                }
            }



            /*** Pin Pivot ***/

            if (uiPinInfoMessage.Visibility == Visibility.Visible)
            {
                if (uiPinInfoMessage.Text.Equals(AppResources.Loading) || uiPinInfoMessage.Text.Equals(AppResources.Refreshing))
                {
                    // If Internet available, Set pin list with root folder file list.
                    // Otherwise, show internet bad message
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        this.FolderTree.Clear();
                        this.SelectedFile.Clear();
                        await this.SetFileTreeForFolder(null, uiPinInfoMessage.Text);
                    }
                    else
                    {
                        base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                    }
                }
            }


            // If it is already signin skydrive, load files.
            // Otherwise, show signin button.
            bool isSkyDriveLogin = false;
            App.ApplicationSettings.TryGetValue<bool>(Account.ACCOUNT_SKY_DRIVE_IS_SIGN_IN, out isSkyDriveLogin);
            if (!isSkyDriveLogin)
            {
                if (uiPinInfoSignInGrid.Visibility == Visibility.Collapsed)
                {
                    uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                    uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (uiPinInfoSignInGrid.Visibility == Visibility.Visible)
                {
                    // Show Loading message and save is login true for pivot moving action while sign in.
                    uiPinInfoListGrid.Visibility = Visibility.Visible;
                    uiPinInfoSignInGrid.Visibility = Visibility.Collapsed;
                    uiPinInfoCurrentPath.Text = "";

                    if (NetworkInterface.GetIsNetworkAvailable())
                        await this.SetPinInfoPivotAsync(this, AppResources.Loading);
                    else
                        base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                }
            }
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            PhoneApplicationService.Current.State["PIVOT"] = uiExplorerPivot.SelectedIndex;
        }


        // Construct pivot item by page index
        private async void uiExplorerPivot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Set View model for dispaly,
            // One time loading.
            switch (uiExplorerPivot.SelectedIndex)
            {
                case PICK_PIVOT:
                    // Remove button and menuitems
                    ApplicationBar.Buttons.RemoveAt(PIN_APP_BAR_BUTTON);
                    ApplicationBar.MenuItems.RemoveAt(DROPBOX_APP_BAR_BUTTON);
                    ApplicationBar.MenuItems.RemoveAt(SKY_DRIVE_APP_BAR_MENUITEMS);

                    // Remove Cloud Kind Image
                    uiCurrentCloudModeText.Visibility = Visibility.Collapsed;


                    // If Internet available, Set space list
                    // Otherwise, show internet bad message
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        if (!this.NearSpaceViewModel.IsDataLoaded)  // Mutex check
                            await this.SetExplorerPivotAsync(AppResources.Loading);
                    }
                    else
                    {
                        base.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.InternetUnavailableMessage, uiNearSpaceMessage);
                    }
                    break;


                case PIN_PIVOT:
                    // Set pin button
                    ApplicationBarIconButton pinInfoAppBarButton = new ApplicationBarIconButton();
                    pinInfoAppBarButton.Text = AppResources.Pin;
                    pinInfoAppBarButton.IconUri = new Uri(PIN_APP_BAR_BUTTON_ICON_URI, UriKind.Relative);
                    pinInfoAppBarButton.IsEnabled = false;
                    pinInfoAppBarButton.Click += pinInfoAppBarButton_Click;
                    ApplicationBar.Buttons.Add(pinInfoAppBarButton);

                    // Set Cloud Setting selection.
                    ApplicationBarMenuItem skyDriveAppBarButton = new ApplicationBarMenuItem();
                    skyDriveAppBarButton.Text = AppResources.SkyDrive;
                    skyDriveAppBarButton.Click += skyDriveAppBarButton_Click;
                    ApplicationBar.MenuItems.Add(skyDriveAppBarButton);

                    ApplicationBarMenuItem dropboxAppBarButton = new ApplicationBarMenuItem();
                    dropboxAppBarButton.Text = AppResources.Dropbox;
                    dropboxAppBarButton.Click += dropboxAppBarButton_Click;
                    ApplicationBar.MenuItems.Add(dropboxAppBarButton);


                    // Set Cloud Kind Image
                    App.IStorageManager = App.SkyDriveManager;
                    uiCurrentCloudModeText.Visibility = Visibility.Visible;
                    uiCurrentCloudModeText.Text = AppResources.SkyDrive;


                    // If it is already signin skydrive, load files.
                    // Otherwise, show signin button.
                    
                    bool isSkyDriveLogin = false;
                    App.ApplicationSettings.TryGetValue<bool>(Account.ACCOUNT_SKY_DRIVE_IS_SIGN_IN, out isSkyDriveLogin);
                    if (!isSkyDriveLogin)
                    {
                        uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                        uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        uiPinInfoListGrid.Visibility = Visibility.Visible;
                        uiPinInfoSignInGrid.Visibility = Visibility.Collapsed;

                        if (NetworkInterface.GetIsNetworkAvailable())
                        {
                            if (!this.IsSignIning && !this.IsFileObjectLoaded)
                                await this.SetPinInfoPivotAsync(this, AppResources.DoingSignIn);
                        }
                        else
                        {
                            base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                        }
                    }
                    break;
            }
        }


        // Refresh space list.
        private async void uiAppBarRefreshMenuItem_Click(object sender, System.EventArgs e)
        {
            switch (uiExplorerPivot.SelectedIndex)
            {
                case PICK_PIVOT:
                    if (NetworkInterface.GetIsNetworkAvailable())
                        await this.SetExplorerPivotAsync(AppResources.Refreshing);
                    else
                        base.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.InternetUnavailableMessage, uiNearSpaceMessage);
                    break;

                case PIN_PIVOT:
                    // TODO different by current state
                    // If it is in sign in process, don't refresh.
                    if (!this.IsSignIning)
                    {
                        // If it is already signin skydrive, load files.
                        // Otherwise, show signin button.
                        bool isSignIn = false;
                        App.ApplicationSettings.TryGetValue<bool>(Account.ACCOUNT_SKY_DRIVE_IS_SIGN_IN, out isSignIn);
                        if (isSignIn)
                        {
                            uiPinInfoCurrentPath.Text = "";

                            if (NetworkInterface.GetIsNetworkAvailable())
                                await this.SetPinInfoPivotAsync(this, AppResources.Refreshing);
                            else
                                base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                        }
                    }
                    break;
            }
        }


        // Move to Setting Page
        private void uiAppBarSettingsButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri(PtcPage.SETTINGS_PAGE, UriKind.Relative));
        }



        /*** Pick Pivot ***/

        // List select event
        private void uiNearSpaceList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected Space View Item
            SpaceViewItem spaceViewItem = uiNearSpaceList.SelectedItem as SpaceViewItem;

            // Set selected item to null for next selection of list item. 
            uiNearSpaceList.SelectedItem = null;


            // If selected item isn't null and it doesn't come from like button, goto File list page.
            // Otherwise, Process Like or Not Like by current state
            if (spaceViewItem != null)  // Go to FIle List Page
            {
                string parameters = App.SpaceManager.GetParameterStringFromSpaceViewItem(spaceViewItem);
                NavigationService.Navigate(new Uri(PtcPage.FILE_LIST_PAGE + parameters, UriKind.Relative));
            }
        }


        private async Task SetExplorerPivotAsync(string message)
        {
            // Show progress indicator 
            base.SetListUnableAndShowMessage(uiNearSpaceList, message, uiNearSpaceMessage);
            PtcPage.SetProgressIndicator(this, true);

            // Before go load, set mutex to true.
            this.NearSpaceViewModel.IsDataLoaded = true;


            // Check whether user consented for location access.
            if (base.GetLocationAccessConsent())  // Got consent of location access.
            {
                // Check whether GPS is on or not
                if (App.GeoCalculateManager.GetGeolocatorPositionStatus())  // GPS is on
                {
                    Geoposition currentGeoposition = await App.GeoCalculateManager.GetCurrentGeopositionAsync();

                    // Check whether GPS works well or not
                    if (currentGeoposition != null)  // works well
                    {
                        // If there is near spaces, Clear and Add spaces to list
                        // Otherwise, Show none message.
                        JArray spaces = await App.SpaceManager.GetNearSpaceViewItemsAsync(currentGeoposition);

                        if (spaces != null)  // There are near spaces
                        {
                            this.NearSpaceViewModel.SetItems(spaces, currentGeoposition);
                            uiNearSpaceList.DataContext = this.NearSpaceViewModel;
                            uiNearSpaceList.Visibility = Visibility.Visible;
                            uiNearSpaceMessage.Visibility = Visibility.Collapsed;
                        }
                        else  // No near spaces
                        {
                            base.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.NoNearSpotMessage, uiNearSpaceMessage);
                        }
                    }
                    else  // works bad
                    {
                        // Show GPS off message box.
                        base.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.BadGpsMessage, uiNearSpaceMessage);
                        NearSpaceViewModel.IsDataLoaded = false;  // Mutex
                    }
                }
                else  // GPS is off
                {
                    // Show GPS off message box.
                    base.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.NoGpsOnMessage, uiNearSpaceMessage);
                    NearSpaceViewModel.IsDataLoaded = false;  // Mutex
                }
            }
            else  // First or not consented of access in location information.
            {
                // Show no consent message box.
                base.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.NoLocationAcessConsentMessage, uiNearSpaceMessage);
                NearSpaceViewModel.IsDataLoaded = false;  // Mutex
            }


            // Hide progress indicator
            PtcPage.SetProgressIndicator(this, false);
        }



        /*** Pin Pivot ***/

        private async void skyDriveAppBarButton_Click(object sender, EventArgs e)
        {
            // If it is not in sky drive mode, change it.
            if (!uiCurrentCloudModeText.Text.Equals(AppResources.SkyDrive))
            {
                uiCurrentCloudModeText.Text = AppResources.SkyDrive;
                App.IStorageManager = App.SkyDriveManager;
                if (!this.IsSignIning && !this.IsFileObjectLoading)
                {
                    // If it is already signin skydrive, load files.
                    // Otherwise, show signin button.
                    bool isSkyDriveLogin = false;
                    App.ApplicationSettings.TryGetValue<bool>(Account.ACCOUNT_SKY_DRIVE_IS_SIGN_IN, out isSkyDriveLogin);
                    if (!isSkyDriveLogin)
                    {
                        uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                        uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        // Show Loading message and save is login true for pivot moving action while sign in.
                        uiPinInfoListGrid.Visibility = Visibility.Visible;
                        uiPinInfoSignInGrid.Visibility = Visibility.Collapsed;
                        uiPinInfoCurrentPath.Text = "";

                        if (NetworkInterface.GetIsNetworkAvailable())
                            await this.SetPinInfoPivotAsync(this, AppResources.Loading);
                        else
                            base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                    }
                }
            }
        }


        private void dropboxAppBarButton_Click(object sender, EventArgs e)
        {
            //TODO. Code below is pseudo.
            // If it is not in dropbox mode, change it.
            if (!uiCurrentCloudModeText.Text.Equals(AppResources.Dropbox))
            {
                uiCurrentCloudModeText.Text = AppResources.Dropbox;
            //    if (!this.IsSignIning && !this.IsFileObjectLoading)
            //    {
            //        // If it is already signin skydrive, load files.
            //        // Otherwise, show signin button.
            //        bool isDropboxSIgnIn = false;
            //        App.ApplicationSettings.TryGetValue<bool>(Account.ACCOUNT_SKY_DRIVE_IS_SIGN_IN, out isDropboxSIgnIn);
            //        if (!isDropboxSIgnIn)
            //        {
            //            uiPinInfoListGrid.Visibility = Visibility.Collapsed;
            //            uiPinInfoSignInGrid.Visibility = Visibility.Visible;
            //        }
            //        else
            //        {
            //            // Show Loading message and save is login true for pivot moving action while sign in.
            //            uiPinInfoListGrid.Visibility = Visibility.Visible;
            //            uiPinInfoSignInGrid.Visibility = Visibility.Collapsed;
            //            uiPinInfoCurrentPath.Text = "";

            //            if (NetworkInterface.GetIsNetworkAvailable())
            //                await this.SetPinInfoPivotAsync(this, AppResources.Loading);
            //            else
            //                base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
            //        }
            //    }
            }
        }


        private void pinInfoAppBarButton_Click(object sender, EventArgs e)
        {
            // Check whether user consented for location access.
            if (base.GetLocationAccessConsent())  // Got consent of location access.
            {
                // Check whether GPS is on or not
                if (App.GeoCalculateManager.GetGeolocatorPositionStatus())  // GPS is on
                {
                    PhoneApplicationService.Current.State["SELECTED_FILE"] = this.SelectedFile;
                    NavigationService.Navigate(new Uri(PtcPage.FILE_LIST_PAGE, UriKind.Relative));
                }
                else  // GPS is off
                {
                    // Show GPS off message box.
                    MessageBox.Show(AppResources.NoGpsOnMessage, AppResources.NoGpsOnCaption, MessageBoxButton.OK);
                }
            }
            else  // First or not consented of access in location information.
            {
                // Show no consent message box.
                MessageBox.Show(AppResources.NoLocationAcessConsentMessage, AppResources.NoLocationAcessConsentCaption, MessageBoxButton.OK);
            }
        }


        private void uiPinInfoListUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.TreeUp();
        }


        // Pin file selection event.
        private async void uiPinInfoList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected File Object
            FileObject fileObject = uiPinInfoList.SelectedItem as FileObject;

            // Set selected item to null for next selection of list item. 
            uiPinInfoList.SelectedItem = null;

            // If selected item isn't null, Do something
            if (fileObject != null)
            {
                // If user select folder, go in.
                // Otherwise, add it to list.
                if (fileObject.ThumnailType.Equals(AppResources.Folder))
                {
                    await this.SetFileTreeForFolder(fileObject, AppResources.Loading);
                    MyDebug.WriteLine(fileObject.Name);
                }
                else  // Do selection if file
                {
                    if (fileObject.SelectCheckImage.Equals(FileObject.CHECK_NOT_IMAGE_PATH))
                    {
                        this.SelectedFile.Add(fileObject);
                        fileObject.SetSelectCheckImage(true);
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[PIN_APP_BAR_BUTTON]).IsEnabled = true;
                    }

                    else
                    {
                        this.SelectedFile.Remove(fileObject);
                        fileObject.SetSelectCheckImage(false);
                        if (this.SelectedFile.Count <= 0)
                            ((ApplicationBarIconButton)ApplicationBar.Buttons[PIN_APP_BAR_BUTTON]).IsEnabled = false;
                    }
                    MyDebug.WriteLine(fileObject.Name);
                }
            }
        }


        // Get file tree from cloud
        private async Task SetFileTreeForFolder(FileObject folder, string message)
        {
            // Show progress indicator 
            base.SetListUnableAndShowMessage(uiPinInfoList, message, uiPinInfoMessage);
            PtcPage.SetProgressIndicator(this, true);

            // Set mutex to true.
            this.IsFileObjectLoaded = true;
            this.IsFileObjectLoading = true;

            // If folder null, set root.
            if (folder == null)
            {
                uiPinInfoCurrentPath.Text = "";
                folder = await App.IStorageManager.GetRootFolderAsync();
            }

            if (!this.FolderTree.Contains(folder))
                this.FolderTree.Push(folder);
            List<FileObject> files = await App.IStorageManager.GetFilesFromFolderAsync(folder.Id);

            // If there exists file, return that.
            if (files.Count > 0)
            {
                uiPinInfoCurrentPath.Text = this.GetCurrentPath();

                // Set file tree to list.
                uiPinInfoList.DataContext = new ObservableCollection<FileObject>(files);
                uiPinInfoList.Visibility = Visibility.Visible;
                uiPinInfoMessage.Visibility = Visibility.Collapsed;
            }
            else
            {
                base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.NoFileInCloudMessage, uiPinInfoMessage);
            }

            // Hide progress indicator, and set mutex to false.
            PtcPage.SetProgressIndicator(this, false);
            this.IsFileObjectLoading = false;
        }


        private string GetCurrentPath()
        {
            FileObject[] array = this.FolderTree.Reverse<FileObject>().ToArray<FileObject>();
            string str = "";
            foreach (FileObject f in array)
                str += f.Name + AppResources.RootPath;
            return str;
        }


        private async void TreeUp()
        {
            if (this.FolderTree.Count > 1)
            {
                this.FolderTree.Pop();
                await this.SetFileTreeForFolder(this.FolderTree.First(), AppResources.Loading);
            }
        }


        private async void uiPinInfoSignInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO Add other cloud mode.
            // If Internet available, Set pin list with root folder file list.
            // Otherwise, show internet bad message
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Show Loading message and save is login true for pivot moving action while sign in.
                uiPinInfoListGrid.Visibility = Visibility.Visible;
                uiPinInfoSignInGrid.Visibility = Visibility.Collapsed;
                uiPinInfoCurrentPath.Text = "";
                await this.SetPinInfoPivotAsync(this, AppResources.Loading);
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private async Task SetPinInfoPivotAsync(DependencyObject context, string message)
        {
            base.SetListUnableAndShowMessage(uiPinInfoList, message, uiPinInfoMessage);
            this.IsSignIning = true;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[SETTING_APP_BAR_BUTTON]).IsEnabled = false;

            // SIgn cloud manager. If success, show files.
            // Otherwise, show error message
            if (await App.IStorageManager.SignIn(context))
            {
                this.FolderTree.Clear();
                this.SelectedFile.Clear();
                await this.SetFileTreeForFolder(null, AppResources.Loading);
            }
            else
            {
                MessageBox.Show(AppResources.BadSignInMessage, AppResources.BadSignInCaption, MessageBoxButton.OK);
                uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                App.ApplicationSettings.Remove(Account.ACCOUNT_SKY_DRIVE_IS_SIGN_IN);
            }

            ((ApplicationBarIconButton)ApplicationBar.Buttons[SETTING_APP_BAR_BUTTON]).IsEnabled = true;
            this.IsSignIning = false;
        }
    }
}
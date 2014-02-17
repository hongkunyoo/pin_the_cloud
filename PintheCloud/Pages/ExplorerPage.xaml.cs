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
        public const int PIN_INFO_PIVOT = 1;


        // Const Instances
        private const string PIN_APP_BAR_BUTTON_ICON_URI = "/Assets/pajeon/png/general_bar_upload.png";
        private const int PIN_APP_BAR_BUTTON = 1;
        private const int SKY_DRIVE_APP_BAR_MENUITEMS = 1;
        private const int DROPBOX_APP_BAR_BUTTON = 2;


        // Instances
        private bool IsFileObjectLoaded = false;
        private bool IsFileObjectLoading = false;
        private bool IsSignIning = false;

        private ApplicationBarIconButton PinInfoAppBarButton = new ApplicationBarIconButton();
        private ApplicationBarMenuItem skyDriveAppBarButton = new ApplicationBarMenuItem();
        private ApplicationBarMenuItem dropboxAppBarButton = new ApplicationBarMenuItem();
        private SpaceViewModel NearSpaceViewModel = new SpaceViewModel();
        private Stack<FileObject> FolderTree = new Stack<FileObject>();
        private List<FileObject> SelectedFile = new List<FileObject>();



        // Constructer
        public ExplorerPage()
        {
            InitializeComponent();

            // Set pin button
            this.PinInfoAppBarButton.Text = AppResources.Pin;
            this.PinInfoAppBarButton.IconUri = new Uri(PIN_APP_BAR_BUTTON_ICON_URI, UriKind.Relative);
            this.PinInfoAppBarButton.IsEnabled = false;
            this.PinInfoAppBarButton.Click += pinInfoAppBarButton_Click;

            // Set Cloud Setting selection.
            this.skyDriveAppBarButton.Text = AppResources.SkyDrive;
            this.skyDriveAppBarButton.Click += skyDriveAppBarButton_Click;

            this.dropboxAppBarButton.Text = AppResources.Dropbox;
            this.dropboxAppBarButton.Click += dropboxAppBarButton_Click;

            // Set Data Context
            uiNearSpaceList.DataContext = this.NearSpaceViewModel;
        }


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Check if it is on the backstack from SplashPage and remove that.
            if (NavigationService.BackStack.Count() == 1)
                NavigationService.RemoveBackEntry();



            /*** Pick Pivot ***/

            // Comes from setting page
            // If near spaces loading failed, do it again.
            if (uiNearSpaceMessage.Visibility == Visibility.Visible && !uiNearSpaceMessage.Text.Equals(AppResources.NoNearSpotMessage))
            {
                // If Internet available, Set space list
                // Otherwise, show internet bad message
                if (NetworkInterface.GetIsNetworkAvailable())
                    await this.SetNearSpaceListAsync(AppResources.Loading);
                else
                    base.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.InternetUnavailableMessage, uiNearSpaceMessage);
            }



            /*** Pin Pivot ***/

            // Check main platform at frist login.
            string mainPlatformType = null;
            App.ApplicationSettings.TryGetValue<string>(Account.ACCOUNT_MAIN_PLATFORM_TYPE, out mainPlatformType);
            for (int i = 0; i < App.PLATFORMS.Length; i++)
            {
                if (mainPlatformType.Equals(App.PLATFORMS[i]))
                {
                    // Set Cloud Mode
                    uiCurrentCloudModeText.Text = App.PLATFORMS[i];
                    break;
                }
            }


            // Comes from File list Page and setting page
            // If selected files are over 1, erase it.
            if (this.SelectedFile.Count > 0)
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    this.SelectedFile.Clear();
                    this.PinInfoAppBarButton.IsEnabled = false;
                    await this.SetPinInfoListAsync(this.FolderTree.First(), AppResources.Loading);
                }
                else
                {
                    base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                }
            }


            // Comes from setting page
            // If files loading failed, do it again.
            if (uiPinInfoMessage.Visibility == Visibility.Visible)
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    if (!this.IsSignIning)
                        await this.SetPinInfoListAsync(this.FolderTree.First(), AppResources.Loading);
                }
                else
                {
                    base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                }
            }


            // Comes from setting page
            // If it is already signin skydrive, load files.
            // Otherwise, show signin button.
            bool isSkyDriveLogin = this.GetIsSignIn();
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

                    if (NetworkInterface.GetIsNetworkAvailable())
                        await this.SetPinInfoListAsync(null, AppResources.Loading);
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
                            await this.SetNearSpaceListAsync(AppResources.Loading);
                    }
                    else
                    {
                        base.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.InternetUnavailableMessage, uiNearSpaceMessage);
                    }
                    break;


                case PIN_INFO_PIVOT:
                    // Set pin app bar button and cloud setting menu items
                    ApplicationBar.Buttons.Add(this.PinInfoAppBarButton);
                    ApplicationBar.MenuItems.Add(this.skyDriveAppBarButton);
                    ApplicationBar.MenuItems.Add(this.dropboxAppBarButton);

                    // Set Cloud Mode Text
                    uiCurrentCloudModeText.Visibility = Visibility.Visible;


                    // If it wasn't already signed in, show signin button.
                    // Otherwise, load files
                    bool isSignIn = this.GetIsSignIn();
                    if (!isSignIn)  // wasn't signed in.
                    {
                        uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                        uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                    }
                    else  // already signed in.
                    {
                        uiPinInfoListGrid.Visibility = Visibility.Visible;
                        uiPinInfoSignInGrid.Visibility = Visibility.Collapsed;

                        if (NetworkInterface.GetIsNetworkAvailable())
                        {
                            if (!this.IsFileObjectLoaded && !this.IsSignIning)
                            {
                                this.SetIStorageManager();
                                await this.SetPinInfoListAsync(null, AppResources.Loading);
                            }
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
                        await this.SetNearSpaceListAsync(AppResources.Refreshing);
                    else
                        base.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.InternetUnavailableMessage, uiNearSpaceMessage);
                    break;

                case PIN_INFO_PIVOT:
                    // If it is in sign in process, don't refresh.
                    if (!this.IsSignIning)
                    {
                        // Refresh only in was already signed in.
                        if (uiPinInfoListGrid.Visibility == Visibility.Visible)
                        {
                            if (NetworkInterface.GetIsNetworkAvailable())
                            {
                                this.SetIStorageManager();
                                await this.SetPinInfoListAsync(null, AppResources.Refreshing);
                            }
                            else
                            {
                                base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                            }
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


        private async Task SetNearSpaceListAsync(string message)
        {
            // Before go load, set mutex to true.
            this.NearSpaceViewModel.IsDataLoaded = true;

            // Show progress indicator 
            base.SetListUnableAndShowMessage(uiNearSpaceList, message, uiNearSpaceMessage);
            base.SetProgressIndicator(true);


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
                            base.Dispatcher.BeginInvoke(() =>
                            {
                                this.NearSpaceViewModel.SetItems(spaces, currentGeoposition);
                                uiNearSpaceList.Visibility = Visibility.Visible;
                                uiNearSpaceMessage.Visibility = Visibility.Collapsed;
                            });
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
            base.SetProgressIndicator(false);
        }



        /*** Pin Pivot ***/

        private async void skyDriveAppBarButton_Click(object sender, EventArgs e)
        {
            // If it is not in sky drive mode, change it.
            if (!uiCurrentCloudModeText.Text.Equals(App.PLATFORMS[App.SKY_DRIVE_LOCATION_KEY]))
            {
                App.IStorageManager = App.IStorageManagers[App.SKY_DRIVE_LOCATION_KEY];
                uiCurrentCloudModeText.Text = App.PLATFORMS[App.SKY_DRIVE_LOCATION_KEY];

                if (!this.IsSignIning && !this.IsFileObjectLoading)
                {
                    // If it is already signin skydrive, load files.
                    // Otherwise, show signin button.
                    bool isSkyDriveLogin = false;
                    App.ApplicationSettings.TryGetValue<bool>(Account.ACCOUNT_IS_SIGN_IN_KEYS[App.SKY_DRIVE_LOCATION_KEY], out isSkyDriveLogin);
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

                        if (NetworkInterface.GetIsNetworkAvailable())
                            await this.SetPinInfoListAsync(null, AppResources.Loading);
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
            if (!uiCurrentCloudModeText.Text.Equals(App.PLATFORMS[App.DROPBOX_LOCATION_KEY]))
            {
                uiCurrentCloudModeText.Text = App.PLATFORMS[App.DROPBOX_LOCATION_KEY];
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


        protected override async void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (uiExplorerPivot.SelectedIndex == PIN_INFO_PIVOT)
            {
                if (this.FolderTree.Count <= 1)
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                    if (!IsFileObjectLoading)
                        await this.TreeUp();
                }
            }
        }


        private async void uiPinInfoListUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.FolderTree.Count > 1)
                await this.TreeUp();
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
                    this.SelectedFile.Clear();
                    this.PinInfoAppBarButton.IsEnabled = false;
                    await this.SetPinInfoListAsync(fileObject, AppResources.Loading);
                    MyDebug.WriteLine(fileObject.Name);
                }
                else  // Do selection if file
                {
                    if (fileObject.SelectCheckImage.Equals(FileObject.CHECK_NOT_IMAGE_PATH))
                    {
                        this.SelectedFile.Add(fileObject);
                        fileObject.SetSelectCheckImage(true);
                        this.PinInfoAppBarButton.IsEnabled = true;
                    }

                    else
                    {
                        this.SelectedFile.Remove(fileObject);
                        fileObject.SetSelectCheckImage(false);
                        if (this.SelectedFile.Count <= 0)
                            this.PinInfoAppBarButton.IsEnabled = false;
                    }
                    MyDebug.WriteLine(fileObject.Name);
                }
            }
        }


        private string GetCurrentPath()
        {
            FileObject[] array = this.FolderTree.Reverse<FileObject>().ToArray<FileObject>();
            string str = "";
            foreach (FileObject f in array)
                str += f.Name + AppResources.RootPath;
            return str;
        }


        private async Task TreeUp()
        {
            this.FolderTree.Pop();
            this.SelectedFile.Clear();
            this.PinInfoAppBarButton.IsEnabled = false;
            await this.SetPinInfoListAsync(this.FolderTree.First(), AppResources.Loading);
        }


        private async void uiPinInfoSignInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // If Internet available, Set pin list with root folder file list.
            // Otherwise, show internet bad message
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Show Loading message and save is login true for pivot moving action while sign in.
                uiPinInfoListGrid.Visibility = Visibility.Visible;
                uiPinInfoSignInGrid.Visibility = Visibility.Collapsed;

                this.SetIStorageManager();
                if (await this.SignIn(this))
                    await this.SetPinInfoListAsync(null, AppResources.Loading);
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }

        private async Task<bool> SignIn(DependencyObject context)
        {
            this.IsSignIning = true;
            base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.DoingSignIn, uiPinInfoMessage);

            bool result = await App.IStorageManager.SignIn(context);
            if (!result)
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(AppResources.BadSignInMessage, AppResources.BadSignInCaption, MessageBoxButton.OK);
                    uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                    uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                });
            }

            this.IsSignIning = false;
            return result;
        }


        private async Task SetPinInfoListAsync(FileObject folder, string message)
        {
            // Set Mutex true and Show Process Indicator
            this.IsFileObjectLoaded = true;
            this.IsFileObjectLoading = true;
            base.SetListUnableAndShowMessage(uiPinInfoList, message, uiPinInfoMessage);
            base.SetProgressIndicator(true);


            // If folder null, set root.
            if (folder == null)
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiPinInfoCurrentPath.Text = "";
                });
                this.FolderTree.Clear();
                this.SelectedFile.Clear();
                folder = await App.IStorageManager.GetRootFolderAsync();
            }

            if (!this.FolderTree.Contains(folder))
                this.FolderTree.Push(folder);
            List<FileObject> files = await App.IStorageManager.GetFilesFromFolderAsync(folder.Id);

            // If there exists file, return that.
            if (files.Count > 0)
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiPinInfoCurrentPath.Text = this.GetCurrentPath();

                    // Set file tree to list.
                    uiPinInfoList.DataContext = new ObservableCollection<FileObject>(files);
                    uiPinInfoList.Visibility = Visibility.Visible;
                    uiPinInfoMessage.Visibility = Visibility.Collapsed;
                });
            }
            else
            {
                base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.NoFileInCloudMessage, uiPinInfoMessage);
            }


            // Set Mutex false and Hide Process Indicator
            base.SetProgressIndicator(false);
            this.IsFileObjectLoading = false;
        }


        private void SetIStorageManager()
        {
            for (int i = 0; i < App.PLATFORMS.Length; i++)
            {
                if (uiCurrentCloudModeText.Text.Equals(App.PLATFORMS[i]))
                {
                    App.IStorageManager = App.IStorageManagers[i];
                    break;
                }
            }
        }


        private bool GetIsSignIn()
        {
            bool isSignIn = false;
            for (int i = 0; i < App.PLATFORMS.Length; i++)
            {
                if (uiCurrentCloudModeText.Text.Equals(App.PLATFORMS[i]))
                {
                    if (!App.ApplicationSettings.TryGetValue<bool>(Account.ACCOUNT_IS_SIGN_IN_KEYS[i], out isSignIn))
                        return false;
                }
            }
            return isSignIn;
        }
    }
}
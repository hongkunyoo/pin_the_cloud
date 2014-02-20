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
        // Const Instances
        private const string PIN_APP_BAR_BUTTON_ICON_URI = "/Assets/pajeon/png/general_bar_upload.png";

        private const int PIN_APP_BAR_BUTTON_INDEX = 1;
        private const int SKY_DRIVE_APP_BAR_MENUITEMS_INDEX = 1;
        private const int DROPBOX_APP_BAR_MENUITEMS_INDEX = 2;

        public const int PICK_PIVOT_INDEX = 0;
        public const int PIN_INFO_PIVOT_INDEX = 1;
        public const string SELECTED_FILE_KEY = "SELECTED_FILE_KEY";


        // Instances
        private bool IsFileObjectLoaded = false;
        private bool IsFileObjectLoading = false;
        private int CurrentPlatformIndex = 0;

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


            /*** Pin Pivot ***/

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


            // Check main platform at frist login and set cloud mode
            int mainPlatformType = 0;
            App.ApplicationSettings.TryGetValue<int>(Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY, out mainPlatformType);
            uiCurrentPlatformText.Text = App.PLATFORMS[mainPlatformType];
            this.CurrentPlatformIndex = mainPlatformType;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Check if it is on the backstack from SplashPage and remove that.
            if (NavigationService.BackStack.Count() == 1)
                NavigationService.RemoveBackEntry();


            // Comes from settings page
            if (PREVIOUS_PAGE.Equals(SETTINGS_PAGE))
            {
                /*** Pin Pivot ***/

                // If it is already signin skydrive, load files.
                // Otherwise, show signin button.
                if (!this.GetIsSignIn())
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
                            this.SetPinInfoListAsync(null, AppResources.Loading);
                        else
                            base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                    }
                }
            }


            // Comes from file list page
            if (PREVIOUS_PAGE.Equals(FILE_LIST_PAGE))
            {
                /*** Pin Pivot ***/

                if (NetworkInterface.GetIsNetworkAvailable())
                    this.SetPinInfoListAsync(this.FolderTree.First(), AppResources.Loading);
                else
                    base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
            }
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        // Construct pivot item by page index
        private void uiExplorerPivot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Set View model for dispaly,
            // One time loading.
            switch (uiExplorerPivot.SelectedIndex)
            {
                case PICK_PIVOT_INDEX:
                    // Remove button and menuitems
                    ApplicationBar.Buttons.RemoveAt(PIN_APP_BAR_BUTTON_INDEX);
                    ApplicationBar.MenuItems.RemoveAt(DROPBOX_APP_BAR_MENUITEMS_INDEX);
                    ApplicationBar.MenuItems.RemoveAt(SKY_DRIVE_APP_BAR_MENUITEMS_INDEX);

                    // Remove Cloud Kind Image
                    uiCurrentPlatformText.Visibility = Visibility.Collapsed;


                    // If Internet available, Set space list
                    // Otherwise, show internet bad message
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        if (!this.NearSpaceViewModel.IsDataLoaded)  // Mutex check
                            this.SetNearSpaceListAsync(AppResources.Loading);
                    }
                    else
                    {
                        base.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.InternetUnavailableMessage, uiNearSpaceMessage);
                    }
                    break;


                case PIN_INFO_PIVOT_INDEX:
                    // Set pin app bar button and cloud setting menu items
                    ApplicationBar.Buttons.Add(this.PinInfoAppBarButton);
                    ApplicationBar.MenuItems.Add(this.skyDriveAppBarButton);
                    ApplicationBar.MenuItems.Add(this.dropboxAppBarButton);

                    // Set Cloud Mode Text
                    uiCurrentPlatformText.Visibility = Visibility.Visible;


                    // If it wasn't already signed in, show signin button.
                    // Otherwise, load files
                    if (!this.GetIsSignIn())  // wasn't signed in.
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
                            if (!this.IsFileObjectLoaded)
                            {
                                App.IStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
                                this.SetPinInfoListAsync(null, AppResources.Loading);
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
        private void uiAppBarRefreshMenuItem_Click(object sender, System.EventArgs e)
        {
            switch (uiExplorerPivot.SelectedIndex)
            {
                case PICK_PIVOT_INDEX:
                    if (NetworkInterface.GetIsNetworkAvailable())
                        this.SetNearSpaceListAsync(AppResources.Refreshing);
                    else
                        base.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.InternetUnavailableMessage, uiNearSpaceMessage);
                    break;

                case PIN_INFO_PIVOT_INDEX:
                    // Refresh only in was already signed in.
                    if (uiPinInfoListGrid.Visibility == Visibility.Visible)
                    {
                        if (NetworkInterface.GetIsNetworkAvailable())
                        {
                            App.IStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
                            this.SetPinInfoListAsync(null, AppResources.Refreshing);
                        }
                        else
                        {
                            base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                        }
                    }
                    break;
            }
        }


        // Move to Setting Page
        private void uiAppBarSettingsButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri(SETTINGS_PAGE, UriKind.Relative));
        }


        public bool GetLocationAccessConsent()
        {
            bool locationAccess = false;
            App.ApplicationSettings.TryGetValue<bool>(Account.LOCATION_ACCESS_CONSENT_KEY, out locationAccess);
            if (!locationAccess)  // First or not consented of access in location information.
            {
                MessageBoxResult result = MessageBox.Show(AppResources.LocationAccessMessage, AppResources.LocationAccess, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                    App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT_KEY] = true;
                else
                    App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT_KEY] = false;
                App.ApplicationSettings.Save();
            }
            return (bool)App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT_KEY];
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
                string parameters = base.GetParameterStringFromSpaceViewItem(spaceViewItem);
                NavigationService.Navigate(new Uri(FILE_LIST_PAGE + parameters + "&pivot=" + PICK_PIVOT_INDEX, UriKind.Relative));
            }
        }


        private async void SetNearSpaceListAsync(string message)
        {
            // Before go load, set mutex to true.
            this.NearSpaceViewModel.IsDataLoaded = true;

            // Show progress indicator 
            base.SetListUnableAndShowMessage(uiNearSpaceList, message, uiNearSpaceMessage);
            base.SetProgressIndicator(true);


            // Check whether user consented for location access.
            if (this.GetLocationAccessConsent())  // Got consent of location access.
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
                                this.NearSpaceViewModel.SetItems(spaces);
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

        private void skyDriveAppBarButton_Click(object sender, EventArgs e)
        {
            // If it is not in sky drive mode, change it.
            if (this.CurrentPlatformIndex != App.SKY_DRIVE_KEY_INDEX)
            {
                if (!this.IsFileObjectLoading)
                {
                    App.IStorageManager = App.IStorageManagers[App.SKY_DRIVE_KEY_INDEX];
                    uiCurrentPlatformText.Text = App.PLATFORMS[App.SKY_DRIVE_KEY_INDEX];
                    this.CurrentPlatformIndex = App.SKY_DRIVE_KEY_INDEX;

                    // If it is already signin skydrive, load files.
                    // Otherwise, show signin button.
                    bool isSkyDriveLogin = false;
                    App.ApplicationSettings.TryGetValue<bool>(Account.ACCOUNT_IS_SIGN_IN_KEYS[App.SKY_DRIVE_KEY_INDEX], out isSkyDriveLogin);
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
                            this.SetPinInfoListAsync(null, AppResources.Loading);
                        else
                            base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                    }
                }
            }
        }


        private void dropboxAppBarButton_Click(object sender, EventArgs e)
        {
            //TODO
        }


        private void pinInfoAppBarButton_Click(object sender, EventArgs e)
        {
            // Check whether user consented for location access.
            if (this.GetLocationAccessConsent())  // Got consent of location access.
            {
                // Check whether GPS is on or not
                if (App.GeoCalculateManager.GetGeolocatorPositionStatus())  // GPS is on
                {
                    PhoneApplicationService.Current.State[SELECTED_FILE_KEY] = this.SelectedFile;
                    this.SelectedFile.Clear();
                    this.PinInfoAppBarButton.IsEnabled = false;
                    NavigationService.Navigate(new Uri(FILE_LIST_PAGE + "&pivot=" + PIN_INFO_PIVOT_INDEX, UriKind.Relative));
                }
                else  // GPS is off
                {
                    MessageBox.Show(AppResources.NoGpsOnMessage, AppResources.NoGpsOnCaption, MessageBoxButton.OK);
                }
            }
            else  // First or not consented of access in location information.
            {
                MessageBox.Show(AppResources.NoLocationAcessConsentMessage, AppResources.NoLocationAcessConsentCaption, MessageBoxButton.OK);
            }
        }


        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (uiExplorerPivot.SelectedIndex == PIN_INFO_PIVOT_INDEX)
            {
                if (this.FolderTree.Count <= 1)
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                    if (!IsFileObjectLoading)
                        this.TreeUp();
                }
            }
        }


        private void uiPinInfoListUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.FolderTree.Count > 1)
                this.TreeUp();
        }


        // Pin file selection event.
        private void uiPinInfoList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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
                //if (fileObject.ThumnailType.Equals(AppResources.Folder))
                if ("".Equals(AppResources.Folder))
                {
                    this.SelectedFile.Clear();
                    this.PinInfoAppBarButton.IsEnabled = false;
                    this.SetPinInfoListAsync(fileObject, AppResources.Loading);
                    MyDebug.WriteLine(fileObject.Name);
                }
                else  // Do selection if file
                {
                    //if (fileObject.SelectCheckImage.Equals(FileObject.CHECK_NOT_IMAGE_PATH))
                    if(false)
                    {
                        this.SelectedFile.Add(fileObject);
                        //fileObject.SetSelectCheckImage(true);
                        this.PinInfoAppBarButton.IsEnabled = true;
                    }

                    else
                    {
                        this.SelectedFile.Remove(fileObject);
                        //fileObject.SetSelectCheckImage(false);
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


        private void TreeUp()
        {
            this.FolderTree.Pop();
            this.SelectedFile.Clear();
            this.PinInfoAppBarButton.IsEnabled = false;
            this.SetPinInfoListAsync(this.FolderTree.First(), AppResources.Loading);
        }


        private async void uiPinInfoSignInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // If Internet available, Set pin list with root folder file list.
            // Otherwise, show internet bad message
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Show Loading message and save is login true for pivot moving action while sign in.
                base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.DoingSignIn, uiPinInfoMessage);
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiPinInfoListGrid.Visibility = Visibility.Visible;
                    uiPinInfoSignInGrid.Visibility = Visibility.Collapsed;
                });

                App.IStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
                Task signInTask = App.IStorageManager.SignIn();
                App.TaskManager.AddSignInTask(signInTask, this.CurrentPlatformIndex);
                await App.TaskManager.WaitSignInTask(this.CurrentPlatformIndex);
                if (App.IStorageManager.GetCurrentAccount() != null)
                {
                    this.SetPinInfoListAsync(null, AppResources.Loading);
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(AppResources.BadSignInMessage, AppResources.BadSignInCaption, MessageBoxButton.OK);
                        uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                        uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                    });
                }
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private async void SetPinInfoListAsync(FileObject folder, string message)
        {
            // Set Mutex true and Show Process Indicator
            this.IsFileObjectLoaded = true;
            this.IsFileObjectLoading = true;
            base.SetListUnableAndShowMessage(uiPinInfoList, message, uiPinInfoMessage);
            base.SetProgressIndicator(true);

            // Wait tasks
            await App.TaskManager.WaitSignInTask(this.CurrentPlatformIndex);
            await App.TaskManager.WaitTask(TaskManager.SIGN_OUT_TASK_KEY);


            // If it haven't signed out before working below code, do it.
            if (App.SkyDriveManager.GetCurrentAccount() != null)
            {
                // If folder null, set root.
                if (folder == null)
                {
                    this.Dispatcher.BeginInvoke(() =>
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
            }


            // Set Mutex false and Hide Process Indicator
            base.SetProgressIndicator(false);
            this.IsFileObjectLoading = false;
        }


        private bool GetIsSignIn()
        {
            bool isSignIn = false;
            if (!App.ApplicationSettings.TryGetValue<bool>(Account.ACCOUNT_IS_SIGN_IN_KEYS[CurrentPlatformIndex], out isSignIn))
                return false;
            return isSignIn;
        }
    }
}
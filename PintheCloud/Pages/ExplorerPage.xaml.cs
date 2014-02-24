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

        public const int PICK_PIVOT_INDEX = 0;
        public const int PIN_INFO_PIVOT_INDEX = 1;
        public const string SELECTED_FILE_KEY = "SELECTED_FILE_KEY";


        // Instances
        private int MainPlatformIndex = 0;
        private int CurrentPlatformIndex = 0;

        private ApplicationBarIconButton PinInfoAppBarButton = new ApplicationBarIconButton();
        private ApplicationBarMenuItem[] AppBarMenuItems = null;

        private SpaceViewModel NearSpaceViewModel = new SpaceViewModel();
        private FileObjectViewModel FileObjectViewModel = new FileObjectViewModel();

        private Stack<FileObjectViewItem> FolderTree = new Stack<FileObjectViewItem>();
        public List<FileObjectViewItem> SelectedFile = new List<FileObjectViewItem>();


        // Constructer
        public ExplorerPage()
        {
            InitializeComponent();

            // Set pin button
            this.PinInfoAppBarButton.Text = AppResources.Pin;
            this.PinInfoAppBarButton.IconUri = new Uri(PIN_APP_BAR_BUTTON_ICON_URI, UriKind.Relative);
            this.PinInfoAppBarButton.IsEnabled = false;
            this.PinInfoAppBarButton.Click += PinInfoAppBarButton_Click;

            // Set Cloud Setting selection.
            this.AppBarMenuItems = new ApplicationBarMenuItem[App.IStorageManagers.Length];
            for (int i = 0; i < this.AppBarMenuItems.Length; i++)
            {
                this.AppBarMenuItems[i] = new ApplicationBarMenuItem();
                this.AppBarMenuItems[i].Text = App.IStorageManagers[i].GetStorageName();
                this.AppBarMenuItems[i].Click += AppBarMenuItem_Click;
            }


            // Check main platform and set current platform index.
            this.MainPlatformIndex = (int)App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY];
            uiCurrentPlatformText.Text = App.IStorageManagers[this.MainPlatformIndex].GetStorageName();
            this.CurrentPlatformIndex = this.MainPlatformIndex;

            // Set Datacontext
            uiNearSpaceList.DataContext = this.NearSpaceViewModel;
            uiPinInfoList.DataContext = this.FileObjectViewModel;
        }


        void ExplorerPage_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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

                if (uiExplorerPivot.SelectedIndex == PIN_INFO_PIVOT_INDEX)
                {
                    // If it is already signin skydrive, load files.
                    // Otherwise, show signin button.
                    IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
                    if (!iStorageManager.IsSignIn())  // wasn't signed in.
                    {
                        if (uiPinInfoSignInGrid.Visibility == Visibility.Collapsed)
                        {
                            uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                            uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                        }
                    }
                    else  // was signed in.
                    {
                        if (uiPinInfoSignInGrid.Visibility == Visibility.Visible)
                        {
                            // Show Loading message and save is login true for pivot moving action while sign in.
                            uiPinInfoListGrid.Visibility = Visibility.Visible;
                            uiPinInfoSignInGrid.Visibility = Visibility.Collapsed;

                            if (NetworkInterface.GetIsNetworkAvailable())
                                this.SetPinInfoListAsync(null, AppResources.Loading, iStorageManager);
                            else
                                base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                        }
                    }
                }
            }


            // Comes from file list page
            if (PREVIOUS_PAGE.Equals(FILE_LIST_PAGE))
            {
                /*** Pick Pivot ***/

                // TODO Refresh

                if (uiExplorerPivot.SelectedIndex == PICK_PIVOT_INDEX)
                {
                    // If Internet available, Set space list
                    // Otherwise, show internet bad message
                    if (NetworkInterface.GetIsNetworkAvailable())
                            this.SetNearSpaceListAsync(AppResources.Loading);
                    else
                        base.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.InternetUnavailableMessage, uiNearSpaceMessage);
                }



                /*** Pin Pivot ***/

                else if (uiExplorerPivot.SelectedIndex == PIN_INFO_PIVOT_INDEX)
                {
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        this.SelectedFile.Clear();
                        this.PinInfoAppBarButton.IsEnabled = false;

                        IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
                        this.SetPinInfoListAsync(this.FolderTree.First(), AppResources.Loading, iStorageManager);
                    }
                    else
                    {
                        base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                    }
                }
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
                    // Remove button and menuitems and Cloud Mode text
                    ApplicationBar.Buttons.RemoveAt(PIN_APP_BAR_BUTTON_INDEX);
                    for (int i = 0; i < AppBarMenuItems.Length; i++)
                        ApplicationBar.MenuItems.Remove(this.AppBarMenuItems[i]);
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
                    // Set button and menuitems and Cloud Mode text
                    ApplicationBar.Buttons.Add(this.PinInfoAppBarButton);
                    for (int i = 0; i < AppBarMenuItems.Length; i++)
                        ApplicationBar.MenuItems.Add(this.AppBarMenuItems[i]);
                    uiCurrentPlatformText.Visibility = Visibility.Visible;


                    // If it wasn't already signed in, show signin button.
                    // Otherwise, load files
                    IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
                    if (!iStorageManager.IsSignIn())  // wasn't signed in.
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
                            if (!this.FileObjectViewModel.IsDataLoaded)
                                this.SetPinInfoListAsync(null, AppResources.Loading, iStorageManager);
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
                            IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
                            this.SetPinInfoListAsync(null, AppResources.Refreshing, iStorageManager);
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
                NavigationService.Navigate(new Uri(FILE_LIST_PAGE + parameters + "&pivot=" + 
                    PICK_PIVOT_INDEX + "&platform=" + this.MainPlatformIndex, UriKind.Relative));
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
                                uiNearSpaceList.Visibility = Visibility.Visible;
                                uiNearSpaceMessage.Visibility = Visibility.Collapsed;
                                this.NearSpaceViewModel.SetItems(spaces);
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

        private void AppBarMenuItem_Click(object sender, EventArgs e)
        {
            // Get index
            ApplicationBarMenuItem appBarMenuItem = (ApplicationBarMenuItem)sender;
            int platformIndex = base.GetPlatformIndex(appBarMenuItem.Text);


            // If it is not in sky drive mode, change it.
            if (this.CurrentPlatformIndex != platformIndex && !this.FileObjectViewModel.IsDataLoading)
            {
                IStorageManager iStorageManager = App.IStorageManagers[platformIndex];
                uiCurrentPlatformText.Text = App.IStorageManagers[platformIndex].GetStorageName();
                this.CurrentPlatformIndex = platformIndex;

                // If it is already signin skydrive, load files.
                // Otherwise, show signin button.
                //bool isSignIn = false;
                //App.ApplicationSettings.TryGetValue<bool>(iStorageManager.GetAccountIsSignInKey(), out isSignIn);
                if (!iStorageManager.IsSignIn())
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
                        this.SetPinInfoListAsync(null, AppResources.Loading, iStorageManager);
                    else
                        base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                }
            }
        }


        private void PinInfoAppBarButton_Click(object sender, EventArgs e)
        {
            // Check whether user consented for location access.
            if (this.GetLocationAccessConsent())  // Got consent of location access.
            {
                // Check whether GPS is on or not
                if (App.GeoCalculateManager.GetGeolocatorPositionStatus())  // GPS is on
                {
                    this.NearSpaceViewModel.IsDataLoaded = false;
                    PhoneApplicationService.Current.State[SELECTED_FILE_KEY] = this.SelectedFile;
                    NavigationService.Navigate(new Uri(FILE_LIST_PAGE + "?pivot=" + PIN_INFO_PIVOT_INDEX + 
                        "&platform=" + this.CurrentPlatformIndex, UriKind.Relative));
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
                    if (!this.FileObjectViewModel.IsDataLoading)
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
            FileObjectViewItem fileObjectViewItem = uiPinInfoList.SelectedItem as FileObjectViewItem;

            // Set selected item to null for next selection of list item. 
            uiPinInfoList.SelectedItem = null;


            // If selected item isn't null, Do something
            if (fileObjectViewItem != null)
            {
                // If user select folder, go in.
                // Otherwise, add it to list.
                if (fileObjectViewItem.ThumnailType.Equals(FileObjectViewModel.FOLDER))
                {
                    this.SelectedFile.Clear();
                    this.PinInfoAppBarButton.IsEnabled = false;

                    IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
                    this.SetPinInfoListAsync(fileObjectViewItem, AppResources.Loading, iStorageManager);
                }
                else  // Do selection if file
                {
                    if (fileObjectViewItem.SelectCheckImage.Equals(FileObjectViewModel.CHECK_NOT_IMAGE_URI))
                    {
                        this.SelectedFile.Add(fileObjectViewItem);
                        fileObjectViewItem.SelectCheckImage = FileObjectViewModel.CHECK_IMAGE_URI;
                        this.PinInfoAppBarButton.IsEnabled = true;
                    }

                    else
                    {
                        this.SelectedFile.Remove(fileObjectViewItem);
                        fileObjectViewItem.SelectCheckImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                        if (this.SelectedFile.Count <= 0)
                            this.PinInfoAppBarButton.IsEnabled = false;
                    }
                }
            }
        }


        private string GetCurrentPath()
        {
            FileObjectViewItem[] array = this.FolderTree.Reverse<FileObjectViewItem>().ToArray<FileObjectViewItem>();
            string str = "";
            foreach (FileObjectViewItem f in array)
                str += f.Name + AppResources.RootPath;
            return str;
        }


        private void TreeUp()
        {
            this.FolderTree.Pop();
            this.SelectedFile.Clear();
            this.PinInfoAppBarButton.IsEnabled = false;

            IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
            this.SetPinInfoListAsync(this.FolderTree.First(), AppResources.Loading, iStorageManager);
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

                // Sign in and await that task.
                IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
                App.TaskManager.AddSignInTask(iStorageManager.SignIn(), this.CurrentPlatformIndex);
                await App.TaskManager.WaitSignInTask(this.CurrentPlatformIndex);

                // If sign in success, set list.
                // Otherwise, show bad sign in message box.
                if (iStorageManager.GetAccount() != null)
                {
                    this.SetPinInfoListAsync(null, AppResources.Loading, iStorageManager);
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


        private async void SetPinInfoListAsync(FileObjectViewItem folder, string message, IStorageManager iStorageManager)
        {
            // Set Mutex true and Show Process Indicator
            this.FileObjectViewModel.IsDataLoaded = true;
            this.FileObjectViewModel.IsDataLoading = true;
            base.SetListUnableAndShowMessage(uiPinInfoList, message, uiPinInfoMessage);
            base.SetProgressIndicator(true);

            // Wait tasks
            await App.TaskManager.WaitSignInTask(this.CurrentPlatformIndex);
            await App.TaskManager.WaitSignOutTask(this.CurrentPlatformIndex);


            // If it haven't signed out before working below code, do it.
            if (iStorageManager.GetAccount() != null)
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

                    FileObject rootFolder = await iStorageManager.GetRootFolderAsync();
                    folder = new FileObjectViewItem();
                    folder.Id = rootFolder.Id;
                }

                if (!this.FolderTree.Contains(folder))
                    this.FolderTree.Push(folder);
                List<FileObject> files = await iStorageManager.GetFilesFromFolderAsync(folder.Id);

                // If there exists file, return that.
                if (files.Count > 0)
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        // Set file tree to list.
                        uiPinInfoList.Visibility = Visibility.Visible;
                        uiPinInfoMessage.Visibility = Visibility.Collapsed;
                        uiPinInfoCurrentPath.Text = this.GetCurrentPath();
                        this.FileObjectViewModel.SetItems(files);
                    });
                }
                else
                {
                    base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.NoFileInCloudMessage, uiPinInfoMessage);
                }
            }


            // Set Mutex false and Hide Process Indicator
            base.SetProgressIndicator(false);
            this.FileObjectViewModel.IsDataLoading = false;
        }
    }
}
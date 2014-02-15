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
        private const string PIN_APP_BAR_BUTTON_ICON_URI = "/Assets/AppBar/png/general_bar_upload.png";
        private const int SETTING_APP_BAR_BUTTON = 0;
        private const int PIN_APP_BAR_BUTTON = 1;
        private const int SKY_DRIVE_APP_BAR_MENUITEMS = 1;
        private const int GOOGLE_DRIVE_APP_BAR_BUTTON = 2;


        // Instances
        private SpaceViewModel NearSpaceViewModel = new SpaceViewModel();
        private bool IsFileObjectLoaded = false;
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
                        this.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.InternetUnavailableMessage, uiNearSpaceMessage);
                }
            }

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
                        this.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                    }
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
                    ApplicationBar.MenuItems.RemoveAt(GOOGLE_DRIVE_APP_BAR_BUTTON);
                    ApplicationBar.MenuItems.RemoveAt(SKY_DRIVE_APP_BAR_MENUITEMS);

                    // Remove Cloud Kind Image
                    uiCurrentCloudKindText.Visibility = Visibility.Collapsed;

                    // If Internet available, Set space list
                    // Otherwise, show internet bad message
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        if (!NearSpaceViewModel.IsDataLoading)  // Mutex check
                            await this.SetExplorerPivotAsync(AppResources.Loading);
                    }
                    else
                    {
                        this.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.InternetUnavailableMessage, uiNearSpaceMessage);
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

                    ApplicationBarMenuItem googleDriveAppBarButton = new ApplicationBarMenuItem();
                    googleDriveAppBarButton.Text = AppResources.GoogleDrive;
                    googleDriveAppBarButton.Click += googleDriveAppBarButton_Click;
                    ApplicationBar.MenuItems.Add(googleDriveAppBarButton);


                    // Set Cloud Kind Image
                    App.CloudManager = App.SkyDriveManager;
                    uiCurrentCloudKindText.Visibility = Visibility.Visible;
                    uiCurrentCloudKindText.Text = AppResources.SkyDrive;

                    // If it is already signin skydrive, load files.
                    // Otherwise, show signin button.
                    bool isSkyDriveLogin = false;
                    App.ApplicationSettings.TryGetValue<bool>(Account.ACCOUNT_SKY_DRIVE_IS_LOGIN, out isSkyDriveLogin);
                    if (!isSkyDriveLogin)
                    {
                        uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                        uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        uiPinInfoListGrid.Visibility = Visibility.Visible;
                        uiPinInfoSignInGrid.Visibility = Visibility.Collapsed;

                        // If Internet available, Set pin list with root folder file list.
                        // Otherwise, show internet bad message
                        if (NetworkInterface.GetIsNetworkAvailable())
                        {
                            if (!this.IsFileObjectLoaded)  // Mutex check
                            {
                                this.SetListUnableAndShowMessage(uiPinInfoList, AppResources.Loading, uiPinInfoMessage);

                                // SIgn cloud manager. If success, show files.
                                // Otherwise, show error message
                                if (!this.IsSignIning)  // Mutex check
                                {
                                    if (!await this.InitialPinInfoListSetUp(this))
                                    {
                                        this.SetListUnableAndShowMessage(uiPinInfoList, AppResources.BadSignInMessage, uiPinInfoMessage);
                                    }
                                }
                            }
                        }
                        else
                        {
                            this.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
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
                    await this.SetExplorerPivotAsync(AppResources.Refreshing);
                    break;
                case PIN_PIVOT:
                    this.FolderTree.Clear();
                    this.SelectedFile.Clear();
                    await this.SetFileTreeForFolder(null, AppResources.Refreshing);
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
            this.SetListUnableAndShowMessage(uiNearSpaceList, message, uiNearSpaceMessage);
            PtcPage.SetProgressIndicator(this, true);

            // Before go load, set mutex to true.
            NearSpaceViewModel.IsDataLoading = true;


            // Check whether user consented for location access.
            if (base.GetLocationAccessConsent())  // Got consent of location access.
            {
                // Check whether GPS is on or not
                if (base.GetGeolocatorPositionStatus())  // GPS is on
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
                            this.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.NoNearSpaceMessage, uiNearSpaceMessage);
                            NearSpaceViewModel.IsDataLoading = false;  // Mutex
                        }
                    }
                    else  // works bad
                    {
                        // Show GPS off message box.
                        this.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.BadGpsMessage, uiNearSpaceMessage);
                        NearSpaceViewModel.IsDataLoading = false;  // Mutex
                    }
                }
                else  // GPS is off
                {
                    // Show GPS off message box.
                    this.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.NoGpsOnMessage, uiNearSpaceMessage);
                    NearSpaceViewModel.IsDataLoading = false;  // Mutex
                }
            }
            else  // First or not consented of access in location information.
            {
                // Show no consent message box.
                this.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.NoLocationAcessConsentMessage, uiNearSpaceMessage);
                NearSpaceViewModel.IsDataLoading = false;  // Mutex
            }


            // Hide progress indicator
            PtcPage.SetProgressIndicator(this, false);
        }



        /*** Pin Pivot ***/

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            switch (uiExplorerPivot.SelectedIndex)
            {
                case PIN_PIVOT:
                    if (this.FolderTree.Count == 1)
                    {
                        e.Cancel = false;
                        NavigationService.GoBack();
                    }
                    else
                    {
                        this.TreeUp();
                        e.Cancel = true;
                    }
                    base.OnBackKeyPress(e);
                    break;
            }
        }


        private void skyDriveAppBarButton_Click(object sender, EventArgs e)
        {
            uiCurrentCloudKindText.Text = AppResources.SkyDrive;
        }


        private void googleDriveAppBarButton_Click(object sender, EventArgs e)
        {
            uiCurrentCloudKindText.Text = AppResources.GoogleDrive;
        }


        private void pinInfoAppBarButton_Click(object sender, EventArgs e)
        {
            if (this.SelectedFile.Count > 0)
            {
                PhoneApplicationService.Current.State["SELECTED_FILE"] = this.SelectedFile;
                NavigationService.Navigate(new Uri(PtcPage.FILE_LIST_PAGE, UriKind.Relative));
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
                        ((ApplicationBarIconButton) ApplicationBar.Buttons[PIN_APP_BAR_BUTTON]).IsEnabled = true;
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
            this.SetListUnableAndShowMessage(uiPinInfoList, message, uiPinInfoMessage);
            PtcPage.SetProgressIndicator(this, true);

            // Before go load, set mutex to true.
            this.IsFileObjectLoaded = true;

            // If folder null, set root.
            if (folder == null)
            {
                uiPinInfoCurrentPath.Text = "";
                folder = await App.CloudManager.GetRootFolderAsync();
            }

            if (!this.FolderTree.Contains(folder))
                this.FolderTree.Push(folder);
            List<FileObject> files = await App.CloudManager.GetFilesFromFolderAsync(folder.Id);

            // Hide progress indicator
            PtcPage.SetProgressIndicator(this, false);

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
                this.SetListUnableAndShowMessage(uiPinInfoList, AppResources.NoFileInCloudMessage, uiPinInfoMessage);
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


        private async void TreeUp()
        {
            if (this.FolderTree.Count > 1)
            {
                this.FolderTree.Pop();
                await this.SetFileTreeForFolder(this.FolderTree.First(), AppResources.Loading);
            }
        }


        // Signin. It includes set mutex.
        private async Task<bool> InitialPinInfoListSetUp(DependencyObject context)
        {
            this.IsSignIning = true;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[SETTING_APP_BAR_BUTTON]).IsEnabled = false;
            bool result = await App.CloudManager.SignIn(context);
            if (result)
            {
                this.FolderTree.Clear();
                this.SelectedFile.Clear();
                await this.SetFileTreeForFolder(null, AppResources.Loading);
            }
            ((ApplicationBarIconButton)ApplicationBar.Buttons[SETTING_APP_BAR_BUTTON]).IsEnabled = true;
            this.IsSignIning = false;
            return result;
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
                this.SetListUnableAndShowMessage(uiPinInfoList, AppResources.Loading, uiPinInfoMessage);
                App.ApplicationSettings[Account.ACCOUNT_SKY_DRIVE_IS_LOGIN] = true;
                App.ApplicationSettings.Save();

                // SIgn cloud manager. If success, show files.
                // Otherwise, show error message
                if (!await this.InitialPinInfoListSetUp(this))
                {
                    MessageBox.Show(AppResources.BadSignInMessage, AppResources.BadSignInCaption, MessageBoxButton.OK);
                    uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                    uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                    App.ApplicationSettings.Remove(Account.ACCOUNT_SKY_DRIVE_IS_LOGIN);
                }
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
                uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                App.ApplicationSettings.Remove(Account.ACCOUNT_SKY_DRIVE_IS_LOGIN);
            }
        }



        /*** Private Method ***/

        private void SetListUnableAndShowMessage(LongListSelector list, string message, TextBlock messageTextBlock)
        {
            list.Visibility = Visibility.Collapsed;
            messageTextBlock.Text = message;
            messageTextBlock.Visibility = Visibility.Visible;
        }
    }
}
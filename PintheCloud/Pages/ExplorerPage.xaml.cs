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
        private const int PICK_PIVOT = 0;
        private const int PIN_PIVOT = 1;
        private const int CONFIRM_APP_BAR_BUTTON = 1;

        private bool IsLikeButtonClicked = false;

        private const string CONFIRM_APP_BAR_BUTTON_ICON_URI = "/Assets/AppBar/check.png";


        // Instances
        private SpaceViewModel NearSpaceViewModel = new SpaceViewModel();
        private bool FileObjectIsDataLoading = false;
        private Stack<FileObject> FolderTree = new Stack<FileObject>();
        private List<FileObject> SelectedFile = new List<FileObject>();


        public ExplorerPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Check if it is on the backstack from SplashPage and remove that.
            if (NavigationService.BackStack.Count() == 1)
                NavigationService.RemoveBackEntry();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        // Construct pivot item by page index
        private async void uiExplorerPivot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Set View model for dispaly,
            // One time loading.
            switch (uiExplorerPivot.SelectedIndex)
            {
                case PICK_PIVOT:
                    // Remove confirm button
                    ApplicationBar.Buttons.RemoveAt(CONFIRM_APP_BAR_BUTTON);

                    // If Internet available, Set space list
                    // Otherwise, show internet bad message
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        if (!NearSpaceViewModel.IsDataLoading)  // Mutex check
                            await this.SetExplorerPivotAsync(AppResources.Loading);
                    }
                    else
                    {
                        uiNearSpaceList.Visibility = Visibility.Collapsed;
                        uiNearSpaceMessage.Text = AppResources.InternetUnavailableMessage;
                        uiNearSpaceMessage.Visibility = Visibility.Visible;
                    }
                    break;

                case PIN_PIVOT:
                    // Set confirm button
                    ApplicationBarIconButton confirmAppBarButton = new ApplicationBarIconButton();
                    confirmAppBarButton.Text = AppResources.Confirm;
                    confirmAppBarButton.IconUri = new Uri(CONFIRM_APP_BAR_BUTTON_ICON_URI, UriKind.Relative);
                    confirmAppBarButton.Click += uiAppBarConfirmButton_Click;
                    ApplicationBar.Buttons.Add(confirmAppBarButton);

                    // If Internet available, Set pin list with root folder file list.
                    // Otherwise, show internet bad message
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        if (!this.FileObjectIsDataLoading)  // Mutex check
                            await this.SetTreeForFolder(await App.SkyDriveManager.GetRootFolderAsync(), AppResources.Loading);
                    }
                    else
                    {
                        uiPinFileList.Visibility = Visibility.Collapsed;
                        uiPinFileMessage.Text = AppResources.InternetUnavailableMessage;
                        uiPinFileMessage.Visibility = Visibility.Visible;
                    }
                    break;
            }
        }



        /*** Pick Pivot ***/

        // List select event
        private async void uiNearSpaceList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected Space View Item
            SpaceViewItem spaceViewItem = uiNearSpaceList.SelectedItem as SpaceViewItem;

            // Set selected item to null for next selection of list item. 
            uiNearSpaceList.SelectedItem = null;

            // If selected item isn't null and it doesn't come from like button, goto File list page.
            // Otherwise, Process Like or Not Like by current state
            if (spaceViewItem != null && !this.IsLikeButtonClicked)  // Go to FIle List Page
            {
                string parameters = App.SpaceManager.GetParameterStringFromSpaceViewItem(spaceViewItem);
                NavigationService.Navigate(new Uri(PtcPage.FILE_LIST_PAGE + parameters, UriKind.Relative));
            }

            else if (spaceViewItem != null && this.IsLikeButtonClicked)  // Do like
            {
                // Set is like button clicked false for next click.
                this.IsLikeButtonClicked = false;

                // Get different Account Space Relation Worker by internet state.
                if (NetworkInterface.GetIsNetworkAvailable()) //  Internet available.
                {
                    App.AccountSpaceRelationManager.SetAccountSpaceRelationWorker(new AccountSpaceRelationInternetAvailableWorker());
                    await this.LikeAsync(spaceViewItem);
                }
                else  // Internet bad
                {
                    MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
                }
            }
        }


        // Process Like or Not Like by current state
        private void uiSpaceLikeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Set this is like button click
            // It is required to seperate between space button and like button
            this.IsLikeButtonClicked = true;
        }


        // Move to Setting Page
        private void uiAppBarSettingsButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri(PtcPage.SETTINGS_PAGE, UriKind.Relative));
        }


        // Refresh space list.
        private async void uiAppBarRefreshButton_Click(object sender, System.EventArgs e)
        {
            switch (uiExplorerPivot.SelectedIndex)
            {
                case PICK_PIVOT:
                    await this.SetExplorerPivotAsync(AppResources.Refreshing);
                    break;
                case PIN_PIVOT:
                    this.FolderTree.Clear();
                    await this.SetTreeForFolder(await App.SkyDriveManager.GetRootFolderAsync(), AppResources.Loading);
                    break;
            }
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


        // Pin file selection event.
        private async void uiPinFileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 1)
            {
                FileObject fileObject = (FileObject)e.AddedItems[0];

                // If user select folder, go in.
                // Otherwise, add it to list.
                if (fileObject.ThumnailType.Equals(AppResources.Folder))
                {
                    await this.SetTreeForFolder(fileObject, AppResources.Loading);
                    MyDebug.WriteLine(fileObject.Name);
                }
                else  // Do selection if file
                {
                    this.SelectedFile.Add(fileObject);
                    fileObject.SetSelectCheck(fileObject.SelectCheckImage);
                    MyDebug.WriteLine(fileObject.Name);
                }
            }
        }


        private void uiAppBarConfirmButton_Click(object sender, System.EventArgs e)
        {
            PhoneApplicationService.Current.State["SELECTED_FILE"] = this.SelectedFile;
            NavigationService.Navigate(new Uri("/Utilities/TestDrive.xaml", UriKind.Relative));
        }



        /*** Self Method ***/

        private async Task SetExplorerPivotAsync(string message)
        {
            // Set worker and show loading message
            App.SpaceManager.SetAccountWorker(new SpaceInternetAvailableWorker());
            App.AccountSpaceRelationManager.SetAccountSpaceRelationWorker(new AccountSpaceRelationInternetAvailableWorker());

            // Show progress indicator 
            uiNearSpaceList.Visibility = Visibility.Collapsed;
            uiNearSpaceMessage.Text = message;
            uiNearSpaceMessage.Visibility = Visibility.Visible;
            base.SetProgressIndicator(true);

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
                            uiNearSpaceList.Visibility = Visibility.Collapsed;
                            uiNearSpaceMessage.Text = AppResources.NoNearSpaceMessage;
                            uiNearSpaceMessage.Visibility = Visibility.Visible;
                            NearSpaceViewModel.IsDataLoading = false;  // Mutex
                        }
                    }
                    else  // works bad
                    {
                        // Show GPS off message box.
                        uiNearSpaceList.Visibility = Visibility.Collapsed;
                        uiNearSpaceMessage.Text = AppResources.BadGpsMessage;
                        uiNearSpaceMessage.Visibility = Visibility.Visible;
                        NearSpaceViewModel.IsDataLoading = false;  // Mutex
                    }
                }
                else  // GPS is off
                {
                    // Show GPS off message box.
                    uiNearSpaceList.Visibility = Visibility.Collapsed;
                    uiNearSpaceMessage.Text = AppResources.NoGpsOnMessage;
                    uiNearSpaceMessage.Visibility = Visibility.Visible;
                    NearSpaceViewModel.IsDataLoading = false;  // Mutex
                }
            }
            else  // First or not consented of access in location information.
            {
                // Show no consent message box.
                uiNearSpaceList.Visibility = Visibility.Collapsed;
                uiNearSpaceMessage.Text = AppResources.NoLocationAcessConsentMessage;
                uiNearSpaceMessage.Visibility = Visibility.Visible;
                NearSpaceViewModel.IsDataLoading = false;  // Mutex
            }


            // Hide progress indicator
            base.SetProgressIndicator(false);
        }


        // Do selection changed event job.
        private async Task LikeAsync(SpaceViewItem spaceViewItem)
        {
            // Get information about image
            string spaceId = spaceViewItem.SpaceId;
            string spaceLikeButtonImageUri = spaceViewItem.SpaceLikeButtonImage.ToString();


            // Set Image and number first for good user experience.
            // Like or Note LIke by current state
            if (spaceLikeButtonImageUri.Equals(SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH))  // Do Like
            {
                spaceViewItem.SpaceLikeNumber++;
                spaceViewItem.SpaceLikeNumberColor = ColorHexStringToBrushConverter.LIKE_COLOR;
                spaceViewItem.SpaceLikeButtonImage = SpaceViewModel.LIKE_PRESS_IMAGE_PATH;

                // If like fail, set image and number back to original
                if (!(await App.AccountSpaceRelationManager.LikeAysnc(spaceId, true)))
                {
                    spaceViewItem.SpaceLikeNumber--;
                    spaceViewItem.SpaceLikeNumberColor = ColorHexStringToBrushConverter.LIKE_NOT_COLOR;
                    spaceViewItem.SpaceLikeButtonImage = SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH;
                }
            }

            else  // Do Not Like
            {
                spaceViewItem.SpaceLikeNumber--;
                spaceViewItem.SpaceLikeNumberColor = ColorHexStringToBrushConverter.LIKE_NOT_COLOR;
                spaceViewItem.SpaceLikeButtonImage = SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH;

                // If not like fail, set image back to original
                if (!(await App.AccountSpaceRelationManager.LikeAysnc(spaceId, false)))
                {
                    spaceViewItem.SpaceLikeNumber++;
                    spaceViewItem.SpaceLikeNumberColor = ColorHexStringToBrushConverter.LIKE_COLOR;
                    spaceViewItem.SpaceLikeButtonImage = SpaceViewModel.LIKE_PRESS_IMAGE_PATH;
                }
            }
        }


        // Get file tree from cloud
        private async Task SetTreeForFolder(FileObject folder, string message)
        {
            // Show progress indicator 
            uiPinFileList.Visibility = Visibility.Collapsed;
            uiPinFileMessage.Text = message;
            uiPinFileMessage.Visibility = Visibility.Visible;
            base.SetProgressIndicator(true);

            // Before go load, set mutex to true.
            this.FileObjectIsDataLoading = true;

            // TODO If there are not any files?
            if (!this.FolderTree.Contains(folder))
                this.FolderTree.Push(folder);
            List<FileObject> files = await App.SkyDriveManager.GetFilesFromFolderAsync(folder.Id);
            uiCurrentPath.Text = this.GetCurrentPath();

            // Set file tree to list.
            uiPinFileList.DataContext = new ObservableCollection<FileObject>(files);
            uiPinFileList.Visibility = Visibility.Visible;
            uiPinFileMessage.Visibility = Visibility.Collapsed;

            // Hide progress indicator
            base.SetProgressIndicator(false);
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
                await this.SetTreeForFolder(this.FolderTree.First(), AppResources.Loading);
            }
        }
    }
}
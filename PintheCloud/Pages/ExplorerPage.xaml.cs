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

        private const string CONFIRM_APP_BAR_BUTTON_ICON_URI = "/Assets/AppBar/check.png";

        public const int PICK_PIVOT = 0;
        public const int PIN_PIVOT = 1;
        private const int CONFIRM_APP_BAR_BUTTON = 1;


        // Instances
        private SpaceViewModel NearSpaceViewModel = new SpaceViewModel();
        private bool FileObjectIsDataLoading = false;
        private Stack<FileObject> FolderTree = new Stack<FileObject>();
        private List<FileObject> SelectedFile = new List<FileObject>();

        private bool IsLikeButtonClicked = false;


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
                        await this.SetTreeForFolder(null, AppResources.Refreshing);
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
                        this.SetListUnableAndShowMessage(uiNearSpaceList, AppResources.InternetUnavailableMessage, uiNearSpaceMessage);
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
                        {
                            this.FolderTree.Clear();
                            this.SelectedFile.Clear();
                            await this.SetTreeForFolder(null, AppResources.Refreshing);
                        }
                    }
                    else
                    {
                        this.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                    }
                    break;
            }
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
                    this.SelectedFile.Clear();
                    await this.SetTreeForFolder(null, AppResources.Refreshing);
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






        /*** Pin Pivot ***/

        // Go up to previous folder.
        private void uiTreeupBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.TreeUp();
        }


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

        /*
        private void uiPinList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 1)
            {
                FileObject file = (FileObject)e.AddedItems[0];
                if (file.Type.Equals("folder"))
                {
                    this.GetTreeForFolder(file, AppResources.Loading);
                    MyDebug.WriteLine(file.Name);
                }
                else  // Do selection if file
                {
                    this.selectedFile.Add(file);
                    MyDebug.WriteLine(file.Name);
                }
            }
        }
        */

        private void uiAppBarConfirmButton_Click(object sender, System.EventArgs e)
        {
            PhoneApplicationService.Current.State["SELECTED_FILE"] = this.SelectedFile;
            NavigationService.Navigate(new Uri(PtcPage.FILE_LIST_PAGE, UriKind.Relative));
        }



        /*** Self Method ***/

        private async Task SetExplorerPivotAsync(string message)
        {
            // Set worker and show loading message
            App.SpaceManager.SetAccountWorker(new SpaceInternetAvailableWorker());
            App.AccountSpaceRelationManager.SetAccountSpaceRelationWorker(new AccountSpaceRelationInternetAvailableWorker());

            // Show progress indicator 
            this.SetListUnableAndShowMessage(uiNearSpaceList, message, uiNearSpaceMessage);
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
            if (spaceLikeButtonImageUri.Equals(SpaceViewItem.LIKE_NOT_PRESS_IMAGE_PATH))  // Do Like
            {
                spaceViewItem.SetLikeButtonImage(true, 1);

                // If like fail, set image and number back to original
                if (!(await App.AccountSpaceRelationManager.LikeAysnc(spaceId, true)))
                    spaceViewItem.SetLikeButtonImage(false, 1);
            }

            else  // Do Not Like
            {
                spaceViewItem.SetLikeButtonImage(false, 1);

                // If not like fail, set image back to original
                if (!(await App.AccountSpaceRelationManager.LikeAysnc(spaceId, false)))
                    spaceViewItem.SetLikeButtonImage(true, 1);
            }
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
                    await this.SetTreeForFolder(fileObject, AppResources.Loading);
                    MyDebug.WriteLine(fileObject.Name);
                }
                else  // Do selection if file
                {
                    if (fileObject.SelectCheckImage.Equals(FileObject.CHECK_NOT_IMAGE_PATH))
                    {
                        fileObject.SetSelectCheckImage(true);
                        this.SelectedFile.Add(fileObject);
                    }

                    else
                    {
                        fileObject.SetSelectCheckImage(false);
                        this.SelectedFile.Remove(fileObject);
                    }
                    MyDebug.WriteLine(fileObject.Name);
                }
            }
        }


        


        // Get file tree from cloud
        private async Task SetTreeForFolder(FileObject folder, string message)
        {
            // Show progress indicator 
            this.SetListUnableAndShowMessage(uiPinInfoList, message, uiPinInfoMessage);
            base.SetProgressIndicator(true);

            // Before go load, set mutex to true.
            this.FileObjectIsDataLoading = true;

            // If folder null, set root.
            if (folder == null)
                folder = await App.SkyDriveManager.GetRootFolderAsync();

            // TODO If there are not any files?
            if (!this.FolderTree.Contains(folder))
                this.FolderTree.Push(folder);
            List<FileObject> files = await App.SkyDriveManager.GetFilesFromFolderAsync(folder.Id);
            uiCurrentPath.Text = this.GetCurrentPath();

            // Set file tree to list.
            uiPinInfoList.DataContext = new ObservableCollection<FileObject>(files);
            uiPinInfoList.Visibility = Visibility.Visible;
            uiPinInfoMessage.Visibility = Visibility.Collapsed;

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



        /*** Private Method ***/

        private void SetListUnableAndShowMessage(LongListSelector list, string message, TextBlock messageTextBlock)
        {
            list.Visibility = Visibility.Collapsed;
            messageTextBlock.Text = message;
            messageTextBlock.Visibility = Visibility.Visible;
        }
    }
}
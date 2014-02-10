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

namespace PintheCloud.Pages
{
    public partial class ExplorerPage : PtcPage
    {
        // Instances
        private const int PICK_PIVOT = 0;
        private const int PIN_PIVOT = 1;

        private bool IsLikeButtonClicked = false;

        public SpaceViewModel NearSpaceViewModel = new SpaceViewModel();
        public FileObjectViewModel FileObjectViewModel = new FileObjectViewModel();


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
                    // If Internet available, Set space list
                    if (NetworkInterface.GetIsNetworkAvailable())
                        if (!NearSpaceViewModel.IsDataLoading)  // Mutex check
                            await this.SetExplorerPivotAsync(AppResources.Loading);
                    break;

                case PIN_PIVOT:
                    // TODO
                    break;
            }
        }


        // List select event
        private async void uiNearSpaceList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected Space View Item
            // Do selection changed event job.
            // Set selected item to null for next selection of list item. 
            SpaceViewItem spaceViewItem = uiNearSpaceList.SelectedItem as SpaceViewItem;
            await this.uiSpaceList_SelectionChanged(spaceViewItem);
            uiNearSpaceList.SelectedItem = null;
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
            // Refresh only if pivot is in pin pivot.
            switch (uiExplorerPivot.SelectedIndex)
            {
                case PIN_PIVOT:
                    // Set View model for dispaly,
                    NearSpaceViewModel.IsDataLoading = false;  // Mutex
                    await this.SetExplorerPivotAsync(AppResources.Refresh);
                    break;
            }
        }



        /*** Self Method ***/

        private async Task SetExplorerPivotAsync(string message)
        {
            // Get different Space Worker by internet state.
            if (NetworkInterface.GetIsNetworkAvailable()) //  Internet available.
            {
                // Set worker and show loading message
                App.SpaceManager.SetAccountWorker(new SpaceInternetAvailableWorker());
                App.SpaceManager.SetAccountSpaceRelationWorker(new AccountSpaceRelationInternetAvailableWorker());

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
                            ObservableCollection<SpaceViewItem> items =
                                await App.SpaceManager.GetNearSpaceViewItemsAsync(currentGeoposition);

                            if (items != null)  // There are near spaces
                            {
                                this.NearSpaceViewModel.Items = items;
                                uiNearSpaceList.DataContext = null;
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
        }


        // Do selection changed event job.
        private async Task uiSpaceList_SelectionChanged(SpaceViewItem spaceViewItem)
        {
            // If selected item isn't null and it doesn't come from like button, goto File list page.
            // Otherwise, Process Like or Not Like by current state
            if (spaceViewItem != null && !this.IsLikeButtonClicked)  // Go to FIle List Page
            {
                string parameters = App.SpaceManager.GetParameterStringFromSpaceViewItem(spaceViewItem);
                NavigationService.Navigate(new Uri(PtcPage.FILE_LIST_PAGE + parameters, UriKind.Relative));
            }

            else if (this.IsLikeButtonClicked)  // Do like
            {
                // Get different Account Space Relation Worker by internet state.
                if (NetworkInterface.GetIsNetworkAvailable()) //  Internet available.
                {
                    App.AccountSpaceRelationManager.SetAccountSpaceRelationWorker(new AccountSpaceRelationInternetAvailableWorker());


                    // Get information about image
                    string spaceId = spaceViewItem.SpaceId;
                    string spaceLikeButtonImageUri = spaceViewItem.SpaceLikeButtonImage.ToString();


                    // Set Image first for good user experience.
                    // Like or Note LIke by current state
                    if (spaceLikeButtonImageUri.Equals(SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH))  // Do Like
                    {
                        spaceViewItem.SpaceLikeButtonImage = new Uri(SpaceViewModel.LIKE_PRESS_IMAGE_PATH, UriKind.Relative);

                        // If like success, set like number ++
                        // If like fail, set image back to original
                        if (await App.AccountSpaceRelationManager.LikeAysnc(spaceId, true))
                        {
                            // TODO ++ like number
                        }
                        else
                        {
                            spaceViewItem.SpaceLikeButtonImage = new Uri(SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH, UriKind.Relative);
                        }
                    }

                    else  // Do Not Like
                    {
                        spaceViewItem.SpaceLikeButtonImage = new Uri(SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH, UriKind.Relative);

                        // If not like success, set like number --
                        // If not like fail, set image back to original
                        if (await App.AccountSpaceRelationManager.LikeAysnc(spaceId, false))
                        {
                            // TODO -- like number
                        }
                        else
                        {
                            spaceViewItem.SpaceLikeButtonImage = new Uri(SpaceViewModel.LIKE_PRESS_IMAGE_PATH, UriKind.Relative);
                        }
                    }

                }
                else  // Internet bad.
                {
                    MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
                }

                // Set like button clicked false for next like button click.
                this.IsLikeButtonClicked = false;
            }
        }
    }
}
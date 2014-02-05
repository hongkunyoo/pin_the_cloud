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
        private const int EXPLORER_PIVOT = 0;
        private const int MY_SPACES_PIVOT = 1;

        private const string EDIT_IMAGE_PATH = "/Assets/pajeon/png/general_edit.png";
        private const string VIEW_IMAGE_PATH = "/Assets/pajeon/png/general_view.png";

        public SpaceViewModel NearSpaceViewModel = new SpaceViewModel();
        public SpaceViewModel MySpaceViewModel = new SpaceViewModel();


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
                case EXPLORER_PIVOT:
                    uiSpaceEditButton.Visibility = Visibility.Collapsed;

                    // If Internet available, Set space list
                    if (NetworkInterface.GetIsNetworkAvailable())
                        if (!NearSpaceViewModel.IsDataLoading)  // Mutex check
                            await this.SetExplorerPivotAsync(AppResources.Loading);
                    break;

                case MY_SPACES_PIVOT:
                    uiSpaceEditButton.Visibility = Visibility.Visible;

                    // If Internet available, Set space list
                    if (NetworkInterface.GetIsNetworkAvailable())
                        if (!MySpaceViewModel.IsDataLoading)  // Mutex check
                            await this.SetMySpacePivotAsync(AppResources.Loading);
                    break;
            }
        }


        // Process of editing spaces, especially deleting or not.
        private void uiSpaceEditButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Get information about image
            Button editButton = (Button)sender;
            Image editButtonImage = (Image)editButton.Content;
            BitmapImage editButtonImageBitmap = (BitmapImage)editButtonImage.Source;
            string editButtonImageUriSource = editButtonImageBitmap.UriSource.ToString();

            // If it is not view mode, go edit
            // Otherwise, go view mode
            if (editButtonImageUriSource.Equals(EDIT_IMAGE_PATH))  // It is View mode
            {
                editButtonImage.Source = new BitmapImage(new Uri(VIEW_IMAGE_PATH, UriKind.Relative));

                // TODO Chande mode to edit mode
            }
            else  // It is Edit mode
            {
                editButtonImage.Source = new BitmapImage(new Uri(EDIT_IMAGE_PATH, UriKind.Relative));

                // TODO Chande mode to view mode
            }
        }


        // Process Like or Not Like by current state
        private async void uiSpaceLikeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Get different Account Space Relation Worker by internet state.
            if (NetworkInterface.GetIsNetworkAvailable()) //  Internet available.
            {
                App.AccountSpaceRelationManager.SetAccountSpaceRelationWorker(new AccountSpaceRelationInternetAvailableWorker());


                // Get information about image
                Button likeButton = (Button)sender;
                Image likeButtonImage = (Image)likeButton.Content;
                string spaceId = likeButtonImage.Tag.ToString();

                BitmapImage likeButtonImageBitmap = (BitmapImage)likeButtonImage.Source;
                string likeButtonImageUriSource = likeButtonImageBitmap.UriSource.ToString();


                // Set Image first for good user experience.
                // Like or Note LIke by current state
                if (likeButtonImageUriSource.Equals(SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH))  // Do Like
                {
                    likeButtonImage.Source = new BitmapImage(new Uri(SpaceViewModel.LIKE_PRESS_IMAGE_PATH, UriKind.Relative));

                    // If like success, set like number ++
                    // If like fail, set image back to original
                    if (await App.AccountSpaceRelationManager.LikeAysnc(spaceId, true))
                    {
                        // TODO ++ 
                    }
                    else
                    {
                        likeButtonImage.Source = new BitmapImage(new Uri(SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH, UriKind.Relative));
                    }
                }

                else  // Do Not Like
                {
                    likeButtonImage.Source = new BitmapImage(new Uri(SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH, UriKind.Relative));

                    // If not like success, set like number --
                    // If not like fail, set image back to original
                    if (await App.AccountSpaceRelationManager.LikeAysnc(spaceId, false))
                    {
                        // TODO --
                    }
                    else
                    {
                        likeButtonImage.Source = new BitmapImage(new Uri(SpaceViewModel.LIKE_PRESS_IMAGE_PATH, UriKind.Relative));
                    }
                }
            }
            else  // // Internet bad.
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        // Move to Setting Page
        private void uiAppBarSettingsButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri(PtcPage.SETTINGS_PAGE, UriKind.Relative));
        }


        // Move to Make Space Page
        private void uiAppBarMakeSpaceButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Utilities/TestDrive.xaml", UriKind.Relative));
        }


        // Move to Map Page
        private void uiAppBarMapButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri(PtcPage.MAP_VIEW_PAGE, UriKind.Relative));
        }


        // Refresh space list.
        private async void uiAppBarRefreshButton_Click(object sender, System.EventArgs e)
        {
            // Set View model for dispaly,
            switch (uiExplorerPivot.SelectedIndex)
            {
                case EXPLORER_PIVOT:
                    NearSpaceViewModel.IsDataLoading = false;  // Mutex
                    await this.SetExplorerPivotAsync(AppResources.Refresh);
                    break;

                case MY_SPACES_PIVOT:
                    MySpaceViewModel.IsDataLoading = false;  // Mutex
                    await this.SetMySpacePivotAsync(AppResources.Refresh);
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

                // If it works bad but no error resluts, repeat.
                if (uiNearSpaceMessage.Visibility == Visibility.Visible && uiNearSpaceMessage.Text.Equals(AppResources.Loading))
                    await this.SetExplorerPivotAsync(message);
            }
        }


        private async Task SetMySpacePivotAsync(string message)
        {
            // Get different Space Worker by internet state.
            if (NetworkInterface.GetIsNetworkAvailable()) //  Internet available.
            {
                // Set worker and show loading message
                App.SpaceManager.SetAccountWorker(new SpaceInternetAvailableWorker());
                App.SpaceManager.SetAccountSpaceRelationWorker(new AccountSpaceRelationInternetAvailableWorker());

                // Show progress indicator 
                uiMySpaceList.Visibility = Visibility.Collapsed;
                uiMySpaceMessage.Text = message;
                uiMySpaceMessage.Visibility = Visibility.Visible;
                base.SetProgressIndicator(true);

                // Before go load, set mutex to true.
                MySpaceViewModel.IsDataLoading = true;


                // If there is my spaces, Clear and Add spaces to list
                // Otherwise, Show none message.
                ObservableCollection<SpaceViewItem> items =
                                await App.SpaceManager.GetMySpaceViewItemsAsync();

                if (items != null)  // There are my spaces
                {
                    this.MySpaceViewModel.Items = items;
                    uiMySpaceList.DataContext = null;
                    uiMySpaceList.DataContext = this.MySpaceViewModel;
                    uiMySpaceList.Visibility = Visibility.Visible;
                    uiMySpaceMessage.Visibility = Visibility.Collapsed;
                }
                else  // No my spaces
                {
                    uiMySpaceList.Visibility = Visibility.Collapsed;
                    uiMySpaceMessage.Text = AppResources.NoMySpaceMessage;
                    uiMySpaceMessage.Visibility = Visibility.Visible;
                    MySpaceViewModel.IsDataLoading = false;  // Mutex
                }


                // Hide progress indicator
                base.SetProgressIndicator(false);

                // If it works bad but no error resluts, repeat.
                if (uiMySpaceMessage.Visibility == Visibility.Visible && uiMySpaceMessage.Text.Equals(AppResources.Loading))
                    await this.SetMySpacePivotAsync(message);
            }
        }
    }
}
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

namespace PintheCloud.Pages
{
    public partial class ExplorerPage : PtcPage
    {
        // Instances
        private const int EXPLORER_PIVOT = 0;
        private const int RECENT_PIVOT = 1;
        private const int MY_SPACES_PIVOT = 2;

        public static SpaceViewModel NearSpaceViewModel = new SpaceViewModel();
        public static SpaceViewModel MySpaceViewModel = new SpaceViewModel();


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
                    if (!NearSpaceViewModel.IsDataLoaded)
                        await this.SetExplorerPivotAsync(AppResources.Loading);
                    break;

                case RECENT_PIVOT:
                    // TODO
                    break;

                case MY_SPACES_PIVOT:
                    if (!MySpaceViewModel.IsDataLoaded)
                        await this.SetMySpacePivotAsync(AppResources.Loading);
                    break;
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
            NavigationService.Navigate(new Uri(PtcPage.SKY_DRIVE_PICKER_PAGE, UriKind.Relative));
        }


        // Move to Map Page
        private void uiAppBarMapButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri(PtcPage.MAP_VIEW_PAGE, UriKind.Relative));
        }


        private void uiNearSpaceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        // Refresh space list.
        private async void uiAppBarRefreshButton_Click(object sender, System.EventArgs e)
        {
            // Set View model for dispaly,
            switch (uiExplorerPivot.SelectedIndex)
            { 
                case EXPLORER_PIVOT:
                    NearSpaceViewModel.IsDataLoaded = false;
                    await this.SetExplorerPivotAsync(AppResources.Refresh);
                    break;

                case RECENT_PIVOT:
                    // TODO
                    break;

                case MY_SPACES_PIVOT:
                    MySpaceViewModel.IsDataLoaded = false;
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
                App.CurrentSpaceManager.SetAccountWorker(new SpaceInternetAvailableWorker());
                uiNearSpaceList.Visibility = Visibility.Collapsed;
                uiNearSpaceMessage.Text = message;
                uiNearSpaceMessage.Visibility = Visibility.Visible;
                base.SetProgressIndicator(true);

                // Check whether user consented for location access.
                if (base.GetLocationAccessConsent())  // Got consent of location access.
                {
                    // Check whether GPS is on or not
                    if (base.GetGeolocatorPositionStatus())  // GPS is on
                    {
                        Geoposition currentGeoposition = await App.CurrentGeoCalculateManager.GetCurrentGeopositionAsync();

                        // Check whether GPS works well or not
                        if (currentGeoposition != null)  // works well
                        {
                            // If there is near spaces, Clear and Add spaces to list
                            // Otherwise, Show none message.
                            if (await App.CurrentSpaceManager.SetNearSpaceViewItemsToSpaceViewModelAsync(currentGeoposition))  // There are near spaces
                            {
                                uiNearSpaceList.Visibility = Visibility.Visible;
                                uiNearSpaceMessage.Visibility = Visibility.Collapsed;
                                NearSpaceViewModel.IsDataLoaded = true;
                                uiNearSpaceList.DataContext = NearSpaceViewModel;
                            }
                            else  // No near spaces
                            {
                                uiNearSpaceList.Visibility = Visibility.Collapsed;
                                uiNearSpaceMessage.Text = AppResources.NoNearSpaceMessage;
                                uiNearSpaceMessage.Visibility = Visibility.Visible;
                            }
                        }
                        else  // works bad
                        {
                            // Show GPS off message box.
                            uiNearSpaceList.Visibility = Visibility.Collapsed;
                            uiNearSpaceMessage.Text = AppResources.BadGpsMessage;
                            uiNearSpaceMessage.Visibility = Visibility.Visible;
                        }
                    }
                    else  // GPS is off
                    {
                        // Show GPS off message box.
                        uiNearSpaceList.Visibility = Visibility.Collapsed;
                        uiNearSpaceMessage.Text = AppResources.NoGpsOnMessage;
                        uiNearSpaceMessage.Visibility = Visibility.Visible;
                    }
                }
                else  // First or not consented of access in location information.
                {
                    // Show no consent message box.
                    uiNearSpaceList.Visibility = Visibility.Collapsed;
                    uiNearSpaceMessage.Text = AppResources.NoLocationAcessConsentMessage;
                    uiNearSpaceMessage.Visibility = Visibility.Visible;
                }

                // Hide progress indicator
                base.SetProgressIndicator(false);
            }

            else  // Internet bad.
            {
                // Show bad Internet message box.
                uiNearSpaceList.Visibility = Visibility.Collapsed;
                uiNearSpaceMessage.Text = AppResources.InternetUnavailableMessage;
                uiNearSpaceMessage.Visibility = Visibility.Visible;
            }
        }


        private async Task SetMySpacePivotAsync(string message)
        {
            // Get different Space Worker by internet state.
            if (NetworkInterface.GetIsNetworkAvailable()) //  Internet available.
            {
                // Set worker and show loading message
                App.CurrentSpaceManager.SetAccountWorker(new SpaceInternetAvailableWorker());
                uiMySpaceList.Visibility = Visibility.Collapsed;
                uiMySpaceMessage.Text = message;
                uiMySpaceMessage.Visibility = Visibility.Visible;
                base.SetProgressIndicator(true);

                // If there is my spaces, Clear and Add spaces to list
                // Otherwise, Show none message.
                if (await App.CurrentSpaceManager.SetMySpaceViewItemsToSpaceViewModelAsync())  // There are my spaces
                {
                    uiMySpaceList.Visibility = Visibility.Visible;
                    uiMySpaceMessage.Visibility = Visibility.Collapsed;
                    MySpaceViewModel.IsDataLoaded = true;
                    uiMySpaceList.DataContext = MySpaceViewModel;
                }
                else  // No my spaces
                {
                    uiMySpaceList.Visibility = Visibility.Collapsed;
                    uiMySpaceMessage.Text = AppResources.NoMySpaceMessage;
                    uiMySpaceMessage.Visibility = Visibility.Visible;
                }

                // Hide progress indicator
                base.SetProgressIndicator(false);
            }

            else  // Internet bad.
            {
                // Show bad Internet message box.
                uiMySpaceList.Visibility = Visibility.Collapsed;
                uiMySpaceMessage.Text = AppResources.InternetUnavailableMessage;
                uiMySpaceMessage.Visibility = Visibility.Visible;
            }
        }
    }
}
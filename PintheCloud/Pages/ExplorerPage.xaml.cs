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

        public SpaceViewModel CurrentSpaceViewModel = new SpaceViewModel();


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
            switch (uiExplorerPivot.SelectedIndex)
            { 
                case EXPLORER_PIVOT:
                    
                    // Get different Space Worker by internet state.
                    if (NetworkInterface.GetIsNetworkAvailable()) //  Internet available.
                    {
                        App.CurrentSpaceManager.SetAccountWorker(new SpaceInternetAvailableWorker());

                        // TODO If there is spaces, Clear and Add spaces to list
                        // TODO Otherwise, Show none message.
                        // TODO load near space use GPS information

                        // Check whether user consented for location access.
                        base.SetProgressIndicator(true, AppResources.Loading);
                        if (base.GetLocationAccessConsent())  // Got consent of location access.
                        {
                            // Check whether GPS is on or not
                            if (base.GetGeolocatorPositionStatus())  // GPS is on
                            {
                                base.SetProgressIndicator(true, AppResources.Loading);
                                Geoposition currentGeoposition = await App.CurrentGeoCalculateManager.GetCurrentGeopositionAsync();

                                // Check whether GPS works well or not
                                if (currentGeoposition != null)  // works well
                                {
                                    // If there is near spaces, Clear and Add spaces to list
                                    // Otherwise, Show none message.
                                    ObservableCollection<SpaceViewItem> items = await App.CurrentSpaceManager.GetNearSpaceViewItemsAsync(currentGeoposition);
                                    if (items != null)  // There are near spaces
                                    {
                                        uiNearSpaceList.Visibility = Visibility.Visible;
                                        uiNearSpaceMessage.Visibility = Visibility.Collapsed;
                                        CurrentSpaceViewModel.Items = items;
                                        this.DataContext = CurrentSpaceViewModel;
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
                        base.SetProgressIndicator(false);
                    }
                    else  // Internet bad.
                    {
                        // Show bad Internet message box.
                        uiNearSpaceList.Visibility = Visibility.Collapsed;
                        uiNearSpaceMessage.Text = AppResources.InternetUnavailableMessage;
                        uiNearSpaceMessage.Visibility = Visibility.Visible;
                    }
                    break;


                case RECENT_PIVOT:
                    // TODO
                    break;


                case MY_SPACES_PIVOT:

                    // Get different Space Worker by internet state.
                    if (NetworkInterface.GetIsNetworkAvailable()) //  Internet available.
                    {
                        App.CurrentSpaceManager.SetAccountWorker(new SpaceInternetAvailableWorker());

                        // If there is my spaces, Clear and Add spaces to list
                        // Otherwise, Show none message.
                        base.SetProgressIndicator(true, AppResources.Loading);
                        ObservableCollection<SpaceViewItem> items = await App.CurrentSpaceManager.GetMySpaceViewItemsAsync();
                        if (items != null)
                        {
                            uiMySpaceList.Visibility = Visibility.Visible;
                            uiMySpaceMessage.Visibility = Visibility.Collapsed;
                            CurrentSpaceViewModel.Items = items;
                            this.DataContext = CurrentSpaceViewModel;
                        }
                        else
                        {
                            uiMySpaceList.Visibility = Visibility.Collapsed;
                            uiMySpaceMessage.Text = AppResources.NoMySpaceMessage;
                            uiMySpaceMessage.Visibility = Visibility.Visible;
                        }
                        base.SetProgressIndicator(false);
                    }
                    else  // Internet bad.
                    {
                        // Show bad Internet message box.
                        uiMySpaceList.Visibility = Visibility.Collapsed;
                        uiMySpaceMessage.Text = AppResources.InternetUnavailableMessage;
                        uiMySpaceMessage.Visibility = Visibility.Visible;
                    }
                    break;
            }
        }


        private void uiRefreshButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO Load new near spaces.
        }


        // Move to Setting Page
        private void uiAppBarSettingsButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri(PtcPage.SETTINGS_PAGE, UriKind.Relative));
        }


        // Move to Make Space Page
        private void uiAppBarMakeSpaceButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri(PtcPage.SKYDRIVE_PICKER_PAGE, UriKind.Relative));
        }


        // Move to Map Page
        private void uiAppBarMapButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri(PtcPage.MAP_VIEW_PAGE, UriKind.Relative));
        }
    }
}
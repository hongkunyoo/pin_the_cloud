using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using PintheCloud.ViewModels;
using PintheCloud.Resources;
using System.Net.NetworkInformation;
using Windows.Devices.Geolocation;
using PintheCloud.Models;
using PintheCloud.Managers;
using PintheCloud.Popups;
using System.Windows.Controls.Primitives;
using PintheCloud.Helpers;

namespace PintheCloud.Pages
{
    public partial class SpotList : PtcPage
    {
        private Popup SubmitSpotPasswordParentPopup = new Popup();
        public SpotViewModel NearSpotViewModel = new SpotViewModel();


        public SpotList()
        {
            InitializeComponent();
            uiNearSpotList.DataContext = this.NearSpotViewModel;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Check if it is on the backstack from SplashPage and remove that.
            for (int i = 0; i < NavigationService.BackStack.Count(); i++)
                NavigationService.RemoveBackEntry();
            this.SetNearSpotList(AppResources.Loading);
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        // List select event
        private void uiNearSpotList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected Spot View Item
            SpotViewItem spotViewItem = uiNearSpotList.SelectedItem as SpotViewItem;

            // Set selected item to null for next selection of list item. 
            uiNearSpotList.SelectedItem = null;

            // If it is private mode, get password from user and check it.
            // Otherwise, goto File list page.
            if (spotViewItem != null)  // Go to FIle List Page
            {
                if (spotViewItem.IsPrivateImage.Equals(FileObjectViewModel.IS_PRIVATE_IMAGE_URI))
                {
                    this.SubmitSpotPasswordParentPopup = new Popup();
                    SubmitSpotPasswordPopup submitSpotPasswordPopup =
                        new SubmitSpotPasswordPopup(this.SubmitSpotPasswordParentPopup, spotViewItem.SpotId, spotViewItem.SpotPassword,
                            uiContentPanel.ActualWidth, uiContentPanel.ActualHeight, uiPivotTitleGrid.ActualHeight);
                    this.SubmitSpotPasswordParentPopup.Child = submitSpotPasswordPopup;
                    this.SubmitSpotPasswordParentPopup.IsOpen = true;
                    this.SubmitSpotPasswordParentPopup.Closed += (senderObject, args) =>
                    {
                        if (((SubmitSpotPasswordPopup)((Popup)senderObject).Child).result)
                        {
                            string parameters = base.GetParameterStringFromSpotViewItem(spotViewItem);
                            NavigationService.Navigate(new Uri(EventHelper.EXPLORER_PAGE + parameters, UriKind.Relative));
                        }
                    };
                }
                else
                {
                    string parameters = base.GetParameterStringFromSpotViewItem(spotViewItem);
                    NavigationService.Navigate(new Uri(EventHelper.EXPLORER_PAGE + parameters, UriKind.Relative));
                }
            }
        }


        private void SetNearSpotList(string message)
        {
            // If Internet available, Set spot list
            // Otherwise, show internet bad message
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                if (!this.NearSpotViewModel.IsDataLoaded)  // Mutex check
                    this.SetNearSpotListAsync(message);
            }
            else
            {
                base.SetListUnableAndShowMessage(uiNearSpotList, uiNearSpotMessage, AppResources.InternetUnavailableMessage);
            }
        }



        private async void SetNearSpotListAsync(string message)
        {
            // Show progress indicator
            base.SetListUnableAndShowMessage(uiNearSpotList, uiNearSpotMessage, message);
            base.SetProgressIndicator(true);

            // Check whether user consented for location access.
            if (base.GetLocationAccessConsent())  // Got consent of location access.
            {
                // Check whether GPS is on or not
                if (GeoHelper.GetLocationStatus() != PositionStatus.Disabled)  // GPS is on
                {
                    // Check whether GPS works well or not
                    Geoposition currentGeoposition = await GeoHelper.GetGeopositionAsync();
                    if (currentGeoposition != null)  // GPS works well
                    {
                        try
                        {
                            // If there is near spots, Clear and Add spots to list
                            // Otherwise, Show none message.
                            List<SpotObject> spots = await App.SpotManager.GetNearSpotListAsync(currentGeoposition);
                            this.NearSpotViewModel.IsDataLoaded = true;
                            if (spots.Count > 0)  // There are near spots
                            {
                                base.Dispatcher.BeginInvoke(() =>
                                {
                                    uiNearSpotList.Visibility = Visibility.Visible;
                                    uiNearSpotMessage.Visibility = Visibility.Collapsed;
                                    this.NearSpotViewModel.SetItems(spots);
                                });
                            }
                            else  // No near spots
                            {
                                base.SetListUnableAndShowMessage(uiNearSpotList, uiNearSpotMessage, AppResources.NoNearSpotMessage);
                            }
                        }
                        catch
                        {
                            base.SetListUnableAndShowMessage(uiNearSpotList, uiNearSpotMessage, AppResources.BadLoadingSpotMessage);
                        }
                    }
                    else  // GPS works bad
                    {
                        base.SetListUnableAndShowMessage(uiNearSpotList, uiNearSpotMessage, AppResources.BadLocationServiceMessage);
                    }
                }
                else  // GPS is off
                {
                    base.SetListUnableAndShowMessage(uiNearSpotList, uiNearSpotMessage, AppResources.NoLocationServiceMessage);
                }
            }
            else  // First or not consented of access in location information.
            {
                base.SetListUnableAndShowMessage(uiNearSpotList, uiNearSpotMessage, AppResources.NoLocationAcessConsentMessage);
            }

            // Hide progress indicator
            base.SetProgressIndicator(false);
        }


        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            // If it is signing, don't close app.
            // Otherwise, show close app message.
            IStorageManager iStorageManager = Switcher.GetCurrentStorage();
            if (this.SubmitSpotPasswordParentPopup.IsOpen)
            {
                e.Cancel = true;
                this.SubmitSpotPasswordParentPopup.IsOpen = false;
            }
            else
            {
                MessageBoxResult result = MessageBox.Show(AppResources.CloseAppMessage, AppResources.CloseAppCaption, MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK)
                    e.Cancel = true;
            }
        }


        private void uiAppBarSettingsButton_Click(object sender, System.EventArgs e)
        {
            PhoneApplicationService.Current.State[SPOT_VIEW_MODEL_KEY] = this.NearSpotViewModel;
            EventHelper.TriggerEvent(EventHelper.POPUP_CLOSE);
            NavigationService.Navigate(new Uri(EventHelper.SETTINGS_PAGE, UriKind.Relative));
        }


        private void uiAppBarRefreshButton_Click(object sender, System.EventArgs e)
        {
            this.NearSpotViewModel.IsDataLoaded = false;
            this.SetNearSpotList(AppResources.Refreshing);
        }


        private void uiAppBarNewSpotButton_Click(object sender, System.EventArgs e)
        {
            // Check whether user consented for location access.
            if (base.GetLocationAccessConsent())  // Got consent of location access.
            {
                PhoneApplicationService.Current.State[SPOT_VIEW_MODEL_KEY] = this.NearSpotViewModel;
                NavigationService.Navigate(new Uri(EventHelper.NEW_SPOT_PAGE, UriKind.Relative));
            }
            else  // First or not consented of access in location information.
            {
                MessageBox.Show(AppResources.NoLocationAcessConsentMessage, AppResources.NoLocationAcessConsentCaption, MessageBoxButton.OK);
            }
        }
    }
}
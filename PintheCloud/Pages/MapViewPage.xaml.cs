using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Services;
using Microsoft.Phone.Maps.Toolkit;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using System.Windows.Navigation;

namespace PintheCloud.Pages
{
    public partial class MapViewPage : PtcPage
    {
        // Instance
        private const double ZOOM_LEVEL = 16;


        public MapViewPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Move to User Location
            await this.ShowUserLocation();
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        // Move marker in map to User's Location
        private async void uiAppBarMyLocationButton_Click(object sender, EventArgs e)
        {
            await this.ShowUserLocation();
        }

        // Move to Make Space Page
        private void uiAppBarMakeSpaceButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri(PtcPage.SKY_DRIVE_PICKER_PAGE, UriKind.Relative));
        }

        // Move to Setting Page
        private void uiAppBarSettingsButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri(PtcPage.SETTINGS_PAGE, UriKind.Relative));
        }

        // Move map location to user's location and set marker
        private async Task ShowUserLocation()
        {
            Geoposition geoposition = await App.CurrentGeoCalculateManager.GetCurrentGeopositionAsync();
            uiUserLocationMarker.GeoCoordinate = geoposition.Coordinate.ToGeoCoordinate();
            uiMapView.SetView(uiUserLocationMarker.GeoCoordinate, ZOOM_LEVEL);
        }
    }
}
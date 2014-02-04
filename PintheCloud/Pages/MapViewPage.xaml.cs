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

namespace PintheCloud.Pages
{
    public partial class MapViewPage : PtcPage
    {
        private readonly double userLocationMarkerZoomLevel = 16;
        public MapViewPage()
        {
            InitializeComponent();
        }
        // Move to Setting Page
        private void uiAppBarSettingsButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri(PtcPage.SETTINGS_PAGE, UriKind.Relative));
        }

        // Move to Map Page
        private void uiAppBarMapButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri(PtcPage.EXPLORER_PAGE, UriKind.Relative));
        }

        // Move to User Location
        private async void uiMyLocation_Click(object sender, EventArgs e)
        {
            await this.ShowUserLocation();
            this.MapView.SetView(this.UserLocationMarker.GeoCoordinate, this.userLocationMarkerZoomLevel);
        }

        private async Task ShowUserLocation()
        {
            Geolocator geolocator;
            Geoposition geoposition;

            this.UserLocationMarker = (UserLocationMarker)this.FindName("UserLocationMarker");

            geolocator = new Geolocator();

            geoposition = await geolocator.GetGeopositionAsync();

            this.UserLocationMarker.GeoCoordinate = geoposition.Coordinate.ToGeoCoordinate();
            this.UserLocationMarker.Visibility = System.Windows.Visibility.Visible;
        }

    }
}
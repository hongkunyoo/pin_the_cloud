using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using PintheCloud.Models;
using PintheCloud.Resources;
using PintheCloud.Utilities;
using PintheCloud.Workers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Devices.Geolocation;
using Windows.UI;

namespace PintheCloud.ViewModels
{
    public class SpaceViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<SpaceViewItem> Items { get; set; }

        // Mutex
        public bool IsDataLoaded { get; set; }


        public SpaceViewModel()
        {
            this.Items = new ObservableCollection<SpaceViewItem>();
        }


        public void SetItems(JArray spaces, Geoposition currentGeoposition)
        {
            // If items have something, clear.
            this.Items.Clear();

            // Get current coordinate
            double currentLatitude = currentGeoposition.Coordinate.Latitude;
            double currentLongtitude = currentGeoposition.Coordinate.Longitude;

            // Convert jarray spaces to space view items and set to view model
            foreach (JObject jSpace in spaces)
            {
                string space_id = (string)jSpace["id"];
                string space_name = (string)jSpace["space_name"];
                double space_latitude = (double)jSpace["space_latitude"];
                double space_longtitude = (double)jSpace["space_longtitude"];
                string account_id = (string)jSpace["account_id"];
                string account_name = (string)jSpace["account_name"];
                double space_distance = (double)jSpace["space_distance"];

                Space space = new Space(space_name, space_latitude, space_longtitude, account_id, account_name, space_distance);
                space.id = space_id;
                this.Items.Add(this.MakeSpaceViewItemFromSpace(space));
            }
        }
        public void SetItems(MobileServiceCollection<Space, Space> spaces)
        {
            // If items have something, clear.
            this.Items.Clear();

            // Convert spaces to space view items and set to view model
            foreach (Space space in spaces)
                this.Items.Add(this.MakeSpaceViewItemFromSpace(space));
        }



        /*** Self Method ***/

        // Make new space view item from space model object.
        private SpaceViewItem MakeSpaceViewItemFromSpace(Space space)
        {
            // Set new space view item
            SpaceViewItem spaceViewItem = new SpaceViewItem();
            spaceViewItem.SpaceName = space.space_name;
            spaceViewItem.AccountId = space.account_id;
            spaceViewItem.AccountName = space.account_name;
            spaceViewItem.SpaceId = space.id;
            spaceViewItem.SpaceDistance = Math.Round(space.space_distance);

            return spaceViewItem;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

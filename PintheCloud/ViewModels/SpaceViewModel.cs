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
        public bool IsDataLoading { get; set; }


        public SpaceViewModel()
        {
            this.Items = new ObservableCollection<SpaceViewItem>();
        }


        public async void SetItems(JArray spaces, Geoposition currentGeoposition)
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
                int space_like_number = (int)jSpace["space_like_number"];
                double space_distance = (double)jSpace["space_distance"];

                bool isLike = await App.AccountSpaceRelationManager.IsLikeAsync(space_id);
                Space space = new Space(space_name, space_latitude, space_longtitude, account_id, account_name, space_like_number, space_distance);
                space.id = space_id;
                this.Items.Add(this.MakeSpaceViewItemFromSpace(space, isLike));
            }
        }
        public async void SetItems(MobileServiceCollection<Space, Space> spaces)
        {
            // If items have something, clear.
            this.Items.Clear();

            // Convert spaces to space view items and set to view model
            foreach (Space space in spaces)
            {
                // Get whether this account likes this space
                bool isLike = await App.AccountSpaceRelationManager.IsLikeAsync(space.id);
                this.Items.Add(this.MakeSpaceViewItemFromSpace(space, isLike));
            }
        }



        /*** Self Method ***/

        // Make new space view item from space model object.
        private SpaceViewItem MakeSpaceViewItemFromSpace(Space space, bool isLike)
        {
            // Set new space view item
            SpaceViewItem spaceViewItem = new SpaceViewItem();
            spaceViewItem.SpaceName = space.space_name;
            spaceViewItem.AccountId = space.account_id;
            spaceViewItem.AccountName = space.account_name;
            spaceViewItem.SpaceLikeNumber = space.space_like_number;
            spaceViewItem.SpaceId = space.id;
            spaceViewItem.SpaceDistance = Math.Round(space.space_distance);


            // If this account likes this space, set like image
            // Otherwise, set not like image
            if (isLike)
                spaceViewItem.SetLikeButtonImage(true, 0);
            else
                spaceViewItem.SetLikeButtonImage(false, 0);

            // If this space is this account's, set id bold font.
            // Otherwise, set light font.
            if (App.AccountManager.GetCurrentAcccount().account_platform_id.Equals(space.account_id))
                spaceViewItem.AccountIdFontWeight = StringToFontWeightConverter.BOLD;
            else
                spaceViewItem.AccountIdFontWeight = StringToFontWeightConverter.LIGHT;

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

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
using Windows.Devices.Geolocation;
using Windows.UI;

namespace PintheCloud.ViewModels
{
    public class SpaceViewModel : INotifyPropertyChanged
    {
        public static string LIKE_NOT_PRESS_IMAGE_PATH = "/Assets/pajeon/png/general_like.png";
        public static string LIKE_PRESS_IMAGE_PATH = "/Assets/pajeon/png/general_like_p.png";


        public ObservableCollection<SpaceViewItem> Items { get; set; }

        public async void SetItems(JArray spaces, Geoposition currentGeoposition)
        {
            // If items have something, clear.
            if (this.Items.Count > 0)
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
            if (this.Items.Count > 0)
                this.Items.Clear();

            // Convert spaces to space view items and set to view model
            foreach (Space space in spaces)
            {
                // Get whether this account likes this space
                bool isLike = await App.AccountSpaceRelationManager.IsLikeAsync(space.id);
                this.Items.Add(this.MakeSpaceViewItemFromSpace(space, isLike));
            }
        }


        // Mutex
        public bool IsDataLoading { get; set; }


        // Do selection changed event job.
        public async Task LikeAsync(SpaceViewItem spaceViewItem)
        {
            // Get information about image
            string spaceId = spaceViewItem.SpaceId;
            string spaceLikeButtonImageUri = spaceViewItem.SpaceLikeButtonImage.ToString();


            // Set Image and number first for good user experience.
            // Like or Note LIke by current state
            if (spaceLikeButtonImageUri.Equals(SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH))  // Do Like
            {
                spaceViewItem.SpaceLikeNumber++;
                spaceViewItem.SpaceLikeNumberColor = ColorHexStringToBrushConverter.LIKE_COLOR;
                spaceViewItem.SpaceLikeButtonImage = SpaceViewModel.LIKE_PRESS_IMAGE_PATH;

                // If like fail, set image and number back to original
                if (!(await App.AccountSpaceRelationManager.LikeAysnc(spaceId, true)))
                {
                    spaceViewItem.SpaceLikeNumber--;
                    spaceViewItem.SpaceLikeNumberColor = ColorHexStringToBrushConverter.LIKE_NOT_COLOR;
                    spaceViewItem.SpaceLikeButtonImage = SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH;
                }
            }

            else  // Do Not Like
            {
                spaceViewItem.SpaceLikeNumber--;
                spaceViewItem.SpaceLikeNumberColor = ColorHexStringToBrushConverter.LIKE_NOT_COLOR;
                spaceViewItem.SpaceLikeButtonImage = SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH;

                // If not like fail, set image back to original
                if (!(await App.AccountSpaceRelationManager.LikeAysnc(spaceId, false)))
                {
                    spaceViewItem.SpaceLikeNumber++;
                    spaceViewItem.SpaceLikeNumberColor = ColorHexStringToBrushConverter.LIKE_COLOR;
                    spaceViewItem.SpaceLikeButtonImage = SpaceViewModel.LIKE_PRESS_IMAGE_PATH;
                }
            }
        }


        public SpaceViewModel()
        {
            this.Items = new ObservableCollection<SpaceViewItem>();
        }



        /*** Self Method ***/

        // Make new space view item from space model object.
        private SpaceViewItem MakeSpaceViewItemFromSpace(Space space, bool isLike)
        {
            // Set new space view item
            SpaceViewItem spaceViewItem = new SpaceViewItem();
            spaceViewItem.SpaceName = space.space_name;
            spaceViewItem.AccountName = space.account_name;
            spaceViewItem.SpaceLikeNumber = space.space_like_number;
            spaceViewItem.SpaceId = space.id;
            spaceViewItem.SpaceDistance = Math.Round(space.space_distance);


            // If this account likes this space, set like image
            // Otherwise, set not like image
            if (isLike)
            {
                spaceViewItem.SpaceLikeNumberColor = ColorHexStringToBrushConverter.LIKE_COLOR;
                spaceViewItem.SpaceLikeButtonImage = SpaceViewModel.LIKE_PRESS_IMAGE_PATH;
            }

            else
            {
                spaceViewItem.SpaceLikeNumberColor = ColorHexStringToBrushConverter.LIKE_NOT_COLOR;
                spaceViewItem.SpaceLikeButtonImage = SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH;
            }
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

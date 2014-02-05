using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using PintheCloud.Models;
using PintheCloud.Pages;
using PintheCloud.Resources;
using PintheCloud.ViewModels;
using PintheCloud.Workers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace PintheCloud.Managers
{
    public class SpaceManagerImplement : SpaceManager
    {
        /*** Instance ***/

        private SpaceWorker CurrentSpaceWorker = null;
        public void SetAccountWorker(SpaceWorker CurrentSpaceWorker)
        {
            this.CurrentSpaceWorker = CurrentSpaceWorker;
        }

        private AccountSpaceRelationWorker CurrentAccountSpaceRelationWorker = null;
        public void SetAccountSpaceRelationWorker(AccountSpaceRelationWorker CurrentAccountSpaceRelationWorker)
        {
            this.CurrentAccountSpaceRelationWorker = CurrentAccountSpaceRelationWorker;
        }



        /*** Implementation ***/

        // Get space view item from space list.
        public async Task<ObservableCollection<SpaceViewItem>> GetNearSpaceViewItemsAsync(Geoposition currentGeoposition)
        {
            // Get current coordinate
            double currentLatitude = currentGeoposition.Coordinate.Latitude;
            double currentLongtitude = currentGeoposition.Coordinate.Longitude;

            // Get spaces formed JArray
            JArray spaces = await this.CurrentSpaceWorker.GetNearSpacesAsync(currentLatitude, currentLongtitude);
            ObservableCollection<SpaceViewItem> items = null;

            // Convert jarray spaces to space view items and set to view model
            if (spaces != null)
            {
                items = new ObservableCollection<SpaceViewItem>();
                foreach (JObject jSpace in spaces)
                {
                    string space_id = (string) jSpace["id"];
                    string space_name = (string) jSpace["space_name"];
                    double space_latitude = (double) jSpace["space_latitude"];
                    double space_longtitude = (double) jSpace["space_longtitude"];
                    string account_id = (string) jSpace["account_id"];
                    string account_name = (string)jSpace["account_name"];
                    int space_like_number = (int) jSpace["space_like_number"];

                    // Get whether this account likes this space
                    AccountSpaceRelation isLike = await this.CurrentAccountSpaceRelationWorker
                        .IsLikeAsync(App.CurrentAccountManager.GetCurrentAcccount().account_platform_id, space_id);

                    Space space = new Space(space_name, space_latitude, space_longtitude, account_id, account_name, space_like_number);
                    space.id = space_id;

                    items.Add(this.MakeSpaceViewItemFromSpace(space, isLike, currentLatitude, currentLongtitude));
                }
            }

            return items;
        }


        // Get space view item from space list.
        public async Task<ObservableCollection<SpaceViewItem>> GetMySpaceViewItemsAsync()
        {
            // Get spaces
            MobileServiceCollection<Space, Space> spaces = await this.CurrentSpaceWorker
                .GetMySpacesAsync(App.CurrentAccountManager.GetCurrentAcccount().account_platform_id);
            ObservableCollection<SpaceViewItem> items = null;

            // Convert spaces to space view items and set to view model
            if (spaces != null)
            {
                items = new ObservableCollection<SpaceViewItem>();
                foreach (Space space in spaces)
                {
                    // Get whether this account likes this space
                    AccountSpaceRelation isLike = await this.CurrentAccountSpaceRelationWorker
                        .IsLikeAsync(App.CurrentAccountManager.GetCurrentAcccount().account_platform_id, space.id);

                    items.Add(this.MakeSpaceViewItemFromSpace(space, isLike));
                }
            }

            return items;
        }



        /*** Self Method ***/

        // Make new space view item from space model object.
        private SpaceViewItem MakeSpaceViewItemFromSpace(Space space, AccountSpaceRelation isLike, double currentLatitude = -1, double currentLongtitude = -1)
        {
            // Set new space view item
            SpaceViewItem spaceViewItem = new SpaceViewItem();
            spaceViewItem.SpaceName = space.space_name;
            spaceViewItem.AccountName = space.account_name;
            spaceViewItem.SpaceLike = space.space_like_number + " " + AppResources.LikeDescription;
            spaceViewItem.SpaceId = space.id;
 
            // If it requires distance, set distance description
            // Otherwise, Set blank.
            if (currentLatitude != -1)
            {
                double distance = App.CurrentGeoCalculateManager.GetDistanceBetweenTwoCoordiantes
                    (currentLatitude, space.space_latitude, currentLongtitude, space.space_longtitude);
                spaceViewItem.SpaceDistance = Math.Round(distance) + AppResources.Meter;
            }
            else
            {
                spaceViewItem.SpaceDistance = "";
            }

            // If this account likes this space, set like image
            // Otherwise, set not like image
            if (isLike != null)
                spaceViewItem.SpaceLikeButtonImage = new Uri(SpaceViewModel.LIKE_PRESS_IMAGE_PATH, UriKind.Relative);
            else
                spaceViewItem.SpaceLikeButtonImage = new Uri(SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH, UriKind.Relative);
            return spaceViewItem;
        }

        // TODO Sort space list
    }
}
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using PintheCloud.Models;
using PintheCloud.Resources;
using PintheCloud.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Workers
{
    public abstract class SpaceWorker
    {
        // Get spaces from DB
        public async Task<MobileServiceCollection<Space, Space>> GetMySpacesAsync(string account_id)
        {
            MobileServiceCollection<Space, Space> spaces = null;
            try
            {
                // Load current account's spaces
                spaces = await App.MobileService.GetTable<Space>()
                    .Where(s => s.account_id == account_id)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException)
            {
            }

            if (spaces.Count > 0)
                return spaces;
            else
                return null;
        }


        // Get spaces 300m away from here
        public async Task<JArray> GetNearSpacesAsync(double currentLatitude, double currentLongtitude)
        {
            string json = @"{'currentLatitude':" + currentLatitude + ",'currentLongtitude':" + currentLongtitude + "}";
            JToken jToken = JToken.Parse(json);
            JArray spaces = null;
            try
            {
                // Load near spaces use custom api in server script
                spaces = (JArray) await App.MobileService.InvokeApiAsync("select_near_spaces_async", jToken);
            }
            catch (MobileServiceInvalidOperationException)
            {
            }
            if (spaces.Count > 0)
                return spaces;
            else
                return null;
        }


        // Make new space view item from space model object.
        public SpaceViewItem MakeSpaceViewItemFromSpace(Space space, double currentLatitude = -1, double currentLongtitude = -1)
        {
            // Set new space view item
            SpaceViewItem spaceViewItem = new SpaceViewItem();
            spaceViewItem.SpaceName = space.space_name;
            spaceViewItem.SpaceLikeDescription = space.space_like_number + " " + AppResources.LikeDescription;

            // If it requires distance, set distance description
            // Otherwise, Set blank.
            if (currentLatitude != -1)
            {
                double distance = App.CurrentGeoCalculateManager.GetDistanceBetweenTwoCoordiantes
                    (currentLatitude, space.space_latitude, currentLongtitude, space.space_longtitude);
                spaceViewItem.SpaceDescription = Math.Round(distance) + " " + AppResources.DistanceDescription;
            }
            else
            {
                spaceViewItem.SpaceDescription = "";
            }
            return spaceViewItem;
        }
    }
}

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


        public async Task<JArray> GetNearSpacesAsync(double currentLatitude, double currentLongtitude)
        {
            // TODO
            // Get spaces 300m away from here

            JArray spaces = null;
            try
            {
                string json = @"{'currentLatitude':" + currentLatitude + ",'currentLongtitude':" + currentLongtitude + "}";
                JToken jToken = JToken.Parse(json);
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


        public SpaceViewItem MakeSpaceViewItemFromSpace(Space space)
        {
            SpaceViewItem spaceViewItem = new SpaceViewItem();
            spaceViewItem.SpaceName = space.space_name;
            spaceViewItem.SpaceLikeDescription = space.space_like_number + " " + AppResources.LikeDescription;
            spaceViewItem.SpaceDescription = space.space_latitude + "";  // TODO Use algorithm to calc distance from lati and longti
            return spaceViewItem;
        }
    }
}

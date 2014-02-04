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
    public class SpaceWorker
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
    }
}

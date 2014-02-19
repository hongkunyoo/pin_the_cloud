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
        public async Task<bool> PinSpaceAsync(Space space)
        {
            try
            {
                await App.MobileService.GetTable<Space>().InsertAsync(space);
            }
            catch (MobileServiceInvalidOperationException)
            {
                return false;
            }
            return true;
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
                spaces = (JArray)await App.MobileService.InvokeApiAsync("select_near_spaces_async", jToken);
            }
            catch (MobileServiceInvalidOperationException)
            {
                return null;
            }
            if (spaces.Count > 0)
                return spaces;
            else
                return null;
        }


        // Get spaces from DB
        public async Task<JArray> GetMySpacesAsync(List<string> ids)
        {
            JArray spaces = null;
            try
            {
                // Load current account's spaces
                spaces = (JArray)await App.MobileService.InvokeApiAsync<List<string>, JArray>("select_my_spaces_async", ids);
            }
            catch (MobileServiceInvalidOperationException)
            {
                return null;
            }
            if (spaces.Count > 0)
                return spaces;
            else
                return null;
        }
    }
}

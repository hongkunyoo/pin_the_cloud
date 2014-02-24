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
    public class SpotWorker
    {
        public async Task<bool> PinSpotAsync(Spot spot)
        {
            try
            {
                await App.MobileService.GetTable<Spot>().InsertAsync(spot);
            }
            catch (MobileServiceInvalidOperationException)
            {
                return false;
            }
            return true;
        }


        public async Task<bool> DeleteSpotAsync(Spot spot)
        {
            try
            {
                await App.MobileService.GetTable<Spot>().DeleteAsync(spot);
            }
            catch (MobileServiceInvalidOperationException)
            {
                return false;
            }
            return true;
        }


        // Get spots 300m away from here
        public async Task<JArray> GetNearSpotsAsync(double currentLatitude, double currentLongtitude)
        {
            string json = @"{'currentLatitude':" + currentLatitude + ",'currentLongtitude':" + currentLongtitude + "}";
            JToken jToken = JToken.Parse(json);
            JArray spots = null;
            try
            {
                // Load near spots use custom api in server script
                spots = (JArray)await App.MobileService.InvokeApiAsync("select_near_spots_async", jToken);
            }
            catch (MobileServiceInvalidOperationException)
            {
                return null;
            }
            if (spots.Count > 0)
                return spots;
            else
                return null;
        }


        // Get spots from DB
        public async Task<JArray> GetMySpotsAsync(List<string> ids)
        {
            JArray spots = null;
            try
            {
                // Load current account's spots
                spots = (JArray)await App.MobileService.InvokeApiAsync<List<string>, JArray>("select_my_spots_async", ids);
            }
            catch (MobileServiceInvalidOperationException)
            {
                return null;
            }
            if (spots.Count > 0)
                return spots;
            else
                return null;
        }
    }
}

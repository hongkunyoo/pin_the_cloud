using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using PintheCloud.Models;
using PintheCloud.Pages;
using PintheCloud.Resources;
using PintheCloud.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace PintheCloud.Managers
{
    public class SpotManagerImplement : SpotManager
    {
        /*** Implementation ***/

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


        // Get spot view item from spot list.
        public async Task<JArray> GetNearSpotViewItemsAsync(Geoposition currentGeoposition)
        {
            // Get current coordinate
            double currentLatitude = currentGeoposition.Coordinate.Latitude;
            double currentLongtitude = currentGeoposition.Coordinate.Longitude;

            // Get spots formed JArray
            return await this.GetNearSpotsAsync(currentLatitude, currentLongtitude);
        }


        // Get spot view item from spot list.
        public async Task<JArray> GetMySpotViewItemsAsync()
        {
            // Get My spots
            List<string> ids = new List<string>();
            for (int i = 0; i < App.IStorageManagers.Length; i++)
            {
                if (App.IStorageManagers[i].IsSignIn())
                {
                    // Wait task
                    IStorageManager iStorageManager = App.IStorageManagers[i];
                    await App.TaskHelper.WaitSignInTask(iStorageManager.GetStorageName());
                    await App.TaskHelper.WaitSignOutTask(iStorageManager.GetStorageName());

                    // If it wasn't signed out, set list.
                    // Othersie, show sign in grid.
                    if (iStorageManager.GetAccount() != null)  // Wasn't signed out.
                        ids.Add(iStorageManager.GetAccount().account_platform_id);
                }
            }
            if (ids.Count > 0)
                return await this.GetMySpotsAsync(ids);
            else
                return null;
        }



        /*** Private Methods ***/

        // Get spots 300m away from here
        private async Task<JArray> GetNearSpotsAsync(double currentLatitude, double currentLongtitude)
        {
            string currentLatitudeString = currentLatitude.ToString().Replace(',', '.');
            string currentLongtitudeString = currentLongtitude.ToString().Replace(',', '.');
            string json = @"{'currentLatitude':" + currentLatitudeString + ",'currentLongtitude':" + currentLongtitudeString + "}";
            JToken jToken = null;
            try
            {
                jToken = JToken.Parse(json);
            }
            catch (Exception)
            { 
            }
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
        private async Task<JArray> GetMySpotsAsync(List<string> ids)
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
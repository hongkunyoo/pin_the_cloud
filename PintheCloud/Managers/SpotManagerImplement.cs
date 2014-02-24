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
    public class SpotManagerImplement : SpotManager
    {
        /*** Instance ***/

        private SpotWorker CurrentSpotWorker = new SpotWorker();



        /*** Implementation ***/

        public async Task<bool> PinSpotAsync(Spot spot)
        {
            return await this.CurrentSpotWorker.PinSpotAsync(spot);
        }


        public async Task<bool> DeleteSpotAsync(Spot spot)
        {
            return await this.CurrentSpotWorker.DeleteSpotAsync(spot);
        }


        // Get spot view item from spot list.
        public async Task<JArray> GetNearSpotViewItemsAsync(Geoposition currentGeoposition)
        {
            // Get current coordinate
            double currentLatitude = currentGeoposition.Coordinate.Latitude;
            double currentLongtitude = currentGeoposition.Coordinate.Longitude;

            // Get spots formed JArray
            return await this.CurrentSpotWorker.GetNearSpotsAsync(currentLatitude, currentLongtitude);
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
                    await App.TaskManager.WaitSignInTask(i);
                    ids.Add(App.IStorageManagers[i].GetAccount().account_platform_id);  
                }
            }
            if (ids.Count <= 0)
                return null;
            else
                return await this.CurrentSpotWorker.GetMySpotsAsync(ids);
        }


        // TODO Sort spot list
    }
}
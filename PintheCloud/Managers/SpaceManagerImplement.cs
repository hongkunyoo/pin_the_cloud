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

        private SpaceWorker CurrentSpaceWorker = new SpaceWorker();



        /*** Implementation ***/

        public async Task<bool> PinSpaceAsync(Space space)
        {
            return await this.CurrentSpaceWorker.PinSpaceAsync(space);
        }


        // Get space view item from space list.
        public async Task<JArray> GetNearSpaceViewItemsAsync(Geoposition currentGeoposition)
        {
            // Get current coordinate
            double currentLatitude = currentGeoposition.Coordinate.Latitude;
            double currentLongtitude = currentGeoposition.Coordinate.Longitude;

            // Get spaces formed JArray
            return await this.CurrentSpaceWorker.GetNearSpacesAsync(currentLatitude, currentLongtitude);
        }


        // Get space view item from space list.
        public async Task<JArray> GetMySpaceViewItemsAsync()
        {
            // Get My spaces
            List<string> ids = new List<string>();
            string id = null;
            for (int i = 0; i < Account.ACCOUNT_ID_KEYS.Length; i++)
                if (App.ApplicationSettings.TryGetValue<string>(Account.ACCOUNT_ID_KEYS[i], out id))
                    ids.Add(id);

            if (ids.Count <= 0)
                return null;
            else
                return await this.CurrentSpaceWorker.GetMySpacesAsync(ids);
        }


        // TODO Sort space list
    }
}
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


        // Get parameters from given space view item
        public string GetParameterStringFromSpaceViewItem(SpaceViewItem spaceViewItem)
        {
            // Go to File List Page with parameters.
            string spaceId = spaceViewItem.SpaceId;
            string spaceName = spaceViewItem.SpaceName;
            string accountId = spaceViewItem.AccountId;
            string accountName = spaceViewItem.AccountName;
            string parameters = "?spaceId=" + spaceId + "&spaceName=" + spaceName + "&accountId=" + accountId + "&accountName=" + accountName;
            return parameters;
        }


        // TODO Sort space list
    }
}
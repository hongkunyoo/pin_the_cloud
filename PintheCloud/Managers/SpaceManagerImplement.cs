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
        public async Task<MobileServiceCollection<Space, Space>> GetMySpaceViewItemsAsync()
        {
            // Get spaces
            return await this.CurrentSpaceWorker
                .GetMySpacesAsync(App.AccountManager.GetCurrentAcccount().account_platform_id);
        }


        // Get parameters from given space view item
        public string GetParameterStringFromSpaceViewItem(SpaceViewItem spaceViewItem)
        {
            // Go to File List Page with parameters.
            string spaceId = spaceViewItem.SpaceId;
            string spaceName = spaceViewItem.SpaceName;
            string accountName = spaceViewItem.AccountName;
            int spaceLike = spaceViewItem.SpaceLikeNumber;
            string spaceLikeButtonImage = spaceViewItem.SpaceLikeButtonImage;
            string parameters = "?spaceId=" + spaceId + "&spaceName=" + spaceName + "&accountName=" + accountName
                + "&spaceLike=" + spaceLike + "&spaceLikeButtonImage=" + spaceLikeButtonImage;

            return parameters;
        }


        // TODO Sort space list
    }
}
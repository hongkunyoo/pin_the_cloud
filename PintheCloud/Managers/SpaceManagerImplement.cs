using Microsoft.WindowsAzure.MobileServices;
using PintheCloud.Models;
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

        public async Task<ObservableCollection<SpaceViewItem>> GetMySpaceViewItemsAsync()
        {
            // Get space view item from space list.
            MobileServiceCollection<Space, Space> spaces = await this.CurrentSpaceWorker
                .GetMySpacesAsync(App.CurrentAccountManager.GetCurrentAcccount().account_platform_id);
            return this.GetSpaceViewItemsFromSpaces(spaces);
        }


        public async Task<ObservableCollection<SpaceViewItem>> GetNearSpaceViewItemsAsync(Geoposition currentGeoposition)
        {
            // Get space view item from space list.
            double currentLatitude = currentGeoposition.Coordinate.Latitude;
            double currentLongtitude = currentGeoposition.Coordinate.Longitude;

            MobileServiceCollection<Space, Space> spaces = await this.CurrentSpaceWorker
                .GetNearSpacesAsync(currentLatitude, currentLongtitude);
            return this.GetSpaceViewItemsFromSpaces(spaces);
        }


        private ObservableCollection<SpaceViewItem> GetSpaceViewItemsFromSpaces(MobileServiceCollection<Space, Space> spaces)
        {
            ObservableCollection<SpaceViewItem> items = null;
            if (spaces != null)
            {
                items = new ObservableCollection<SpaceViewItem>();
                foreach (Space space in spaces)
                    items.Add(this.CurrentSpaceWorker.MakeSpaceViewItemFromSpace(space));
            }
            return items;
        }
    }
}
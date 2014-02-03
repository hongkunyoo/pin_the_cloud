﻿using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
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

        // Get space view item from space list.
        public async Task<ObservableCollection<SpaceViewItem>> GetMySpaceViewItemsAsync()
        {
            // Get spaces
            MobileServiceCollection<Space, Space> spaces = await this.CurrentSpaceWorker
                .GetMySpacesAsync(App.CurrentAccountManager.GetCurrentAcccount().account_platform_id);

            // Convert spaces to space view items
            ObservableCollection<SpaceViewItem> items = null;
            if (spaces != null)
            {
                items = new ObservableCollection<SpaceViewItem>();
                foreach (Space space in spaces)
                {
                    items.Add(this.CurrentSpaceWorker.MakeSpaceViewItemFromSpace(space));
                }
                    
            }
            return items;
        }


        // Get space view item from space list.
        public async Task<ObservableCollection<SpaceViewItem>> GetNearSpaceViewItemsAsync(Geoposition currentGeoposition)
        {
            // Get current coordinate
            double currentLatitude = currentGeoposition.Coordinate.Latitude;
            double currentLongtitude = currentGeoposition.Coordinate.Longitude;

            // Get spaces formed JArray
            JArray spaces = await this.CurrentSpaceWorker.GetNearSpacesAsync(currentLatitude, currentLongtitude);

            // Convert jarray spaces to space view items
            ObservableCollection<SpaceViewItem> items = null;
            if (spaces != null)
            {
                items = new ObservableCollection<SpaceViewItem>();
                foreach (JObject space in spaces)
                {
                    string space_name = (string) space["space_name"];
                    double space_latitude = (double) space["space_latitude"];
                    double space_longtitude = (double) space["space_longtitude"];
                    string account_id = (string) space["account_id"];
                    int space_like_number = (int) space["space_like_number"];
                    items.Add(this.CurrentSpaceWorker.MakeSpaceViewItemFromSpace(new Space(space_name, space_latitude, space_longtitude, account_id, space_like_number),
                        currentLatitude, currentLongtitude));
                }
            }
            return items;
        }
    }
}
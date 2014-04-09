using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using PintheCloud.Helpers;
using PintheCloud.Models;
using PintheCloud.Pages;
using PintheCloud.Resources;
using PintheCloud.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace PintheCloud.Managers
{
    public class SpotManager
    {
        private List<SpotObject> spotList;
        private List<SpotObject> mySpotList;
        private SpotObject newSpot;



        // Check Spot Password
        public async Task<bool> CheckSpotPasswordAsync(string spotId, string spotPassword)
        {
            string json = @"{'spotId':'" + spotId + "','spotPassword':'" + spotPassword + "'}";
            JToken jToken = JToken.Parse(json);
            try
            {
                // Load current account's spots
                await App.MobileService.InvokeApiAsync("check_spot_password_async", jToken);
            }
            catch
            {
                return false;
            }
            return true;
        }



        /*** Private Methods ***/

        // Get spots 300m away from here
        private async Task<JArray> GetNearSpotsAsync(double currentLatitude, double currentLongtitude)
        {
            // Load near spots use custom api in server script
            string currentLatitudeString = currentLatitude.ToString().Replace(',', '.');
            string currentLongtitudeString = currentLongtitude.ToString().Replace(',', '.');
            string json = @"{'currentLatitude':" + currentLatitudeString + ",'currentLongtitude':" + currentLongtitudeString + "}";
            JToken jToken = JToken.Parse(json);
            JArray spots = null;
            try
            {
                spots = (JArray)await App.MobileService.InvokeApiAsync("select_near_spots_async", jToken);
            }
            catch
            {
                return null;
            }
            return spots;
        }


        // Get current account's spots from DB
        private async Task<JArray> GetMySpotsAsync(string ptcAccountId)
        {
            string json = @"{'ptcAccountId':'" + ptcAccountId + "'}";
            JToken jToken = JToken.Parse(json);
            JArray spots = new JArray();
            try
            {
                spots = (JArray)await App.MobileService.InvokeApiAsync("select_my_spots_async", jToken);
            }
            catch
            {
                return null;
            }
            return spots;
        }


        public async Task<bool> CreateSpotAsync(SpotObject so)
        {
            MSSpotObject spot = SpotObject.ConvertToMSSpotObject(so);
            try
            {
                await App.MobileService.GetTable<MSSpotObject>().InsertAsync(spot);
            }
            catch
            {
                return false;
            }
            so.Id = spot.id;
            this.newSpot = so;
            return true;
        }


        public async Task<bool> DeleteSpotAsync(string spotId)
        {
            MSSpotObject msso = new MSSpotObject("", 0, 0, "", "", 0, false, "");
            msso.id = spotId;
            try
            {
                await App.MobileService.GetTable<MSSpotObject>().DeleteAsync(msso);
            }
            catch
            {
                return false;
            }
            return true;
        }


        public async Task<List<SpotObject>> GetNearSpotListAsync(Geoposition currentGeoposition)
        {
            List<SpotObject> list = new List<SpotObject>();

            // Get current coordinate
            double currentLatitude = currentGeoposition.Coordinate.Latitude;
            double currentLongtitude = currentGeoposition.Coordinate.Longitude;

            // Get spots formed JArray
            JArray jSpots = await this.GetNearSpotsAsync(currentLatitude, currentLongtitude);

            // If loading spot doesn't occur error, Convert jarray spots to spot list
            if (jSpots != null)
            {
                foreach (JObject jSpot in jSpots)
                {
                    // Set new spot view item
                    string spotId = (string)jSpot["id"];
                    string spotName = (string)jSpot["spot_name"];
                    double spotLatitude = (double)jSpot["spot_latitude"];
                    double spotLongtitude = (double)jSpot["spot_longtitude"];
                    string accountId = (string)jSpot["ptcaccount_id"];
                    string accountName = (string)jSpot["ptcaccount_name"];
                    double spotDistance = (double)jSpot["spot_distance"];
                    bool isPrivate = (bool)jSpot["is_private"];
                    string spot_password = (string)jSpot["spot_password"];
                    string create_at = (string)jSpot["create_at"];

                    SpotObject spot = new SpotObject(spotName, spotLatitude, spotLongtitude, accountId, accountName, spotDistance, isPrivate, spot_password, create_at);
                    spot.Id = spotId;
                    list.Add(spot);
                }
            }
            else
            {
                return null;
            }
            this.spotList = list;
            return list;
        }


        public async Task<List<SpotObject>> GetMySpotList()
        {
            // Get signed in my spots formed JArray
            // If loading spot doesn't occur error, Convert jarray spots to spot list
            List<SpotObject> spots = new List<SpotObject>();
            JArray jSpots = await this.GetMySpotsAsync(App.AccountManager.GetPtcId());
            if (jSpots != null)
            {
                foreach (JObject jSpot in jSpots)
                {
                    // Set new spot view item
                    string spotId = (string)jSpot["id"];
                    string spotName = (string)jSpot["spot_name"];
                    double spotLatitude = (double)jSpot["spot_latitude"];
                    double spotLongtitude = (double)jSpot["spot_longtitude"];
                    string accountId = (string)jSpot["ptcaccount_id"];
                    string accountName = (string)jSpot["ptcaccount_name"];
                    double spotDistance = (double)jSpot["spot_distance"];
                    bool isPrivate = (bool)jSpot["is_private"];
                    string spot_password = (string)jSpot["spot_password"];
                    string create_at = (string)jSpot["create_at"];

                    SpotObject spot = new SpotObject(spotName, spotLatitude, spotLongtitude, accountId, accountName, 
                        spotDistance, isPrivate, spot_password, create_at);
                    spot.Id = spotId;
                    spots.Add(spot);
                }
            }
            else
            {
                return null;
            }
            this.mySpotList = spots;
            return spots;
        }


        public SpotObject GetSpotObject(string spotId)
        {
            if (this.spotList != null)
                for (var i = 0; i < this.spotList.Count; i++)
                    if (spotList[i].Id.Equals(spotId)) return spotList[i];

            if (this.mySpotList != null)
                for (var i = 0; i < this.mySpotList.Count; i++)
                    if (mySpotList[i].Id.Equals(spotId)) return mySpotList[i];

            if (this.newSpot != null)
                if (this.newSpot.Id.Equals(spotId)) return this.newSpot;

            return null;
        }

    }
}
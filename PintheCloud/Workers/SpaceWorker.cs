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
            }
            if (spaces.Count > 0)
                return spaces;
            else
                return null;
        }


        // Get spaces from DB
        public async Task<MobileServiceCollection<Space, Space>> GetMySpacesAsync(string account_id)
        {
            MobileServiceCollection<Space, Space> spaces = null;
            try
            {
                ///*** DEBUG CODE ***/
                //Random random = new Random();
                //double currentLat = 37.61017539;
                //double currentLon = 126.99735290;
                //double unit = 100000000.0;

                //double random1 = ((double)random.Next(10)) / unit;
                //double random2 = ((double)random.Next(10)) / unit;
                //await App.MobileService.GetTable<Space>().InsertAsync(new Space("Imagine Cup", currentLat + random1, currentLon + random2, account_id, "Seungmin Lee", 0, 0));

                //random1 = ((double)random.Next(10)) / unit;
                //random2 = ((double)random.Next(10)) / unit;
                //await App.MobileService.GetTable<Space>().InsertAsync(new Space("Innovation", currentLat + random1, currentLon - random2, "MicrosoftAccount:511dbec057e113d9a3b9d6dc79f339ef", "Chaesoo Rim", 0, 0));

                //random1 = ((double)random.Next(10)) / unit;
                //random2 = ((double)random.Next(10)) / unit;
                //await App.MobileService.GetTable<Space>().InsertAsync(new Space("Pin the Cloud", currentLat - random1, currentLon + random2, "MicrosoftAccount:b8547ee2dd7eb2ac6fcf3a4f3c6c56aa", "hongkun yoo", 0, 0));

                //random1 = ((double)random.Next(10)) / unit;
                //random2 = ((double)random.Next(10)) / unit;
                //await App.MobileService.GetTable<Space>().InsertAsync(new Space("At Here", currentLat - random1, currentLon - random2, "MicrosoftAccount:tempid", "Hwajeong Kim", 0, 0));

                //random1 = ((double)random.Next(10)) / unit;
                //random2 = ((double)random.Next(10)) / unit;
                //await App.MobileService.GetTable<Space>().InsertAsync(new Space("Direct Sharing", currentLat + random1, currentLon + random2, account_id, "Seungmin Lee", 0, 0));
                ///*** DEBUG CODE **/*

                // Load current account's spaces
                spaces = await App.MobileService.GetTable<Space>()
                    .Where(s => s.account_id == account_id)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException)
            {
            }

            if (spaces.Count > 0)
                return spaces;
            else
                return null;
        }
    }
}

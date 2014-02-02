using Microsoft.WindowsAzure.MobileServices;
using PintheCloud.Models;
using PintheCloud.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Workers
{
    public class SpaceInternetAvailableWorker : SpaceWorker
    {
        // Get spaces from DB
        public override async Task<MobileServiceCollection<Space, Space>> GetMySpacesAsync(string account_id)
        {
            MobileServiceCollection<Space, Space> spaces = null;  
            try
            {
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

        public override Task<MobileServiceCollection<Space, Space>> GetNearSpacesAsync(string account_id)
        {
            // TODO
            // Get current position.
            // Calc distance between each space to current position
            // Get spaces 300m away from here
            return null;
        }
    }
}

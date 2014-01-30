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
        public override async Task<MobileServiceCollection<Space, Space>> GetMyNearSpacesAsync(string account_id)
        {
            MobileServiceCollection<Space, Space> spaces = null;

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    await App.MobileService.GetTable<Space>().InsertAsync(new Space("Test", 0, 0, account_id, 0));
                }
                catch (MobileServiceInvalidOperationException)
                { 
                }
            }
                
            try
            {
                // TODO add algorithm of geo calculate
                // Now, just load all spaces of current account's
                spaces = await App.MobileService.GetTable<Space>()
                    .Where(s => s.Account_id == account_id)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException)
            {
            }

            return spaces;
        }
    }
}

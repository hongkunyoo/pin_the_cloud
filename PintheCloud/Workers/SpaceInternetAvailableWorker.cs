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
        public override async Task<MobileServiceCollection<Space, Space>> GetSpaceViewModelAsync(string account_id)
        {
            MobileServiceCollection<Space, Space> spaces = null;
            try
            {
                // TODO add algorithm of geo calculate
                // TODO Now, just load all spaces of current account's
                spaces = await App.MobileService.GetTable<Space>()
                    .Where(s => s.account_id == account_id)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException)
            {
            }

            return spaces;
        }
    }
}

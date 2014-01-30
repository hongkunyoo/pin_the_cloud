using Microsoft.WindowsAzure.MobileServices;
using PintheCloud.Models;
using PintheCloud.ViewModels;
using PintheCloud.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<MobileServiceCollection<Space, Space>> GetSpaceViewModelAsync()
        {
            // TODO Get Space list and set to this space view model.
            return await this.CurrentSpaceWorker.GetSpaceViewModelAsync(App.CurrentAccountManager.GetCurrentAcccount().account_platform_id);
        }
    }
}
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

        public async Task<SpaceViewModel> GetSpaceViewModelAsync()
        {
            // TODO Get Space list and set to this space view model.
            return await this.CurrentSpaceWorker.GetSpaceViewModelAsync();
        }
    }
}
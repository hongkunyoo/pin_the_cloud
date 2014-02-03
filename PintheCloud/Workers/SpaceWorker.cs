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
    public abstract class SpaceWorker
    {
        // Get spaces from DB
        public async Task<MobileServiceCollection<Space, Space>> GetMySpacesAsync(string account_id)
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


        public async Task<MobileServiceCollection<Space, Space>> GetNearSpacesAsync(double currentLatitude, double currentLongtitude)
        {
            // TODO
            // Get current position.
            // Calc distance between each space to current position
            // Get spaces 300m(?) away from here
            return await Task<MobileServiceCollection<Space, Space>>.FromResult<MobileServiceCollection<Space, Space>>(null);
        }


        public SpaceViewItem MakeSpaceViewItemFromSpace(Space space)
        {
            SpaceViewItem spaceViewItem = new SpaceViewItem();
            spaceViewItem.SpaceName = space.space_name;
            spaceViewItem.SpaceLikeDescription = space.space_like_number + " People like this space.";
            spaceViewItem.SpaceDescription = space.space_latitude + "";  // TODO Use algorithm to calc distance from lati and longti
            return spaceViewItem;
        }
    }
}

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
        public abstract Task<MobileServiceCollection<Space, Space>> GetMySpacesAsync(string account_id);
        public abstract Task<MobileServiceCollection<Space, Space>> GetNearSpacesAsync(string account_id);


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

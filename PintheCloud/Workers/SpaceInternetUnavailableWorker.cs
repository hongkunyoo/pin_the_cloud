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
    public class SpaceInternetUnavailableWorker : SpaceWorker
    {
        public override Task<MobileServiceCollection<Space, Space>> GetMyNearSpacesAsync(string account_id)
        {
            // TODO
            return null;
        }
    }
}

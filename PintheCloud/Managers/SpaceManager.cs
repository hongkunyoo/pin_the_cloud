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
    public interface SpaceManager
    {
        void SetAccountWorker(SpaceWorker CurrentSpaceWorker);

        Task<MobileServiceCollection<Space, Space>> GetMyNearSpacesAsync();
    }
}

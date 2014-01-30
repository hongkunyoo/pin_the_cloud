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
    }
}

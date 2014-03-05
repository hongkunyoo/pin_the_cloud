﻿using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using PintheCloud.Models;
using PintheCloud.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace PintheCloud.Managers
{
    public interface SpotManager
    {
        Task<bool> PinSpotAsync(Spot spot);
        Task<bool> DeleteSpotAsync(Spot spot);
        Task<JArray> GetNearSpotViewItemsAsync(Geoposition currentGeoposition);
        Task<JArray> GetMySpotViewItemsAsync();
        Task<JArray> CheckSpotPasswordAsync(string spotId, string spotPassword);
    }
}

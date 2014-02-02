using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace PintheCloud.Managers
{
    public interface GeoCalculateManager
    {
        double GetDistanceFromLatitudeLongtitude(double latitude, double longtitude);
        Task<Geoposition> GetCurrentGeopositionAsync();
        System.Device.Location.CivicAddress GetCurrentCivicAddress();
    }
}

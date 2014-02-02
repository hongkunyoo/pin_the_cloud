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
        double GetDistanceFromLatitudeLongtitude(double currentLatitude, double currentLongtitude, double destinationLatitude, double destinationLongtitude);
        Task<Geoposition> GetCurrentGeopositionAsync();
    }
}

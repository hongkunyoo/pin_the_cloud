using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace PintheCloud.Managers
{
    public class GeoCalculateManagerImplement : GeoCalculateManager
    {
        // Instances
        Geolocator geolocator = new Geolocator();


        public bool GetGeolocatorPositionStatus()
        {
            if (this.geolocator.LocationStatus != PositionStatus.Disabled)
                return true;
            else
                return false;
        }


        // Get Geolocator to use GPS for getting location info.
        public async Task<Geoposition> GetCurrentGeopositionAsync()
        {
            Geoposition geoposition = await this.geolocator.GetGeopositionAsync();
            return geoposition;
        }
    }
}

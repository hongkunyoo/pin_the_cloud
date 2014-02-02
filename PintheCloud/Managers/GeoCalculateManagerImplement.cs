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
        // Get distance between given coordinates
        public double GetDistanceFromLatitudeLongtitude(double currentLatitude, double currentLongtitude, double destinationLatitude, double destinationLongtitude)
        {
            double x = currentLongtitude - destinationLongtitude;
            double y = currentLatitude - destinationLatitude;
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        }

        // Get Geolocator to use GPS for getting location info.
        public async Task<Geoposition> GetCurrentGeopositionAsync()
        {

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;
            Geoposition geoposition = null;
            try
            {
                geoposition = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                    );
            }
            catch (Exception)
            {
            }
            return geoposition;
        }
    }
}

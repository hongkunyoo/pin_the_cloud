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
        // Get distance between two coordinates
        public double GetDistanceBetweenTwoCoordiantes(double lat1, double lat2, double lon1, double lon2)
        {
            var R = 6371; // km

            var dLat = toRad(lat2 - lat1);
            var dLon = toRad(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
                * Math.Cos(toRad(lat1)) * Math.Cos(toRad(lat2));
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;

            // return unit meter
            return d / 1000;
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


        // Converts numeric degrees to radians
        private double toRad(double value)
        {
            return value * Math.PI / 180;
        }
    }
}

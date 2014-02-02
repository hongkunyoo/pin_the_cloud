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
        public double GetDistanceFromLatitudeLongtitude(double latitude, double longtitude)
        {
            // TODO Use math algorightm
            return 0;
        }

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
            catch (Exception ex)
            {
                // the application does not have the right capability or the location master switch is off
                if ((uint)ex.HResult == 0x80004004)  
                {
                    
                }
            }
            return geoposition;
        }

        public System.Device.Location.CivicAddress GetCurrentCivicAddress()
        {
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            watcher.MovementThreshold = 1.0; // set to one meter
            watcher.TryStart(false, TimeSpan.FromMilliseconds(1000));

            CivicAddressResolver resolver = new CivicAddressResolver();

            System.Device.Location.CivicAddress address = null;
            if (watcher.Position.Location.IsUnknown == false)
            {
                address = resolver.ResolveAddress(watcher.Position.Location);
                int a = 1;
            }

            return address;
        }
    }
}

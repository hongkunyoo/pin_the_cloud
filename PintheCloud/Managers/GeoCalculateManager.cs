using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Managers
{
    public interface GeoCalculateManager
    {
        double GetDistanceFromLatitudeLongtitude(double latitude, double longtitude);
        // TODO Get current location information using GPS.
    }
}

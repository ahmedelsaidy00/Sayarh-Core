using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sayarah.Application.Helpers
{
    public static class GeoCoordinateHepler
    {
        public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371000; // Radius of Earth in meters
            var latRad1 = DegreesToRadians(lat1);
            var latRad2 = DegreesToRadians(lat2);
            var deltaLat = DegreesToRadians(lat2 - lat1);
            var deltaLon = DegreesToRadians(lon2 - lon1);

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(latRad1) * Math.Cos(latRad2) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // Distance in meters
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

    }
}

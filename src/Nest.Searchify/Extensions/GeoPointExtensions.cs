using Nest.Searchify.Utils;

namespace Nest.Searchify.Extensions
{
    public static class GeoPointExtensions
    {
        public static double DistanceTo(this GeoPoint from, GeoPoint to, GeoMath.MeasureUnits units,
            int decimalPoints = 2)
        {
            return from.DistanceTo(to.Latitude, to.Longitude, units, decimalPoints);
        }

        public static double DistanceTo(this GeoPoint from, double latitude, double longitude, GeoMath.MeasureUnits units,
            int decimalPoints = 2)
        {
            return GeoMath.Distance(from, GeoPoint.TryCreate(latitude, longitude), units, decimalPoints);
        }
    }
}
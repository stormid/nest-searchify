using Nest.Searchify.Utils;

namespace Nest.Searchify.Extensions
{
    public static class GeoLocationExtensions
    {
        public static double DistanceTo(this GeoLocation from, GeoLocation to, GeoMath.MeasureUnits units,
            int decimalPoints = 2)
        {
            return from.DistanceTo(to.Latitude, to.Longitude, units, decimalPoints);
        }

        public static double DistanceTo(this GeoLocation from, double latitude, double longitude, GeoMath.MeasureUnits units,
            int decimalPoints = 2)
        {
            return GeoMath.Distance(from, GeoLocation.TryCreate(latitude, longitude), units, decimalPoints);
        }
    }
}
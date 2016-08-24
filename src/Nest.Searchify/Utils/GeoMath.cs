using System;

namespace Nest.Searchify.Utils
{
    public class GeoMath
    {
        /// <summary>
        /// The distance type to return the results in.
        /// </summary>
        public enum MeasureUnits { Miles, Kilometers };

        public static double Distance(GeoPoint from, GeoPoint to, MeasureUnits units, int decimalPoints = 2)
        {
            return Distance(from.Latitude, from.Longitude, to.Latitude, to.Longitude, units, decimalPoints);
        }

        /// <summary>
        /// Returns the distance in miles or kilometers of any two
        /// latitude / longitude points. (Haversine formula)
        /// </summary>
        public static double Distance(double latitudeA, double longitudeA, double latitudeB, double longitudeB, MeasureUnits units, int decimalPoints = 2)
        {
            if (latitudeA <= -90 || latitudeA >= 90 || longitudeA <= -180 || longitudeA >= 180
                || latitudeB <= -90 && latitudeB >= 90 || longitudeB <= -180 || longitudeB >= 180)
            {
                throw new ArgumentException(
                    $"Invalid value point coordinates. Points A({latitudeA},{longitudeA}) B({latitudeB},{longitudeB}) ");
            }


            double R = (units == MeasureUnits.Miles) ? 3960 : 6371;
            var dLat = ToRadian(latitudeB - latitudeA);
            var dLon = ToRadian(longitudeB - longitudeA);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadian(latitudeA)) * Math.Cos(ToRadian(latitudeB)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            var d = R * c;
            return Math.Round(d, decimalPoints);
        }



        /// <summary>
        /// Convert to Radians.
        /// </summary>      
        private static double ToRadian(double val)
        {
            return (Math.PI / 180) * val;
        }

    }
}
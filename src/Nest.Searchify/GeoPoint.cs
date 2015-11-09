using System;
using System.Globalization;
using System.Linq;
using Nest.Searchify.Queries;
using Newtonsoft.Json;

namespace Nest.Searchify
{
	/*
	 * Taken from SolrNet https://github.com/mausch/SolrNet/blob/master/SolrNet/Location.cs
	 */

	/// <summary>
	/// Represents a Latitude/Longitude as a 2 dimensional point. 
	/// </summary>
	public class GeoPoint : IEquatable<GeoPoint>, IFormattable
	{
		/// <summary>
		/// Latitude
		/// </summary>
		[JsonProperty("lat")]
        public double Latitude
		{
			get { return _latitude; }
		}

		private readonly double _latitude;

		/// <summary>
		/// Longitude
		/// </summary>
		[JsonProperty("lon")]
        public double Longitude
		{
			get { return _longitude; }
		}

		private readonly double _longitude;

	    public static implicit operator GeoPoint(string value)
	    {
	        if (!string.IsNullOrWhiteSpace(value))
	        {
	            var coords = value.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).Select(Convert.ToDouble).ToArray();
	            if (coords.Length == 2)
	            {
	                return new GeoPoint(coords[0], coords[1]);
	            }
                throw new InvalidCastException("Unable to create GeoPoint from input string, string must contain 2 numeric values separated by a comma");
	        }
            throw new ArgumentNullException(nameof(value), "No GeoPoint value specified");
        }

		/// <summary>
		/// Represents a Latitude/Longitude as a 2 dimensional point. 
		/// </summary>
		/// <param name="latitude">Value between -90 and 90</param>
		/// <param name="longitude">Value between -180 and 180</param>
		/// <exception cref="ArgumentOutOfRangeException">If <paramref name="latitude"/> or <paramref name="longitude"/> are invalid</exception>
		public GeoPoint(double latitude, double longitude)
		{
			if (!IsValidLatitude(latitude))
				throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture,
					"Invalid latitude '{0}'. Valid values are between -90 and 90", latitude));
			if (!IsValidLongitude(longitude))
				throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture,
					"Invalid longitude '{0}'. Valid values are between -180 and 180", longitude));
			_latitude = latitude;
			_longitude = longitude;
		}

		/// <summary>
		/// True if <paramref name="latitude"/> is a valid latitude. Otherwise false.
		/// </summary>
		/// <param name="latitude"></param>
		/// <returns></returns>
		public static bool IsValidLatitude(double latitude)
		{
			return latitude >= -90 && latitude <= 90;
		}

		/// <summary>
		/// True if <paramref name="longitude"/> is a valid longitude. Otherwise false.
		/// </summary>
		/// <param name="longitude"></param>
		/// <returns></returns>
		public static bool IsValidLongitude(double longitude)
		{
			return longitude >= -180 && longitude <= 180;
		}

		/// <summary>
		/// Try to create a <see cref="GeoLocation"/>. 
		/// Return <value>null</value> if either <paramref name="latitude"/> or <paramref name="longitude"/> are invalid.
		/// </summary>
		/// <param name="latitude">Value between -90 and 90</param>
		/// <param name="longitude">Value between -180 and 180</param>
		/// <returns></returns>
		public static GeoPoint TryCreate(double latitude, double longitude)
		{
			if (IsValidLatitude(latitude) && IsValidLongitude(longitude))
				return new GeoPoint(latitude, longitude);
			return null;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0},{1}", _latitude, _longitude);
		}

		public bool Equals(GeoPoint other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return _latitude.Equals(other._latitude) && _longitude.Equals(other._longitude);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return Equals((GeoPoint)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_latitude.GetHashCode()*397) ^ _longitude.GetHashCode();
			}
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return ToString();
		}
	}
}
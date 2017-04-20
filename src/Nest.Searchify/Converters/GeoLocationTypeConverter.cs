using System;
using System.ComponentModel;
using System.Globalization;
using Nest.Searchify.Queries;

namespace Nest.Searchify.Converters
{
    public sealed class GeoLocationParameterTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var point = value as GeoLocationParameter;
            if (point == null) return base.ConvertTo(context, culture, value, destinationType);

            return point.ToString();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var s = value as string;
            if (s == null) return base.ConvertFrom(context, culture, value);
            GeoLocationParameter point = s;
            return point;
        }
    }
}
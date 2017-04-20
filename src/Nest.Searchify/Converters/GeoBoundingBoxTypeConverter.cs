using System;
using System.ComponentModel;
using Nest.Searchify.Queries.Models;

namespace Nest.Searchify.Converters
{
    public sealed class GeoBoundingBoxTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            var s = value as string;
            if (s == null) return base.ConvertFrom(context, culture, value);
            GeoBoundingBox bbox = s;
            return bbox;
        }
    }
}
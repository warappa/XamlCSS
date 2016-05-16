using System;
using System.Globalization;

namespace XamlCSS.ComponentModel
{
	public class EnumTypeConverter<Tout> : TypeConverter
	{
		public override bool CanConvertFrom(Type sourceType)
		{
			return sourceType == typeof(string);
		}
		public override object ConvertFrom(CultureInfo culture, object o)
		{
			return Enum.Parse(typeof(Tout), o as string, true);
		}
		public override object ConvertFrom(object o)
		{
			return Enum.Parse(typeof(Tout), o as string, true);
		}
		public override object ConvertFromInvariantString(string value)
		{
			return Enum.Parse(typeof(Tout), value, true);
		}
	}
}

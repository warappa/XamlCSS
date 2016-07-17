using System;
using System.Globalization;
using Xamarin.Forms;

namespace XamlCSS.ComponentModel
{
	public class NumberTypeConverter<Tout> : TypeConverter
	{
		public override bool CanConvertFrom(Type sourceType)
		{
			return sourceType == typeof(string);
		}
		public override object ConvertFrom(CultureInfo culture, object o)
		{
			var s = o as string;

			var t = typeof(Tout);

			object res = null;

			if (t == typeof(byte))
				res = byte.Parse(s, culture);
			else if (t == typeof(int))
				res = int.Parse(s, culture);
			else if (t == typeof(uint))
				res = uint.Parse(s, culture);
			else if (t == typeof(long))
				res = long.Parse(s, culture);
			else if (t == typeof(ulong))
				res = ulong.Parse(s, culture);
			else if (t == typeof(float))
				res = float.Parse(s, culture);
			else if (t == typeof(double))
				res = double.Parse(s, culture);
			else if (t == typeof(bool))
			{
				s = s.ToLowerInvariant();
				if (s == "true")
					res = true;
				else if (s == "false")
					res = false;
				else
					throw new InvalidOperationException($"'{o}' is not a valid value for bool!");
			}
			if (res != null)
				return res;

			throw new InvalidOperationException($"Unable to parse value '{o}' to ''");
		}
		public override object ConvertFrom(object o)
		{
			return this.ConvertFrom(CultureInfo.CurrentUICulture, o);
		}
		public override object ConvertFromInvariantString(string value)
		{
			return ConvertFrom((object)value);
		}
	}
}

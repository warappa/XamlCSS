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
			var stringValue = o as string;

			var outputType = typeof(Tout);

			object convertedValue = null;

			if (outputType == typeof(byte))
			{
				convertedValue = byte.Parse(stringValue, culture);
			}
			else if (outputType == typeof(int))
			{
				convertedValue = int.Parse(stringValue, culture);
			}
			else if (outputType == typeof(uint))
			{
				convertedValue = uint.Parse(stringValue, culture);
			}
			else if (outputType == typeof(long))
			{
				convertedValue = long.Parse(stringValue, culture);
			}
			else if (outputType == typeof(ulong))
			{
				convertedValue = ulong.Parse(stringValue, culture);
			}
			else if (outputType == typeof(float))
			{
				convertedValue = float.Parse(stringValue, culture);
			}
			else if (outputType == typeof(double))
			{
				convertedValue = double.Parse(stringValue, culture);
			}
			else if (outputType == typeof(bool))
			{
				stringValue = stringValue.ToLowerInvariant();

				if (stringValue == "true")
				{
					convertedValue = true;
				}
				else if (stringValue == "false")
				{
					convertedValue = false;
				}
				else
				{
					throw new InvalidOperationException($"'{o}' is not a valid value for bool!");
				}
			}

			if (convertedValue != null)
			{
				return convertedValue;
			}

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

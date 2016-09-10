using System;
using System.Globalization;

namespace XamlCSS.ComponentModel
{
	public class GenericTypeConverter<Tout> : TypeConverter
	{
		readonly Func<Type, bool> canConvert;
		readonly Func<CultureInfo, object, Tout> convert;

		public GenericTypeConverter(Func<object, Tout> convert, Func<Type, bool> canConvert = null)
		{
			if (convert == null)
            {
                throw new ArgumentNullException(nameof(convert));
            }

            this.canConvert = canConvert ?? (_ => true);
			this.convert = (c, o) => convert(o);
		}

		public GenericTypeConverter(Func<CultureInfo, object, Tout> convert, Func<Type, bool> canConvert = null)
		{
			if (convert == null)
            {
                throw new ArgumentNullException(nameof(convert));
            }

            this.canConvert = canConvert ?? (_ => true);
			this.convert = convert;
		}

		public override bool CanConvertFrom(Type sourceType)
		{
			return canConvert(sourceType);
		}
		public override object ConvertFrom(CultureInfo culture, object o)
		{
			return convert(culture, o);
		}
		public override object ConvertFrom(object o)
		{
			return convert(CultureInfo.CurrentUICulture, o);
		}
		public override object ConvertFromInvariantString(string value)
		{
			return convert(CultureInfo.CurrentUICulture, (object)value);
		}
	}
}

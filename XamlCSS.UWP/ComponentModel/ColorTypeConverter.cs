using System;
using System.Linq;
using Windows.UI;

namespace XamlCSS.ComponentModel
{
	public class ColorTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(Type sourceType)
		{
			return sourceType == typeof(string) ||
				sourceType == typeof(Color);
		}
		public override object ConvertFrom(object o)
		{
            if (o == null)
            {
                return null;
            }

            if (o is Color)
            {
                return o;
            }

			if (o is string)
			{
				var value = o as string;

				if (value.StartsWith("#", StringComparison.Ordinal))
				{
					string a = "ff";
					var r = value.Substring(1, 2);
					var g = value.Substring(3, 2);
					var b = value.Substring(5, 2);

					if (value.Length == 9) // alpha
					{
						a = r;
						r = g;
						g = b;
						b = value.Substring(7, 2);
					}

					return Color.FromArgb(
                        Convert.ToByte(a, 16),
						Convert.ToByte(r, 16),
						Convert.ToByte(g, 16),
						Convert.ToByte(b, 16));
				}

				return TypeHelpers.DeclaredProperties(typeof(Colors))
					.Where(x => x.Name == value)
					.Select(x => x.GetValue(null, null))
					.SingleOrDefault();
			}

			return null;
		}
	}
}

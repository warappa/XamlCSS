using System;
using System.Collections.Generic;
using System.Linq;

namespace XamlCSS.CssParsing
{
	public static class StringExtensions
	{
		public static IEnumerable<string> SplitThem(this IEnumerable<string> strings, char separator)
		{
			var stringSeparator = separator.ToString();
			
			return strings
				.SelectMany(value =>
				{
					var stringValues = value
						.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
						.SelectMany(subValue => new[] { stringSeparator, subValue });

					if (!value.StartsWith(stringSeparator, StringComparison.Ordinal))
					{
						stringValues = stringValues.Skip(1);
					}

					if (value.EndsWith(stringSeparator, StringComparison.Ordinal))
					{
						stringValues = stringValues.Concat(new[] { stringSeparator });
					}

					return stringValues;
				})
				.ToList();
		}
	}
}

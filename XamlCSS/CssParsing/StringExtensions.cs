using System;
using System.Linq;

namespace XamlCSS.CssParsing
{
	public static class StringExtensions
	{
		public static string[] SplitThem(this string[] strs, char sep)
		{
			return strs.SelectMany(x =>
			{
				var res = x.Split(new[] { sep }, StringSplitOptions.RemoveEmptyEntries)
					.SelectMany(y => new[] { sep.ToString(), y });

				if (x.StartsWith(sep.ToString(), StringComparison.Ordinal) == false)
					res = res.Skip(1);
				if (x.EndsWith(sep.ToString(), StringComparison.Ordinal))
					res = res.Concat(new[] { sep.ToString() });

				return res;
			})
			.ToArray();
		}
	}
}

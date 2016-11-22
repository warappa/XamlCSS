using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                    List<string> output = new List<string>();
                    StringBuilder sb = new StringBuilder();
                    for (var i = 0; i < value.Length; i++)
                    {
                        var c = value[i];
                        if (c != separator)
                        {
                            sb.Append(c);
                        }
                        else
                        {
                            if (sb.Length > 0)
                            {
                                output.Add(sb.ToString());
                            }
                            sb.Clear();
                            output.Add(c.ToString());
                        }
                    }
                    if (sb.Length > 0)
                    {
                        output.Add(sb.ToString());
                    }

                    return output;
                })
                .ToList();
        }
    }
}

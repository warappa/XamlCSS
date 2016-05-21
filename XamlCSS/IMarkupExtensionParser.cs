using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlCSS
{
	public interface IMarkupExtensionParser
	{
		object Parse(string expression);
		object ProvideValue(string expression, object obj);
	}
}

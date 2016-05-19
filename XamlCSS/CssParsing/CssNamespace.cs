using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlCSS.CssParsing
{
	public class CssNamespace
	{
		public CssNamespace()
		{

		}
		public CssNamespace(string alias, string @namespace)
		{
			this.Alias = alias;
			this.Namespace = @namespace;
		}

		public string Alias { get; set; }
		public string Namespace { get; set; }
	}
}

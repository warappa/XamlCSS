namespace XamlCSS.CssParsing
{
	public class CssNamespace
	{
		public CssNamespace(string alias, string @namespace)
		{
			this.Alias = alias;
			this.Namespace = @namespace;
		}

		public string Alias { get; set; }
		public string Namespace { get; set; }
	}
}

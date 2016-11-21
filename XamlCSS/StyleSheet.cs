using System.Collections.Generic;
using XamlCSS.CssParsing;

namespace XamlCSS
{
    public class StyleSheet
	{
		public static readonly StyleSheet Empty = new StyleSheet();

		private StyleRuleCollection _rules = new StyleRuleCollection();

		public List<CssNamespace> Namespaces { get; set; }

		public StyleRuleCollection Rules
		{
			get { return _rules; }
			set { _rules = value; }
		}

        private string content = null;
        public string Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
                var sheet = CssParser.Parse(content);
                this.Namespaces = sheet.Namespaces;
                this.Rules = sheet.Rules;
            }
        }
    }
}

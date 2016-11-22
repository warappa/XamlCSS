using System.Collections.Generic;
using System.ComponentModel;
using XamlCSS.CssParsing;

namespace XamlCSS
{
    public class StyleSheet :INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;

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

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Content"));
            }
        }
    }
}

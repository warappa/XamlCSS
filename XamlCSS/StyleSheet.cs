using System.Collections.Generic;
using System.ComponentModel;
using XamlCSS.CssParsing;

namespace XamlCSS
{
    public class StyleSheet : INotifyPropertyChanged
    {
        public static readonly StyleSheet Empty = new StyleSheet();

        public List<CssNamespace> Namespaces { get; set; } = new List<CssNamespace>();

        public StyleRuleCollection Rules { get; set; } = new StyleRuleCollection();
        
        public event PropertyChangedEventHandler PropertyChanged;

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

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Content"));
            }
        }
    }
}

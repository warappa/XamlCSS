using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using XamlCSS.CssParsing;

namespace XamlCSS
{
    public class StyleSheet : INotifyPropertyChanged
    {
        public static readonly StyleSheet Empty = new StyleSheet();

        protected List<CssNamespace> namespaces = new List<CssNamespace>();
        virtual public List<CssNamespace> Namespaces
        {
            get
            {
                return namespaces;
            }
            set
            {
                namespaces = value;
            }
        }

        protected StyleRuleCollection rules = new StyleRuleCollection();
        virtual public StyleRuleCollection Rules
        {
            get
            {
                return rules;
            }
            set
            {
                rules = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        virtual public object AttachedTo { get; set; }

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

        public static Func<object, object> GetParent { get; internal set; }
        public static Func<object, StyleSheet> GetStyleSheet { get; internal set; }
    }
}

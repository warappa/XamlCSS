using System.Collections.Generic;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class UnivseralSelector : SelectorFragment
    {
        public UnivseralSelector(CssNodeType type, string text) : base(type, text)
        {
            
        }

        private void Initialize(StyleSheet styleSheet)
        {
            var namespaceSeparatorIndex = Text.IndexOf('|');
            string alias = "";
            string @namespace = null;
            if (namespaceSeparatorIndex > -1)
            {
                alias = Text.Substring(0, namespaceSeparatorIndex);
                if (alias != "*")
                {
                    @namespace = styleSheet.GetNamespaceUri(alias); //domElement.LookupNamespaceUri(@namespace);
                }
            }
            else
            {
                @namespace = styleSheet.GetNamespaceUri(""); //domElement.LookupNamespaceUri("");
            }

            this.Alias = alias;
            this.NamespaceUri = @namespace;
            this.initializedWith = styleSheet;
        }

        public override bool Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorFragment[] fragments, ref int currentIndex)
        {
            if (initializedWith != styleSheet)
            {
                Initialize(styleSheet);
            }

            return domElement.NamespaceUri == NamespaceUri || NamespaceUri == "*";
        }

        public string Alias { get; private set; }
        public string NamespaceUri { get; private set; }

        private StyleSheet initializedWith;
    }
}
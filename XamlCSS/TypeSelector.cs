using System.Collections.Generic;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class TypeSelector : SelectorFragment
    {
        private StyleSheet initializedWith;

        public TypeSelector(CssNodeType type, string text) : base(type, text)
        {
            
        }

        private void Initialize(StyleSheet styleSheet)
        {
            var namespaceSeparatorIndex = Text.IndexOf('|');
            string @namespace = null;
            //string prefix = null;
            string alias = "";
            string localName = null;
            if (namespaceSeparatorIndex > -1)
            {
                alias = Text.Substring(0, namespaceSeparatorIndex);
                if (alias != "*")
                {
                    @namespace = styleSheet.GetNamespaceUri(alias);
                }
                else
                {

                }

                localName = Text.Substring(namespaceSeparatorIndex + 1);
            }
            else
            {
                @namespace = styleSheet.GetNamespaceUri("");
                localName = Text;
            }

            this.Alias = alias;
            this.LocalName = localName;
            this.NamespaceUri = @namespace;
            this.initializedWith = styleSheet;
        }

        public override MatchResult Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorFragment[] fragments, ref int currentIndex)
        {
            if (initializedWith != styleSheet)
            {
                Initialize(styleSheet);
            }

            var isMatch = domElement.LocalName == LocalName && (Alias == "*" || domElement.NamespaceUri == NamespaceUri);
            return isMatch ? MatchResult.Success : MatchResult.ItemFailed;
        }

        public string Alias { get; private set; }
        public string LocalName { get; private set; }
        public string NamespaceUri { get; private set; }
    }
}
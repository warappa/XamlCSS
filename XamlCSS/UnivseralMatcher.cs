using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class UnivseralMatcher : SelectorMatcher
    {
        public UnivseralMatcher(CssNodeType type, string text) : base(type, text)
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
                    @namespace = styleSheet.GetNamespaceUri(alias, "");
                }
            }
            else
            {
                @namespace = null;
            }

            this.Alias = alias;
            this.NamespaceUri = @namespace;
            this.initializedWith = styleSheet;
            this.styleSheetVersion = styleSheet.Version;
        }

        public override MatchResult Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorMatcher[] fragments, ref int currentIndex)
        {
            if (initializedWith != styleSheet ||
                styleSheetVersion != styleSheet.Version)
            {
                Initialize(styleSheet);
            }

            var isMatch = NamespaceUri == null || domElement.AssemblyQualifiedNamespaceName == NamespaceUri;
            return isMatch ? MatchResult.Success : MatchResult.ItemFailed;
        }

        public string Alias { get; private set; }
        public string NamespaceUri { get; private set; }

        private StyleSheet initializedWith;
        private int styleSheetVersion;
    }
}
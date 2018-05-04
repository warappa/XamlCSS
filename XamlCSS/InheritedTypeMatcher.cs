using System.Reflection;
using XamlCSS.CssParsing;
using XamlCSS.Dom;
using XamlCSS.Utils;

namespace XamlCSS
{
    public class InheritedTypeMatcher : SelectorMatcher
    {
        private StyleSheet initializedWith;
        private int styleSheetVersion;
        private TypeInfo ElementTypeInfo;

        public InheritedTypeMatcher(CssNodeType type, string text) : base(type, text)
        {
            Text = text.Substring(1);
        }

        private void Initialize(StyleSheet styleSheet)
        {
            var namespaceSeparatorIndex = Text.IndexOf('|');
            string @namespace = null;
            //string prefix = null;
            string alias = "";
            string tagName = null;
            if (namespaceSeparatorIndex > -1)
            {
                alias = Text.Substring(0, namespaceSeparatorIndex);
                tagName = Text.Substring(namespaceSeparatorIndex + 1);

                if (alias != "*")
                {
                    @namespace = styleSheet.GetNamespaceUri(alias, tagName);
                }
                else
                {

                }
            }
            else
            {
                tagName = Text;
                @namespace = styleSheet.GetNamespaceUri("", tagName);
            }

            this.Alias = alias;
            this.TagName = tagName;
            this.NamespaceUri = @namespace;
            this.initializedWith = styleSheet;
            this.styleSheetVersion = styleSheet.Version;
            // Hack
            try
            {
                this.ElementTypeInfo = System.Type.GetType(TypeHelpers.ResolveFullTypeName(styleSheet.Namespaces, Text), false)?.GetTypeInfo();
            }
            catch { /* no valid type */ }
        }

        public override MatchResult Match<TDependencyObject, TDependencyProperty>(StyleSheet styleSheet, ref IDomElement<TDependencyObject, TDependencyProperty> domElement, SelectorMatcher[] fragments, ref int currentIndex)
        {
            if (initializedWith != styleSheet ||
                styleSheetVersion != styleSheet.Version)
            {
                Initialize(styleSheet);
            }

            if (ElementTypeInfo == null)
            {
                return MatchResult.ItemFailed;
            }

            var isMatch = ElementTypeInfo.IsAssignableFrom(domElement.Element.GetType().GetTypeInfo());
            return isMatch ? MatchResult.Success : MatchResult.ItemFailed;
        }

        public string Alias { get; private set; }
        public string TagName { get; private set; }
        public string NamespaceUri { get; private set; }
    }
}
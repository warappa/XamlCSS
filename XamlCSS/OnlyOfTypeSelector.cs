using System.Linq;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class OnlyOfTypeSelector : SelectorFragment
    {
        public OnlyOfTypeSelector(CssNodeType type, string text) : base(type, text)
        {
        }

        public override MatchResult Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorFragment[] fragments, ref int currentIndex)
        {
            var tagname = domElement.TagName;
            var namespaceUri = domElement.NamespaceUri;

            return domElement.Parent?.ChildNodes
                .Where(x => x.TagName == tagname && x.NamespaceUri == namespaceUri)
                .Count() == 1 ? MatchResult.Success : MatchResult.ItemFailed;
        }
    }
}
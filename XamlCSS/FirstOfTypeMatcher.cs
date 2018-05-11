using System.Linq;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class FirstOfTypeMatcher : SelectorMatcher
    {
        public FirstOfTypeMatcher(CssNodeType type, string text) : base(type, text)
        {
        }

        public override MatchResult Match<TDependencyObject, TDependencyProperty>(StyleSheet styleSheet, ref IDomElement<TDependencyObject, TDependencyProperty> domElement, SelectorMatcher[] fragments, ref int currentIndex)
        {
            var tagName = domElement.TagName;
            var namespaceUri = domElement.AssemblyQualifiedNamespaceName;

            var children = domElement.LogicalParent?.LogicalChildNodes
                .Where(x => x.TagName == tagName && x.AssemblyQualifiedNamespaceName == namespaceUri)
                .ToList();

            if (children == null)
            {
                return MatchResult.ItemFailed;
            }
            return children.IndexOf(domElement) == 0 ? MatchResult.Success : MatchResult.ItemFailed;
        }
    }
}

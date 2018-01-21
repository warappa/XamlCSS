using System.Linq;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class OnlyOfTypeMatcher : SelectorMatcher
    {
        public OnlyOfTypeMatcher(CssNodeType type, string text) : base(type, text)
        {
        }

        public override MatchResult Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorMatcher[] fragments, ref int currentIndex)
        {
            var tagname = domElement.TagName;
            var namespaceUri = domElement.AssemblyQualifiedNamespaceName;

            return domElement.Parent?.ChildNodes
                .Where(x => x.TagName == tagname && x.AssemblyQualifiedNamespaceName == namespaceUri)
                .Count() == 1 ? MatchResult.Success : MatchResult.ItemFailed;
        }
    }
}
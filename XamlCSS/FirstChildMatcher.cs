using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class FirstChildMatcher : SelectorMatcher
    {
        public FirstChildMatcher(CssNodeType type, string text) : base(type, text)
        {
        }

        public override MatchResult Match<TDependencyObject, TDependencyProperty>(StyleSheet styleSheet, ref IDomElement<TDependencyObject, TDependencyProperty> domElement, SelectorMatcher[] fragments, ref int currentIndex)
        {
            return (domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1) == 0 ? MatchResult.Success : MatchResult.ItemFailed;
        }
    }
}

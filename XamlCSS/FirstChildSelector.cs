using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class FirstChildSelector : SelectorFragment
    {
        public FirstChildSelector(CssNodeType type, string text) : base(type, text)
        {
        }

        public override MatchResult Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorFragment[] fragments, ref int currentIndex)
        {
            return (domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1) == 0 ? MatchResult.Success : MatchResult.ItemFailed;
        }
    }
}
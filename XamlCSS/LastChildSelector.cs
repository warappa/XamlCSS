using System.Linq;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class LastChildSelector : SelectorFragment
    {
        public LastChildSelector(CssNodeType type, string text) : base(type, text)
        {
        }

        public override MatchResult Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorFragment[] fragments, ref int currentIndex)
        {
            return domElement.Parent?.ChildNodes.IndexOf(domElement) == (domElement.Parent?.ChildNodes.Count()) - 1 ? MatchResult.Success : MatchResult.ItemFailed;
        }
    }
}
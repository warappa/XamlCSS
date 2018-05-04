using System.Collections.Generic;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class IdMatcher : SelectorMatcher
    {
        public IdMatcher(CssNodeType type, string text) : base(type, text)
        {
            Text = text?.Substring(1);
        }

        
        public override MatchResult Match<TDependencyObject, TDependencyProperty>(StyleSheet styleSheet, ref IDomElement<TDependencyObject, TDependencyProperty> domElement, SelectorMatcher[] fragments, ref int currentIndex)
        {
            return domElement.Id == Text ? MatchResult.Success : MatchResult.ItemFailed;
        }
    }
}

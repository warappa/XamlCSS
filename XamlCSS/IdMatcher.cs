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

        
        public override MatchResult Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorMatcher[] fragments, ref int currentIndex)
        {
            return domElement.Id == Text ? MatchResult.Success : MatchResult.ItemFailed;
        }
    }
}
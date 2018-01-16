using System.Collections.Generic;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class IdSelector : SelectorFragment
    {
        public IdSelector(CssNodeType type, string text) : base(type, text)
        {
            Text = text?.Substring(1);
        }

        
        public override MatchResult Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorFragment[] fragments, ref int currentIndex)
        {
            return domElement.Id == Text ? MatchResult.Success : MatchResult.ItemFailed;
        }
    }
}
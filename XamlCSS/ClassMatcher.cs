using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class ClassMatcher : SelectorMatcher
    {
        public ClassMatcher(CssNodeType type, string text) : base(type, text)
        {
            Text = text.Substring(1);
        }

        public override MatchResult Match<TDependencyObject, TDependencyProperty>(StyleSheet styleSheet, ref IDomElement<TDependencyObject, TDependencyProperty> domElement, SelectorMatcher[] fragments, ref int currentIndex)
        {
            return domElement.ClassList.Contains(Text) ? MatchResult.Success : MatchResult.ItemFailed;
        }
    }
}

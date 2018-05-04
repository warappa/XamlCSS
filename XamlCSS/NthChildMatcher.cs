using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class NthChildMatcher : NthMatcherBase
    {
        public NthChildMatcher(CssNodeType type, string text) : base(type, text)
        {
        }

        protected override string GetParameterExpression(string expression)
        {
            if (expression?.Length >= 11 == true)
            {
                return expression?.Substring(11).Replace(")", "");
            }

            return null;
        }

        public override MatchResult Match<TDependencyObject, TDependencyProperty>(StyleSheet styleSheet, ref IDomElement<TDependencyObject, TDependencyProperty> domElement, SelectorMatcher[] fragments, ref int currentIndex)
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                return MatchResult.ItemFailed;
            }

            var thisPosition = domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1;
            thisPosition++;

            return CalcIsNth(factor, distance, ref thisPosition) ? MatchResult.Success : MatchResult.ItemFailed;
        }
    }
}

using System.Linq;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class NthLastOfTypeMatcher : NthMatcherBase
    {
        public NthLastOfTypeMatcher(CssNodeType type, string text) : base(type, text)
        {

        }

        protected override string GetParameterExpression(string expression)
        {
            if (expression.Length >= 18)
            {
                return expression.Substring(18).Replace(")", "");
            }

            return null;
        }

        public override MatchResult Match<TDependencyObject, TDependencyProperty>(StyleSheet styleSheet, ref IDomElement<TDependencyObject, TDependencyProperty> domElement, SelectorMatcher[] fragments, ref int currentIndex)
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                return MatchResult.ItemFailed;
            }

            var tagName = domElement.TagName;

            var thisPosition = domElement.LogicalParent?.LogicalChildNodes.Where(x => x.TagName == tagName).IndexOf(domElement) ?? -1;

            thisPosition = (domElement.LogicalParent?.LogicalChildNodes.Where(x => x.TagName == tagName).Count() ?? 0) - thisPosition;

            return CalcIsNth(factor, distance, ref thisPosition) ? MatchResult.Success : MatchResult.ItemFailed;
        }
    }
}

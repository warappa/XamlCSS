using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class NthLastChildMatcher : NthMatcherBase
    {
        public NthLastChildMatcher(CssNodeType type, string text) : base(type, text)
        {
            Text = text?.Substring(11).Replace(")", "");
        }

        protected override string GetParameterExpression(string expression)
        {
            if (expression?.Length >= 16 == true)
            {
                return expression?.Substring(16).Replace(")", "");
            }

            return null;
        }

        public override MatchResult Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorMatcher[] fragments, ref int currentIndex)
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                // TODO: maybe can use GeneralParentFailed
                return MatchResult.ItemFailed;
            }

            var thisPosition = domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1;

            thisPosition = (domElement.Parent?.ChildNodes.Count ?? 0) - thisPosition;

            return CalcIsNth(factor, distance, ref thisPosition) ? MatchResult.Success : MatchResult.ItemFailed;
        }
    }
}

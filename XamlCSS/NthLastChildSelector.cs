using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class NthLastChildSelector : NthSelectorFragmentBase
    {
        public NthLastChildSelector(CssNodeType type, string text) : base(type, text)
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

        public override bool Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorFragment[] fragments, ref int currentIndex)
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                return false;
            }

            var thisPosition = domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1;

            thisPosition = (domElement.Parent?.ChildNodes.Count ?? 0) - thisPosition;

            return CalcIsNth(factor, distance, ref thisPosition);
        }
    }
}
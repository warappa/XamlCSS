using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class NthChildSelector : NthSelectorFragmentBase
    {
        public NthChildSelector(CssNodeType type, string text) : base(type, text)
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

        public override bool Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorFragment[] fragments, ref int currentIndex)
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                return false;
            }

            var thisPosition = domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1;
            thisPosition++;

            return CalcIsNth(factor, distance, ref thisPosition);
        }
    }
}
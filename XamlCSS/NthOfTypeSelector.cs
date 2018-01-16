using System.Linq;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class NthOfTypeSelector : NthSelectorFragmentBase
    {
        public NthOfTypeSelector(CssNodeType type, string text) : base(type, text)
        {
            
        }

        protected override string GetParameterExpression(string expression)
        {
            if (expression?.Length >= 13 == true)
            {
                return expression?.Substring(13).Replace(")", "");
            }

            return null;
        }

        public override MatchResult Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorFragment[] fragments, ref int currentIndex)
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                return MatchResult.ItemFailed;
            }

            var tagname = domElement.TagName;

            var thisPosition = domElement.Parent?.ChildNodes.Where(x => x.TagName == tagname).IndexOf(domElement) ?? -1;
            thisPosition++;

            return CalcIsNth(factor, distance, ref thisPosition) ? MatchResult.Success : MatchResult.ItemFailed;
        }
    }
}
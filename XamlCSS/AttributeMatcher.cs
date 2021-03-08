using System.Collections;
using System.Text.RegularExpressions;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class AttributeMatcher : SelectorMatcher
    {
        private static Regex attributeMatcher = new Regex(@"^\[([a-zA-Z0-9]+)(\|?\~?=)?(.+)?\]$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public string PropertyName { get; protected set; }
        public string Operator { get; protected set; }
        public string Value { get; protected set; }

        public AttributeMatcher(CssNodeType type, string text) : base(type, text)
        {
            var match = attributeMatcher.Match(text);
            if (match.Groups.Count > 1 &&
                match.Groups[1].Success)
            {
                PropertyName = match.Groups[1].Value;
            }
            if (match.Groups.Count > 2 &&
                match.Groups[2].Success)
            {
                Operator = match.Groups[2].Value;
            }
            if (match.Groups.Count > 3 &&
                match.Groups[3].Success)
            {
                Value = match.Groups[3].Value.Trim('"');
            }
        }

        public override MatchResult Match<TDependencyObject, TDependencyProperty>(StyleSheet styleSheet, ref IDomElement<TDependencyObject, TDependencyProperty> domElement, SelectorMatcher[] fragments, ref int currentIndex)
        {
            if (!domElement.HasAttribute(PropertyName))
            {
                return MatchResult.ItemFailed;
            }

            var dependencyProperty = domElement.Attributes[PropertyName];
            domElement.EnsureAttributeWatcher(dependencyProperty);

            // just check if it exists - here: is not null
            if (Value == null)
            {
                return domElement.GetAttributeValue(dependencyProperty) != null ? MatchResult.Success : MatchResult.ItemFailed;
            }

            if (Operator == "=")
            {
                return domElement.GetAttributeValue(dependencyProperty)?.ToString() == Value ? MatchResult.Success : MatchResult.ItemFailed;
            }

            //else if (Operator == "~=")
            //{
            //    var v = domElement.GetAttributeValue(dependencyProperty);
            //    if (v is IEnumerable e)
            //    {
            //        foreach (var item in e)
            //        {
            //            if (item?.ToString() == Value)
            //            {
            //                return MatchResult.Success;
            //            }
            //        }
            //    }

            //    return MatchResult.ItemFailed;
            //}
            //else if (Operator == "|=")
            //{
            //    var v = domElement.GetAttributeValue(dependencyProperty);
            //    if (v is IEnumerable e)
            //    {
            //        foreach (var item in e)
            //        {
            //            if (item?.ToString() == Value)
            //            {
            //                return MatchResult.Success;
            //            }
            //        }
            //    }

            //    return MatchResult.ItemFailed;
            //}

            return MatchResult.ItemFailed;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class SelectorFragment
    {
        public CssNodeType Type { get; protected set; }
        public string Text { get; protected set; }

        private Regex nthRegex;

        public SelectorFragment(CssNodeType type, string text)
        {
            Type = type;
            Text = text;

            nthRegex = new Regex(@"((?<factor>[0-9]+)n)?(?<distance>([\+\-]?[0-9]+))?");
        }

        virtual public bool Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorFragment[] fragments, ref int currentIndex)
        {
            /*if (Type == CssNodeType.IdSelector)
            {
                return domElement.Id == Text.Substring(1);
            }
            else if (Type == CssNodeType.ClassSelector)
            {
                return domElement.ClassList.Contains(Text.Substring(1));
            }
            else*/
            if (Type == CssNodeType.DirectSiblingCombinator)
            {
                var thisIndex = domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1;

                if (thisIndex == 0)
                {
                    return false;
                }

                var sibling = domElement.Parent?.ChildNodes[thisIndex - 1];
                if (sibling == null)
                {
                    return false;
                }
                currentIndex--;

                var result = fragments[currentIndex].Match(styleSheet, ref sibling, fragments, ref currentIndex);
                domElement = sibling;

                return result;
            }

            else if (Type == CssNodeType.GeneralSiblingCombinator)
            {
                var thisIndex = domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1;

                if (thisIndex == 0)
                {
                    return false;
                }

                currentIndex--;

                if ((domElement.Parent?.ChildNodes.Count > 0) == true)
                {
                    foreach (var sibling in domElement.Parent.ChildNodes.Take(thisIndex))
                    {
                        var refSibling = sibling;
                        if (fragments[currentIndex].Match(styleSheet, ref refSibling, fragments, ref currentIndex))
                        {
                            domElement = sibling;
                            return true;
                        }
                    }
                }

                return false;
            }

            else if (Type == CssNodeType.DirectDescendantCombinator)
            {
                var result = domElement.Parent?.ChildNodes.Contains(domElement) == true;
                domElement = domElement.Parent;
                return result;
            }

            else if (Type == CssNodeType.GeneralDescendantCombinator)
            {
                currentIndex--;
                var fragment = fragments[currentIndex];

                var current = domElement.Parent;
                while (current != null)
                {
                    if (fragment.Match(styleSheet, ref current, fragments, ref currentIndex))
                    {
                        domElement = current;
                        return true;
                    }
                    current = current.Parent;
                }
                return false;
            }

            else if (Type == CssNodeType.PseudoSelector)
            {
                if (Text == ":first-child")
                {
                    return (domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1) == 0;
                }
                else if (Text == ":last-child")
                {
                    return domElement.Parent?.ChildNodes.IndexOf(domElement) == (domElement.Parent?.ChildNodes.Count()) - 1;
                }
                else if (Text.StartsWith(":nth-child", StringComparison.Ordinal))
                {
                    var expression = Text.Substring(11).Replace(")", "");
                    int factor, distance;

                    GetFactorAndDistance(expression, out factor, out distance);

                    var thisPosition = domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1;
                    thisPosition++;

                    var isNth = ((factor != 0 ? thisPosition % factor : thisPosition) - (distance >= 0 ? distance : factor + distance)) == 0;

                    return isNth;
                }
                else if (Text.StartsWith(":nth-last-child", StringComparison.Ordinal))
                {
                    var expression = Text.Substring(11).Replace(")", "");
                    int factor, distance;

                    GetFactorAndDistance(expression, out factor, out distance);

                    var thisPosition = domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1;
                    thisPosition++;

                    thisPosition = (domElement.Parent?.ChildNodes.Count ?? 0) - thisPosition;

                    var isNth = ((factor != 0 ? thisPosition % factor : thisPosition) - (distance >= 0 ? distance : factor + distance)) == 0;

                    return isNth;
                }
                else if (Text.StartsWith(":nth-of-type", StringComparison.Ordinal))
                {
                    var expression = Text.Substring(13).Replace(")", "");

                    int factor, distance;

                    GetFactorAndDistance(expression, out factor, out distance);

                    var tagname = domElement.TagName;

                    var thisPosition = domElement.Parent?.ChildNodes.Where(x => x.TagName == tagname).IndexOf(domElement) ?? -1;
                    thisPosition++;

                    var isNth = ((factor != 0 ? thisPosition % factor : thisPosition) - (distance >= 0 ? distance : factor + distance)) == 0;

                    return isNth;
                }
                else if (Text.StartsWith(":nth-last-of-type", StringComparison.Ordinal))
                {
                    var expression = Text.Substring(13).Replace(")", "");

                    int factor, distance;

                    GetFactorAndDistance(expression, out factor, out distance);

                    var tagname = domElement.TagName;

                    var thisPosition = domElement.Parent?.ChildNodes.Where(x => x.TagName == tagname).IndexOf(domElement) ?? -1;
                    thisPosition++;

                    thisPosition = (domElement.Parent?.ChildNodes.Where(x => x.TagName == tagname).Count() ?? 0) - thisPosition;

                    var isNth = ((factor != 0 ? thisPosition % factor : thisPosition) - (distance >= 0 ? distance : factor + distance)) == 0;

                    return isNth;
                }

                return false;
            }

            return false;
        }

        private void GetFactorAndDistance(string expression, out int factor, out int distance)
        {
            factor = 0;
            distance = 0;
            if (expression == "even")
            {
                factor = 2;
            }
            else if (expression == "odd")
            {
                factor = 2;
                distance = 1;
            }
            else
            {
                var matchResult = nthRegex.Match(expression);

                int.TryParse(matchResult.Groups["factor"]?.Value ?? "", out factor);
                int.TryParse(matchResult.Groups["distance"]?.Value ?? "", out distance);

                if (factor == 0 &&
                    matchResult.Groups["n"] != null)
                {
                    factor = 1;
                }
            }

            if (factor != 0)
            {
                distance = distance % factor;
            }
        }
    }
}
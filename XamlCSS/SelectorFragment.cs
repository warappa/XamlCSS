using System;
using System.Linq;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class SelectorFragment
    {
        public CssNodeType Type { get; protected set; }
        public string Text { get; protected set; }

        public SelectorFragment(CssNodeType type, string text)
        {
            Type = type;
            Text = text;
        }

        virtual public bool Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorFragment[] fragments, ref int currentIndex)
        {
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
                else if (Text.StartsWith(":only-of-type", StringComparison.Ordinal))
                {
                    var tagname = domElement.TagName;

                    return domElement.Parent?.ChildNodes.Where(x => x.TagName == tagname).Count() == 1;
                }

                return false;
            }

            return false;
        }
    }
}
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

        virtual public MatchResult Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorFragment[] fragments, ref int currentIndex)
        {
            if (Type == CssNodeType.GeneralDescendantCombinator)
            {
                currentIndex--;
                var fragment = fragments[currentIndex];

                var current = domElement.Parent;
                while (current != null)
                {
                    if (fragment.Match(styleSheet, ref current, fragments, ref currentIndex).IsSuccess)
                    {
                        domElement = current;
                        return MatchResult.Success;
                    }
                    current = current.Parent;
                }
                return MatchResult.GeneralParentFailed;
            }

            else if (Type == CssNodeType.DirectDescendantCombinator)
            {
                var result = domElement.Parent?.ChildNodes.Contains(domElement) == true;
                domElement = domElement.Parent;
                return result ? MatchResult.Success : MatchResult.DirectParentFailed;
            }

            else if (Type == CssNodeType.GeneralSiblingCombinator)
            {
                var thisIndex = domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1;

                if (thisIndex == 0)
                {
                    return MatchResult.ItemFailed;
                }

                currentIndex--;

                if ((domElement.Parent?.ChildNodes.Count > 0) == true)
                {
                    foreach (var sibling in domElement.Parent.ChildNodes.Take(thisIndex))
                    {
                        var refSibling = sibling;
                        if (fragments[currentIndex].Match(styleSheet, ref refSibling, fragments, ref currentIndex).IsSuccess)
                        {
                            domElement = sibling;
                            return MatchResult.Success;
                        }
                    }
                }

                return MatchResult.ItemFailed;
            }

            else if (Type == CssNodeType.DirectSiblingCombinator)
            {
                var thisIndex = domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1;

                if (thisIndex <= 0)
                {
                    return MatchResult.ItemFailed;
                }

                var sibling = domElement.Parent?.ChildNodes[thisIndex - 1];
                if (sibling == null)
                {
                    return MatchResult.ItemFailed;
                }
                currentIndex--;

                var result = fragments[currentIndex].Match(styleSheet, ref sibling, fragments, ref currentIndex);
                domElement = sibling;

                return result;
            }

            return MatchResult.ItemFailed;
        }
    }
}

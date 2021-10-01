using System.Linq;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class SelectorMatcher
    {
        public CssNodeType Type { get; protected set; }
        public string Text { get; protected set; }
        public bool IsVisualTree { get; }

        public SelectorMatcher(CssNodeType type, string text)
        {
            Type = type;
            Text = text;
            IsVisualTree = Text == ":visualtree";
        }

        public virtual MatchResult Match<TDependencyObject, TDependencyProperty>(StyleSheet styleSheet, ref IDomElement<TDependencyObject, TDependencyProperty> domElement, SelectorMatcher[] fragments, ref int currentIndex)
        {
            if (Type == CssNodeType.GeneralDescendantCombinator)
            {
                currentIndex--;
                var savedIndex = currentIndex;

                var fragment = fragments[currentIndex];
                var current = fragment.Type == CssNodeType.PseudoSelector && fragment.IsVisualTree ? domElement.Parent : domElement.LogicalParent;
                while (current != null)
                {
                    MatchResult res = null;
                    while (currentIndex >= 0)
                    {
                        fragment = fragments[currentIndex];

                        res = fragment.Match(styleSheet, ref current, fragments, ref currentIndex);
                        if (!res.IsSuccess)
                        {
                            break;
                        }

                        currentIndex--;
                    }

                    currentIndex++;

                    if (res.IsSuccess)
                    {
                        domElement = current;
                        return MatchResult.Success;
                    }

                    currentIndex = savedIndex;
                    fragment = fragments[currentIndex];

                    current = fragment.Type == CssNodeType.PseudoSelector && fragment.IsVisualTree ? current.Parent : current.LogicalParent;
                }

                return MatchResult.GeneralParentFailed;
            }

            else if (Type == CssNodeType.DirectDescendantCombinator)
            {
                var result = domElement.LogicalParent?.LogicalChildNodes.Contains(domElement) == true;
                domElement = domElement.LogicalParent;
                return result ? MatchResult.Success : MatchResult.DirectParentFailed;
            }

            else if (Type == CssNodeType.GeneralSiblingCombinator)
            {
                var thisIndex = domElement.LogicalParent?.LogicalChildNodes.IndexOf(domElement) ?? -1;

                if (thisIndex == 0)
                {
                    return MatchResult.ItemFailed;
                }

                currentIndex--;

                if ((domElement.LogicalParent?.LogicalChildNodes.Count > 0) == true)
                {
                    foreach (var sibling in domElement.LogicalParent.LogicalChildNodes.Take(thisIndex))
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
                var thisIndex = domElement.LogicalParent?.LogicalChildNodes.IndexOf(domElement) ?? -1;

                if (thisIndex <= 0)
                {
                    return MatchResult.ItemFailed;
                }

                var sibling = domElement.LogicalParent?.LogicalChildNodes[thisIndex - 1];
                if (sibling == null)
                {
                    return MatchResult.ItemFailed;
                }
                currentIndex--;

                var result = fragments[currentIndex].Match(styleSheet, ref sibling, fragments, ref currentIndex);
                domElement = sibling;

                return result;
            }

            else if (Type == CssNodeType.PseudoSelector)
            {
                if (IsVisualTree)
                {
                    if (domElement.IsInVisualTree == false)
                    {
                        return MatchResult.ItemFailed;
                    }

                    return MatchResult.Success;
                }
            }

            return MatchResult.ItemFailed;
        }

        private static MatchResult GeneralVisualDescendantCombinator<TDependencyObject, TDependencyProperty>(StyleSheet styleSheet, ref IDomElement<TDependencyObject, TDependencyProperty> domElement, SelectorMatcher[] fragments, ref int currentIndex)
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using XamlCSS.Utils;

namespace XamlCSS.Dom
{
    public static class LinqExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> list, T element)
        {
            var counter = 0;
            foreach (var item in list)
            {
                if (item?.Equals(element) == true)
                {
                    return counter;
                }

                counter++;
            }

            return -1;
        }

        public static bool Isi(this string current, string other)
        {
            return string.Equals(current, other, StringComparison.OrdinalIgnoreCase);
        }

        public static bool Is(this string current, string other)
        {
            return string.Equals(current, other, StringComparison.Ordinal);
        }

        public static bool Contains(this IList<string> list, string[] tokens)
        {
            var length = tokens.Length;
            for (int i = 0; i < length; i++)
            {
                if (!list.Contains(tokens[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ContainsAll<T>(this IEnumerable<T> target, IEnumerable<T> other)
        {
            return target.Intersect(other).Count() == other.Count();
        }

        public static void GetElementsByClassName<TDependencyObject, TDependencyProperty>(this IList<IDomElement<TDependencyObject, TDependencyProperty>> elements, string[] classNames, IList<IDomElement<TDependencyObject, TDependencyProperty>> result)
            where TDependencyObject : class
        {
            var length = elements.Count;
            for (int i = 0; i < length; i++)
            {
                var element = elements[i];
                if (element.ClassList.ContainsAll(classNames))
                {
                    result.Add(element);
                }

                if (element.LogicalChildNodes.Count != 0)
                {
                    element.LogicalChildNodes.GetElementsByClassName(classNames, result);
                }
            }
        }

        public static void GetElementsByTagName<TDependencyObject, TDependencyProperty>(this IList<IDomElement<TDependencyObject, TDependencyProperty>> elements, string namespaceUri, string tagName, IList<IDomElement<TDependencyObject, TDependencyProperty>> result)
            where TDependencyObject : class
        {
            var length = elements.Count;

            for (int i = 0; i < length; i++)
            {
                var element = elements[i];
                if (element.AssemblyQualifiedNamespaceName.Is(namespaceUri) && (tagName == null || tagName.Isi(element.TagName)))
                {
                    result.Add(element);
                }

                if (element.LogicalChildNodes.Count != 0)
                {
                    element.LogicalChildNodes.GetElementsByTagName(namespaceUri, tagName, result);
                }
            }
        }
        // used but old
        public static IList<IDomElement<TDependencyObject, TDependencyProperty>> QuerySelectorAll<TDependencyObject, TDependencyProperty>(this IList<IDomElement<TDependencyObject, TDependencyProperty>> elements, StyleSheet styleSheet, ISelector selector, SelectorType type, int endGroupIndex)
            where TDependencyObject : class
        {
            var list = new List<IDomElement<TDependencyObject, TDependencyProperty>>(50);

            elements.QuerySelectorAll(styleSheet, selector, list, type, endGroupIndex);

            return list;
        }

        public static int MoveUpToNextMatch<TDependencyObject, TDependencyProperty>(
            IDomElement<TDependencyObject, TDependencyProperty> baseElement,
            IList<Selector> generalParentGroups,
            IDomElement<TDependencyObject, TDependencyProperty> limitElement,
            int groupIndex,
            StyleSheet styleSheet)
            where TDependencyObject : class
        {
            var currentGroup = generalParentGroups[groupIndex];

            var onVisualTree = currentGroup.StartOnVisualTree();
            var current = onVisualTree ? baseElement.Parent : baseElement.LogicalParent;

            var bestResult = -1;

            while (current != null &&
                current != limitElement)
            {
                var match = currentGroup.Match(styleSheet, current, -1, 0);

                if (match.IsSuccess)
                {
                    bestResult = groupIndex;
                    // last parent group
                    if (groupIndex == generalParentGroups.Count - 1)
                    {
                        return groupIndex;
                    }

                    var nextResult = MoveUpToNextMatch(baseElement, generalParentGroups, current, groupIndex + 1, styleSheet);
                    if (nextResult > bestResult)
                    {
                        return nextResult;
                    }
                }

                current = onVisualTree ? current.Parent : current.LogicalParent;
            }

            return bestResult;
        }

        //public static void QuerySelectorAllNew<TDependencyObject, TDependencyProperty>(this IList<IDomElement<TDependencyObject, TDependencyProperty>> elements, StyleSheet styleSheet, ISelector selector, IList<IDomElement<TDependencyObject, TDependencyProperty>> result, SelectorType type)
        //    where TDependencyObject : class
        //{
        //    if (elements.Count == 0)
        //    {
        //        return;
        //    }

        //    var startGroupIndex = 0;
        //    var fragments = (selector as Selector).selectorMatchers;

        //    var groups = new List<Selector>();
        //    var currentGroup = new List<SelectorMatcher>();
        //    for (var i = 0; i < fragments.Length; i++)
        //    {
        //        var fragment = fragments[i];
        //        if (fragment.Type == CssParsing.CssNodeType.GeneralDescendantCombinator)
        //        {
        //            groups.Add(new Selector(currentGroup));

        //            currentGroup = new List<SelectorMatcher>();
        //        }
        //        else
        //        {
        //            currentGroup.Add(fragment);
        //        }
        //    }

        //    if (currentGroup.Any())
        //    {
        //        groups.Add(new Selector(currentGroup));
        //    }

        //    if (groups.Count > 1)
        //    {
        //        var element = elements[0];

        //        //IDomElement<TDependencyObject, TDependencyProperty> lastFound = null;

        //        // test general parents up
        //        //for (var i = 0; i < groups.Count - 1; i++)
        //        //{
        //        //    var group = groups[i];

        //        //    var groupSelector = new Selector(group);
        //        //    var lastFragment = group.Last();

        //        //    var isVisualTree = MatchesOnVisualTree(lastFragment);

        //        //    var current = isVisualTree ? element.Parent : element.LogicalParent;
        //        //    while (current != null &&
        //        //        current != lastFound)
        //        //    {
        //        //        var match = groupSelector.Match(styleSheet, current);

        //        //        if (match.IsSuccess)
        //        //        {
        //        //            startGroupIndex = i + 1;
        //        //            lastFound = current;
        //        //            break;
        //        //        }

        //        //        current = isVisualTree ? current.Parent : current.LogicalParent;
        //        //    }

        //        //    // this group didn't match
        //        //    if (startGroupIndex != i + 1)
        //        //    {
        //        //        break;
        //        //    }
        //        //}

        //        var bestMatch = MoveUpToNextMatch(element, groups.Take(groups.Count - 1).ToList(), null, 0, styleSheet);
        //        startGroupIndex = bestMatch + 1;

        //    }
        //    var reducedGroups = groups.Skip(startGroupIndex).ToList();
        //    //var useVisualChildElements = MatchesOnVisualTree(reducedGroups[0].Last());
        //    //var children = useVisualChildElements ? element.ChildNodes : element.LogicalChildNodes;
        //    // test general parents down

        //    if (reducedGroups.Count == 0)
        //    {

        //    }
        //    if (reducedGroups.Count > 1)
        //    {
        //        TestGeneralParentsDown(elements, reducedGroups, result, styleSheet);
        //    }
        //    else
        //    {
        //        TestEveryNode(reducedGroups[0], elements, result, styleSheet);
        //    }

        //}

        private static bool MatchesOnVisualTree(SelectorMatcher lastFragment)
        {
            return lastFragment.Type == CssParsing.CssNodeType.PseudoSelector &&
                                    lastFragment.Text == ":visualtree";
        }

        //private static void TestGeneralParentsDown<TDependencyObject, TDependencyProperty>(
        //    IList<IDomElement<TDependencyObject, TDependencyProperty>> domElements,
        //    IList<Selector> groups,
        //    IList<IDomElement<TDependencyObject, TDependencyProperty>> result,
        //    StyleSheet styleSheet)
        //    where TDependencyObject : class
        //{
        //    {
        //        var group = groups[0];

        //        var isVisualTree = group.StartOnVisualTree();

        //        foreach (var element in domElements)
        //        {
        //            var match = group.Match(styleSheet, element);
        //            if (match.IsSuccess)
        //            {
        //                if (groups.Count == 2)
        //                {
        //                    // found leaf
        //                    var lastGroup = groups[1];
        //                    var onVisualTree = lastGroup.StartOnVisualTree();
        //                    TestEveryNode(lastGroup, onVisualTree ? element.ChildNodes : element.LogicalChildNodes, result, styleSheet);
        //                }
        //                else
        //                {
        //                    // test with next general parent group
        //                    var newGroups = groups.Skip(1).ToList();
        //                    var onVisualTree = newGroups[0].StartOnVisualTree();
        //                    TestGeneralParentsDown(onVisualTree ? element.ChildNodes : element.LogicalChildNodes, newGroups, result, styleSheet);
        //                }
        //            }
        //            else
        //            {
        //                // test on with this group
        //                TestGeneralParentsDown(isVisualTree ? element.ChildNodes : element.LogicalChildNodes, groups, result, styleSheet);
        //            }
        //        }
        //    }
        //}

        //private static void TestEveryNode<TDependencyObject, TDependencyProperty>(
        //    ISelector selector,
        //    IList<IDomElement<TDependencyObject, TDependencyProperty>> elements,
        //    IList<IDomElement<TDependencyObject, TDependencyProperty>> result,
        //    StyleSheet styleSheet)
        //    where TDependencyObject : class
        //{
        //    var skipThisLevel = false;
        //    var length = elements.Count;

        //    var type = selector.StartOnVisualTree() ? SelectorType.VisualTree : SelectorType.LogicalTree;

        //    for (int i = 0; i < length; i++)
        //    {
        //        var element = elements[i];
        //        var inTree = true;

        //        if (type == SelectorType.LogicalTree &&
        //            !element.IsInLogicalTree)
        //        {
        //            inTree = false;
        //        }
        //        else if (type == SelectorType.VisualTree &&
        //            !element.IsInVisualTree)
        //        {
        //            inTree = false;
        //        }
        //        else if (!element.IsReady)
        //        {
        //            inTree = false;
        //        }

        //        if (!skipThisLevel &&
        //            inTree)
        //        {
        //            var shouldNotProcess =
        //                !object.ReferenceEquals(element.StyleInfo != null ? element.StyleInfo.CurrentStyleSheet : null, styleSheet) ||
        //                (element.StyleInfo.DoMatchCheck & type) != type;

        //            if (shouldNotProcess)
        //            {
        //                continue;
        //            }

        //            var match = selector.Match(styleSheet, element);

        //            if (match.IsSuccess)
        //            {
        //                result.Add(element);
        //            }
        //            else if (match.HasDirectParentFailed)
        //            {
        //                skipThisLevel = true;
        //            }
        //        }

        //        if (inTree)
        //        {
        //            var children = type == SelectorType.LogicalTree ? element.LogicalChildNodes : element.ChildNodes;
        //            if (children.Count != 0)
        //            {
        //                TestEveryNode(selector, children, result, styleSheet);
        //            }
        //        }
        //    }
        //}

        // used but old
        public static void QuerySelectorAll<TDependencyObject, TDependencyProperty>(this IList<IDomElement<TDependencyObject, TDependencyProperty>> elements, StyleSheet styleSheet, ISelector selector, IList<IDomElement<TDependencyObject, TDependencyProperty>> result, SelectorType type, int endGroupIndex)
            where TDependencyObject : class
        {
            var length = elements.Count;
            var skipThisLevel = false;

            for (int i = 0; i < length; i++)
            {
                var element = elements[i];
                var inTree = true;
                var triedReduce = false;

                if (!element.IsReady)
                {
                    inTree = false;
                }
                else if (type == SelectorType.LogicalTree &&
                    element.IsInLogicalTree != true)
                {
                    inTree = false;
                }
                else if (type == SelectorType.VisualTree &&
                    element.IsInVisualTree != true)
                {
                    inTree = false;
                }

                if (!skipThisLevel &&
                    inTree)
                {
                    var shouldNotProcess =
                        !object.ReferenceEquals(element.StyleInfo != null ? element.StyleInfo.CurrentStyleSheet : null, styleSheet) ||
                        (element.StyleInfo.DoMatchCheck & type) != type;

                    if (shouldNotProcess)
                    {
                        continue;
                    }

                    var match = selector.Match(styleSheet, element, -1, endGroupIndex);

                    if (match.IsSuccess)
                    {
                        result.Add(element);
                        if (!triedReduce &&
                            match.Group > 1)
                        {
                            endGroupIndex = selector.GroupCount - 1;
                            triedReduce = true;
                        }
                    }
                    else if (match.HasGeneralParentFailed)
                    {
                        skipThisLevel = true;
                        if (!triedReduce &&
                            match.Group > 1)
                        {
                            endGroupIndex = match.Group - 1;
                            triedReduce = true;
                        }
                    }
                    else if (match.HasDirectParentFailed)
                    {
                        skipThisLevel = true;
                    }
                }

                if (inTree)
                {
                    var children = type == SelectorType.LogicalTree ? element.LogicalChildNodes : element.ChildNodes;
                    if (children.Count != 0)
                    {
                        children.QuerySelectorAll(styleSheet, selector, result, type, endGroupIndex);
                    }
                }
            }
        }

        private static ISelector GetReducedSelector(MatchResult matchResult, ISelector selector)
        {
            return selector;
            //var s = (Selector)selector;
            //if (!s.HasGeneralDescendantCombinator)
            //{
            //    return selector;
            //}

            //var selectorMatchers = matchResult.SelectorMatchers;

            //var newFragments = new List<SelectorMatcher>();

            //var successfulSelectorMatchers = selectorMatchers.Take(matchResult.CurrentIndex);

            //for (var i = successfulSelectorMatchers.Count() - 1; i >= 0; i--)
            //{
            //    var fragment = successfulSelectorMatchers.ElementAt(i);
            //    if (fragment.Type == CssParsing.CssNodeType.GeneralDescendantCombinator)
            //    {
            //        break;
            //    }
            //    newFragments.Add(fragment);
            //}

            //if (newFragments.Count == s.selectorMatchers.Count())
            //{
            //    return selector;
            //}

            //return new Selector(newFragments);
        }
    }
}

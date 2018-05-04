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

        public static void GetElementsByClassName<TDependencyObject>(this IList<IDomElement<TDependencyObject>> elements, string[] classNames, IList<IDomElement<TDependencyObject>> result)
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

        public static void GetElementsByTagName<TDependencyObject>(this IList<IDomElement<TDependencyObject>> elements, string namespaceUri, string tagName, IList<IDomElement<TDependencyObject>> result)
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

        public static IList<IDomElement<TDependencyObject>> QuerySelectorAll<TDependencyObject>(this IList<IDomElement<TDependencyObject>> elements, StyleSheet styleSheet, ISelector selector, SelectorType type)
            where TDependencyObject : class
        {
            var list = new List<IDomElement<TDependencyObject>>(50);

            elements.QuerySelectorAll(styleSheet, selector, list, type);

            return list;
        }

        public static void QuerySelectorAll<TDependencyObject>(this IList<IDomElement<TDependencyObject>> elements, StyleSheet styleSheet, ISelector selector, IList<IDomElement<TDependencyObject>> result, SelectorType type)
            where TDependencyObject : class
        {
            var length = elements.Count;
            var skipThisLevel = false;

            for (int i = 0; i < length; i++)
            {
                var element = elements[i];
                var inTree = true;

                if (type == SelectorType.LogicalTree &&
                        !element.IsInLogicalTree)
                {
                    inTree = false;
                }
                else if (type == SelectorType.VisualTree &&
                    !element.IsInVisualTree)
                {
                    inTree = false;
                }
                else if (!element.IsReady)
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

                    var match = selector.Match(styleSheet, element);

                    if (match.IsSuccess)
                    {
                        result.Add(element);
                    }
                    else if (match.HasGeneralParentFailed)
                    {
                        skipThisLevel = true;
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
                        children.QuerySelectorAll(styleSheet, selector, result, type);
                    }
                }
            }
        }

        public static IDomElement<TDependencyObject> QuerySelector<TDependencyObject>(this IList<IDomElement<TDependencyObject>> elements, StyleSheet styleSheet, ISelector selector, SelectorType type)
            where TDependencyObject : class
        {
            var length = elements.Count;
            var skipThisLevel = false;

            for (int i = 0; i < length; i++)
            {
                var element = elements[i];
                if (!skipThisLevel)
                {
                    var match = selector.Match(styleSheet, element);
                    if (match.IsSuccess)
                    {
                        return element;
                    }
                    else if (match.HasGeneralParentFailed)
                    {
                        //return null;
                        skipThisLevel = true;
                    }
                    else if (match.HasDirectParentFailed)
                    {
                        skipThisLevel = true;
                    }
                }

                var children = (type == SelectorType.LogicalTree ? element.LogicalChildNodes : element.ChildNodes);
                if (children.Count != 0)
                {
                    element = children.QuerySelector(styleSheet, selector, type);

                    if (element != null)
                    {
                        return element;
                    }
                }
            }

            return null;
        }

        public static T QuerySelector<T, TDependencyObject>(this IList<IDomElement<TDependencyObject>> elements, StyleSheet styleSheet, ISelector selectors, SelectorType type)
            where TDependencyObject : class
            where T : class, IDomElement<TDependencyObject>
        {
            return elements.QuerySelector(styleSheet, selectors, type) as T;
        }
    }
}

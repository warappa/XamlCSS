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
                if (element != null)
                {
                    if (element.ClassList.ContainsAll(classNames))
                    {
                        result.Add(element);
                    }

                    if (element.ChildNodes.Count != 0)
                    {
                        element.ChildNodes.GetElementsByClassName(classNames, result);
                    }
                }
            }
        }

        public static void GetElementsByTagName<TDependencyObject>(this IList<IDomElement<TDependencyObject>> elements, string tagName, IList<IDomElement<TDependencyObject>> result)
            where TDependencyObject : class
        {
            var length = elements.Count;

            for (int i = 0; i < length; i++)
            {
                var element = elements[i];
                if (element != null)
                {
                    if (tagName == null || tagName.Isi(element.TagName))
                    {
                        result.Add(element);
                    }

                    if (element.ChildNodes.Count != 0)
                    {
                        element.ChildNodes.GetElementsByTagName(tagName, result);
                    }
                }
            }
        }

        public static void GetElementsByTagName<TDependencyObject>(this IList<IDomElement<TDependencyObject>> elements, string namespaceUri, string localName, IList<IDomElement<TDependencyObject>> result)
            where TDependencyObject : class
        {
            var length = elements.Count;

            for (int i = 0; i < length; i++)
            {
                var element = elements[i];
                if (element != null)
                {
                    if (element.NamespaceUri.Is(namespaceUri) && (localName == null || localName.Isi(element.LocalName)))
                    {
                        result.Add(element);
                    }

                    if (element.ChildNodes.Count != 0)
                    {
                        element.ChildNodes.GetElementsByTagName(namespaceUri, localName, result);
                    }
                }
            }
        }

        public static IList<IDomElement<TDependencyObject>> QuerySelectorAll<TDependencyObject>(this IList<IDomElement<TDependencyObject>> elements, StyleSheet styleSheet, ISelector selector)
            where TDependencyObject : class
        {
            var list = new List<IDomElement<TDependencyObject>>();

            elements.QuerySelectorAll(styleSheet, selector, list);

            return list;
        }

        public static void QuerySelectorAll<TDependencyObject>(this IList<IDomElement<TDependencyObject>> elements, StyleSheet styleSheet, ISelector selector, IList<IDomElement<TDependencyObject>> result)
            where TDependencyObject : class
        {
            var length = elements.Count;

            for (int i = 0; i < length; i++)
            {
                var element = elements[i];
                if (element != null)
                {
                    if (element.StyleInfo?.CurrentStyleSheet != styleSheet)
                    {
                        continue;
                    }

                    if (selector.Match(styleSheet, element))
                    {
                        var path = element.GetPath();
                        result.Add(element);
                    }

                    if (element.ChildNodes.Count != 0)
                    {
                        element.ChildNodes.QuerySelectorAll(styleSheet, selector, result);
                    }
                }
            }
        }

        public static IDomElement<TDependencyObject> QuerySelector<TDependencyObject>(this IList<IDomElement<TDependencyObject>> elements, StyleSheet styleSheet, ISelector selector)
            where TDependencyObject : class
        {
            var length = elements.Count;

            for (int i = 0; i < length; i++)
            {
                var element = elements[i];
                if (element != null)
                {
                    if (selector.Match(styleSheet, element))
                    {
                        return element;
                    }

                    if (element.ChildNodes.Count != 0)
                    {
                        element = element.ChildNodes.QuerySelector(styleSheet, selector);

                        if (element != null)
                        {
                            return element;
                        }
                    }
                }
            }

            return null;
        }

        public static T QuerySelector<T, TDependencyObject>(this IList<IDomElement<TDependencyObject>> elements, StyleSheet styleSheet, ISelector selectors)
            where TDependencyObject : class
            where T : class, IDomElement<TDependencyObject>
        {
            return elements.QuerySelector(styleSheet, selectors) as T;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Dom.Css;
using AngleSharp.Parser.Css;

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

        public static bool Contains(this ITokenList list, string[] tokens)
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                if (!list.Contains(tokens[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static void GetElementsByClassName(this INodeList elements, string[] classNames, List<IElement> result)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                var element = elements[i] as IElement;
                if (element != null)
                {
                    if (element.ClassList.Contains(classNames))
                    {
                        result.Add(element);
                    }

                    if (element.ChildElementCount != 0)
                    {
                        element.ChildNodes.GetElementsByClassName(classNames, result);
                    }
                }
            }
        }
        public static void GetElementsByTagName(this INodeList elements, string tagName, List<IElement> result)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                var element = elements[i] as IElement;
                if (element != null)
                {
                    if (tagName == null || tagName.Isi(element.LocalName))
                    {
                        result.Add(element);
                    }

                    if (element.ChildElementCount != 0)
                    {
                        element.ChildNodes.GetElementsByTagName(tagName, result);
                    }
                }
            }
        }
        public static void GetElementsByTagName(this INodeList elements, string namespaceUri, string localName, List<IElement> result)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                var element = elements[i] as IElement;
                if (element != null)
                {
                    if (element.NamespaceUri.Is(namespaceUri) && (localName == null || localName.Isi(element.LocalName)))
                    {
                        result.Add(element);
                    }

                    if (element.ChildElementCount != 0)
                    {
                        element.ChildNodes.GetElementsByTagName(namespaceUri, localName, result);
                    }
                }
            }
        }
        public static IList<IElement> QuerySelectorAll(this INodeList elements, ISelector selector)
        {
            List<IElement> list = new List<IElement>();

            elements.QuerySelectorAll(selector, list);

            return list;
        }
        public static void QuerySelectorAll(this INodeList elements, ISelector selector, List<IElement> result)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                var element = elements[i] as IElement;
                if (element != null)
                {
                    if (selector.Match(element))
                    {
                        result.Add(element);
                    }

                    if (element.HasChildNodes)
                    {
                        element.ChildNodes.QuerySelectorAll(selector, result);
                    }
                }
            }
        }
        public static IElement QuerySelector(this INodeList elements, ISelector selector)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                var element = elements[i] as IElement;
                if (element != null)
                {
                    if (selector.Match(element))
                    {
                        return element;
                    }

                    if (element.HasChildNodes)
                    {
                        element = element.ChildNodes.QuerySelector(selector);

                        if (element != null)
                        {
                            return element;
                        }
                    }
                }
            }

            return null;
        }
        public static T QuerySelector<T>(this INodeList elements, ISelector selectors) where T : class, IElement
        {
            return elements.QuerySelector(selectors) as T;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using XamlCSS.Dom;

namespace XamlCSS.Utils
{
    public static class HierarchyDebugExtensions
    {
        public static string GetPath<TDependencyObject>(IDomElement<TDependencyObject> matchingNode)
            where TDependencyObject : class
        {
            var sb = new List<string>();
            var current = matchingNode;
            while (current != null)
            {
                sb.Add(current.Element.GetType().Name + (!string.IsNullOrWhiteSpace(current.Id) ? "#" + current.Id : ""));
                current = (IDomElement<TDependencyObject>)current.Parent;
            }
            sb.Reverse();
            return string.Join("->", sb);
        }

        public static void PrintHerarchyDebugInfo<TDependencyObject, TUIElement, TStyle, TDependencyProperty>(
            this ITreeNodeProvider<TDependencyObject> treeNodeProvider,
            IDependencyPropertyService<TDependencyObject, TUIElement, TStyle, TDependencyProperty> dependencyPropertyService,
            TUIElement styleResourceReferenceHolder, 
            TUIElement startFrom
            )
            where TDependencyObject : class
            where TUIElement : class, TDependencyObject
            where TStyle : class
            where TDependencyProperty : class
        {
            Debug.WriteLine("");
            Debug.WriteLine("------------------");
            Debug.WriteLine("Print FrameworkElement hierarchy:");
            Debug.WriteLine("----------------");
            Debug.WriteLine("----------------");

            var s = startFrom ?? styleResourceReferenceHolder;
            Recursive(treeNodeProvider, dependencyPropertyService, s, 0, treeNodeProvider.GetParent(s));

            Debug.WriteLine("");
            Debug.WriteLine("Print DomElement hierarchy:");
            Debug.WriteLine("----------------");

            var sDom = treeNodeProvider.GetDomElement(s);
            RecursiveDom(treeNodeProvider, sDom, 0, (IDomElement<TDependencyObject>)sDom.Parent);

            Debug.WriteLine("----------------");
            Debug.WriteLine("----------------");
        }

        public static void Recursive<TDependencyObject, TUIElement, TStyle, TDependencyProperty>(
            this ITreeNodeProvider<TDependencyObject> treeNodeProvider,
            IDependencyPropertyService<TDependencyObject, TUIElement, TStyle, TDependencyProperty> dependencyPropertyService,
            TDependencyObject element,
            int level,
            TDependencyObject expectedParent
            )
            where TDependencyObject : class
            where TUIElement : class, TDependencyObject
            where TStyle : class
            where TDependencyProperty : class
        {
            if (element == null)
            {
                return;
            }

            if (expectedParent != treeNodeProvider.GetParent(element))
            {
                Debug.WriteLine("!!!!!");
                Debug.WriteLine($"Expected parent: { dependencyPropertyService.GetName(expectedParent) }");
                Debug.WriteLine($"Actual parent:   { dependencyPropertyService.GetName(treeNodeProvider.GetParent(element)) }");
                Debug.WriteLine("!!!!!");
            }

            Debug.WriteLine(new String(' ', level) + element.GetType().Name + "#" + dependencyPropertyService.GetName(element));
            var children = treeNodeProvider.GetChildren(element);
            foreach (var child in children)
            {
                Recursive(treeNodeProvider, dependencyPropertyService, child, level + 1, element);
            }
        }

        public static void RecursiveDom<TDependencyObject>(
            this ITreeNodeProvider<TDependencyObject> treeNodeProvider,
            IDomElement<TDependencyObject> domElement, int level, IDomElement<TDependencyObject> expectedParent)
            where TDependencyObject : class
        {
            if (domElement == null)
            {
                return;
            }

            if (expectedParent != domElement.Parent)
            {
                Debug.WriteLine("!!!!!");
                Debug.WriteLine($"Expected parent: { expectedParent.TagName + "#" + expectedParent.Id }");
                Debug.WriteLine($"Actual parent:   { domElement.Parent?.TagName + "#" + domElement.Parent?.Id }");
                Debug.WriteLine("!!!!!");
            }

            Debug.WriteLine(new String(' ', level) + domElement.Element.GetType().Name + "#" + domElement.Id);

            var children = treeNodeProvider.GetDomElementChildren(domElement);
            foreach (var child in children)
            {
                RecursiveDom(treeNodeProvider, child, level + 1, domElement);
            }
        }
    }
}

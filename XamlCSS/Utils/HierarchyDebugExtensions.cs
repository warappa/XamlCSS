using System;
using System.Collections.Generic;
using System.Diagnostics;
using XamlCSS.Dom;

namespace XamlCSS.Utils
{
    public static class HierarchyDebugExtensions
    {
        public static string GetPath<TDependencyObject>(this IDomElement<TDependencyObject> matchingNode)
            where TDependencyObject : class
        {
            var sb = new List<string>();
            var current = matchingNode;
            while (current != null)
            {
                sb.Add(current.Element.GetType().Name + (!string.IsNullOrWhiteSpace(current.Id) ? "#" + current.Id : ""));
                current = current.Parent;
            }
            sb.Reverse();
            return string.Join("->", sb);
        }

        public static void PrintHerarchyDebugInfo<TDependencyObject, TStyle, TDependencyProperty>(
            this ITreeNodeProvider<TDependencyObject> treeNodeProvider,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            TDependencyObject styleResourceReferenceHolder,
            TDependencyObject startFrom
            )
            where TDependencyObject : class
            where TStyle : class
            where TDependencyProperty : class
        {
            var type = SelectorType.LogicalTree;

            Debug.WriteLine("");
            Debug.WriteLine("------------------");
            Debug.WriteLine("Print FrameworkElement hierarchy:");
            Debug.WriteLine("----------------");
            Debug.WriteLine("----------------");

            var s = startFrom ?? styleResourceReferenceHolder;
            Recursive(treeNodeProvider, dependencyPropertyService, s, 0, treeNodeProvider.GetParent(s, type));

            Debug.WriteLine("");
            Debug.WriteLine("Print DomElement hierarchy:");
            Debug.WriteLine("----------------");

            var sDom = treeNodeProvider.GetDomElement(s);
            RecursiveDom(treeNodeProvider, sDom, 0, type == SelectorType.VisualTree ? sDom.Parent : sDom.LogicalParent);

            Debug.WriteLine("----------------");
            Debug.WriteLine("----------------");
        }

        public static void Recursive<TDependencyObject, TStyle, TDependencyProperty>(
            this ITreeNodeProvider<TDependencyObject> treeNodeProvider,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            TDependencyObject element,
            int level,
            TDependencyObject expectedParent
            )
            where TDependencyObject : class
            where TStyle : class
            where TDependencyProperty : class
        {
            var type = SelectorType.LogicalTree;

            if (element == null)
            {
                return;
            }

            if (expectedParent != treeNodeProvider.GetParent(element, type))
            {
                Debug.WriteLine("!!!!!");
                Debug.WriteLine($"Expected parent: { dependencyPropertyService.GetName(expectedParent) } {expectedParent.GetType().Name}");
                Debug.WriteLine($"Actual parent:   { dependencyPropertyService.GetName(treeNodeProvider.GetParent(element, type)) } {treeNodeProvider.GetParent(element, type).GetType().Name}");
                Debug.WriteLine("!!!!!");
            }

            Debug.WriteLine(new String(' ', level) + element.GetType().Name + "#" + dependencyPropertyService.GetName(element));
            var children = treeNodeProvider.GetChildren(element, type);
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
            var type = SelectorType.LogicalTree;
            if (domElement == null)
            {
                return;
            }

            if (expectedParent != domElement.LogicalParent)
            {
                Debug.WriteLine("!!!!!");
                Debug.WriteLine($"Expected parent: { expectedParent.TagName + "#" + expectedParent.Id } {expectedParent.Element?.GetType().Name}");
                Debug.WriteLine($"Actual parent:   { domElement.LogicalParent?.TagName + "#" + domElement.LogicalParent?.Id } {domElement.LogicalParent?.Element?.GetType().Name ?? "null"}");
                Debug.WriteLine("!!!!!");
            }
            if (expectedParent != null &&
                expectedParent.Element != treeNodeProvider.GetParent(domElement.Element, type))
            {
                Debug.WriteLine("XXXXX");
                Debug.WriteLine($"  Expected parent: {expectedParent.Element.GetType().Name}");
                Debug.WriteLine($"  Actual parent:   { treeNodeProvider.GetParent(domElement.Element, type)?.GetType().Name}");
                Debug.WriteLine("XXXXX");
            }

            Debug.WriteLine(new String(' ', level) + domElement.Element.GetType().Name + "#" + domElement.Id);

            var children = treeNodeProvider.GetDomElementChildren(domElement, type);
            foreach (var child in children)
            {
                RecursiveDom(treeNodeProvider, child, level + 1, domElement);
            }
        }
    }
}

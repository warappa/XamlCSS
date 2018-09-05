using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XamlCSS.Dom;

namespace XamlCSS.Utils
{
    public static class HierarchyDebugExtensions
    {
        public static string GetPath<TDependencyObject, TDependencyProperty>(this IDomElement<TDependencyObject, TDependencyProperty> matchingNode, SelectorType type)
            where TDependencyObject : class
        {
            var sb = new List<string>();
            var current = matchingNode;
            while (current != null)
            {
                sb.Add(current.Element.GetType().Name + (!string.IsNullOrWhiteSpace(current.Id) ? "#" + current.Id : ""));
                current = type == SelectorType.VisualTree ? current.Parent : current.LogicalParent;
            }
            sb.Reverse();
            return string.Join("->", sb);
        }

        public static string GetElementPath<TDependencyObject, TDependencyProperty>(this TDependencyObject element, ITreeNodeProvider<TDependencyObject, TDependencyProperty> treeNodeProvider, SelectorType type)
            where TDependencyObject : class
        {
            var sb = new List<string>();
            var current = element;
            while (current != null)
            {
                sb.Add(current.GetType().Name);
                current = treeNodeProvider.GetParent(current, type);
            }
            sb.Reverse();
            return string.Join("->", sb);
        }

        public static void PrintHerarchyDebugInfo<TDependencyObject, TStyle, TDependencyProperty>(
            this ITreeNodeProvider<TDependencyObject, TDependencyProperty> treeNodeProvider,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            TDependencyObject styleResourceReferenceHolder,
            TDependencyObject startFrom,
            SelectorType type
            )
            where TDependencyObject : class
            where TStyle : class
            where TDependencyProperty : class
        {
            Debug.WriteLine("");
            Debug.WriteLine("------------------");
            Debug.WriteLine("Print FrameworkElement hierarchy: " + type.ToString());
            Debug.WriteLine("----------------");
            Debug.WriteLine("----------------");

            var s = startFrom ?? styleResourceReferenceHolder;
            Recursive(treeNodeProvider, dependencyPropertyService, s, 0, treeNodeProvider.GetParent(s, type), type);

            Debug.WriteLine("");
            Debug.WriteLine("Print DomElement hierarchy: " + type.ToString());
            Debug.WriteLine("----------------");

            var sDom = treeNodeProvider.GetDomElement(s);
            RecursiveDom(treeNodeProvider, sDom, 0, type == SelectorType.VisualTree ? sDom.Parent : sDom.LogicalParent, type);

            Debug.WriteLine("----------------");
            Debug.WriteLine("----------------");
        }

        public static void Recursive<TDependencyObject, TStyle, TDependencyProperty>(
            this ITreeNodeProvider<TDependencyObject, TDependencyProperty> treeNodeProvider,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            TDependencyObject element,
            int level,
            TDependencyObject expectedParent,
            SelectorType type
            )
            where TDependencyObject : class
            where TStyle : class
            where TDependencyProperty : class
        {
            if (element == null)
            {
                return;
            }

            var providerParent = treeNodeProvider.GetParent(element, type);
            if (expectedParent != providerParent)
            {
                Debug.WriteLine($"!!!!! element: '{element.ToString()}'");
                Debug.WriteLine($"Expected parent: { dependencyPropertyService.GetName(expectedParent) } {expectedParent.GetType().Name} '{expectedParent.ToString()}'");
                
                Debug.WriteLine($"Provider parent:   { (providerParent != null ? dependencyPropertyService.GetName(providerParent) : "NULL!") } {(providerParent != null ? treeNodeProvider.GetParent(element, type).GetType().Name : "NULL!")}");
                Debug.WriteLine("!!!!!");
            }

            Debug.WriteLine(new String(' ', level * 2) + element.GetType().Name + "#" + dependencyPropertyService.GetName(element));
            var children = treeNodeProvider.GetChildren(element, type);
            foreach (var child in children)
            {
                Recursive(treeNodeProvider, dependencyPropertyService, child, level + 1, element, type);
            }
        }

        public static void RecursiveDom<TDependencyObject, TDependencyProperty>(
            this ITreeNodeProvider<TDependencyObject, TDependencyProperty> treeNodeProvider,
            IDomElement<TDependencyObject, TDependencyProperty> domElement, int level, IDomElement<TDependencyObject, TDependencyProperty> expectedParent,
            SelectorType type)
            where TDependencyObject : class
        {
            var domParent = type == SelectorType.LogicalTree ? domElement.LogicalParent : domElement.Parent;

            if (expectedParent != domParent)
            {
                Debug.WriteLine($"!!!!! DomElement {domElement.Element.GetType().Name}");
                Debug.WriteLine($"Expected parent: { expectedParent.TagName + "#" + expectedParent.Id } {expectedParent.Element?.GetType().Name}");
                Debug.WriteLine($"Provider parent:   {domParent?.TagName + "#" + domParent?.Id } {domParent?.Element?.GetType().Name ?? "null"}");
                Debug.WriteLine("!!!!!");
            }

            if (expectedParent != null &&
                expectedParent.Element != treeNodeProvider.GetParent(domElement.Element, type))
            {
                Debug.WriteLine($"XXXXX {domElement.Element.GetType().Name}");
                Debug.WriteLine($"  Expected parent: {expectedParent.Element.GetType().Name}");
                Debug.WriteLine($"  Provider parent:   { treeNodeProvider.GetParent(domElement.Element, type)?.GetType().Name}");
                Debug.WriteLine("XXXXX");
            }

            Debug.WriteLine(new String(' ', level * 2) + domElement.Element.GetType().Name + "#" + domElement.Id + " | " + string.Join(", ", domElement.StyleInfo?.CurrentMatchedSelectors.Select(x => x.Value) ?? new string[0])+
                $" | {domElement.IsInLogicalTree}/{domElement.IsInVisualTree}");

            var children = treeNodeProvider.GetDomElementChildren(domElement, type);
            foreach (var child in children)
            {
                RecursiveDom(treeNodeProvider, child, level + 1, domElement, type);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using XamlCSS.Dom;
using XamlCSS.WPF.Dom;

namespace XamlCSS.WPF
{
    public class TreeNodeProvider : ITreeNodeProvider<DependencyObject>
    {
        readonly IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyPropertyService;

        public TreeNodeProvider(IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyPropertyService)
        {
            this.dependencyPropertyService = dependencyPropertyService;
        }

        public IEnumerable<DependencyObject> GetChildren(DependencyObject element)
        {
            var list = new List<DependencyObject>();

            try
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
                {
                    var child = VisualTreeHelper.GetChild(element, i) as DependencyObject;

                    if (child != null)
                    {
                        list.Add(child);
                    }
                }
            }
            catch { }
            return list;
        }

        public IEnumerable<IDomElement<DependencyObject>> GetChildren(IDomElement<DependencyObject> node)
        {
            return this.GetChildren(node.Element as DependencyObject)
                .Select(x => GetLogicalTree(x))
                .ToList();
        }

        public DependencyObject GetParent(DependencyObject tUIElement)
        {
            if (tUIElement is FrameworkElement)
            {
                return (tUIElement as FrameworkElement).Parent;
            }

            if (tUIElement is FrameworkContentElement)
            {
                return (tUIElement as FrameworkContentElement).Parent;
            }

            return null;
        }

        public IDomElement<DependencyObject> GetLogicalTreeParent(DependencyObject obj)
        {
            return GetLogicalTree(GetParent(obj));
        }
        public IDomElement<DependencyObject> GetLogicalTree(DependencyObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            var cached = GetFromDependencyObject(obj);

            if (cached != null &&
                cached is LogicalDomElement)
            {
                return cached;
            }

            cached = new LogicalDomElement(obj, GetLogicalTreeParent);
            dependencyPropertyService.SetDomElement(obj, cached);

            return cached;
        }

        public IDomElement<DependencyObject> GetVisualTreeParent(DependencyObject obj)
        {
            return GetVisualTree(GetParent(obj));
        }
        public IDomElement<DependencyObject> GetVisualTree(DependencyObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            var cached = GetFromDependencyObject(obj);

            if (cached != null &&
                cached is VisualDomElement)
            {
                return cached;
            }

            cached = new VisualDomElement(obj, GetVisualTreeParent);
            dependencyPropertyService.SetDomElement(obj, cached);

            return cached;
        }

        private IDomElement<DependencyObject> GetFromDependencyObject(DependencyObject obj)
        {
            return dependencyPropertyService.GetDomElement(obj);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using XamlCSS.Dom;

namespace XamlCSS.WPF.Dom
{
    public class VisualWithLogicalFallbackTreeNodeProvider : TreeNodeProviderBase<DependencyObject, Style, DependencyProperty>
    {
        public VisualWithLogicalFallbackTreeNodeProvider(IDependencyPropertyService<DependencyObject, Style, DependencyProperty> dependencyPropertyService)
            : base(dependencyPropertyService)
        {
        }

        public override IDomElement<DependencyObject> CreateTreeNode(DependencyObject dependencyObject)
        {
            return new LogicalDomElement(dependencyObject, GetDomElement(GetParent(dependencyObject, SelectorType.VisualTree)), GetDomElement(GetParent(dependencyObject, SelectorType.LogicalTree)), this, namespaceProvider);
        }

        public override IEnumerable<DependencyObject> GetChildren(DependencyObject element, SelectorType type)
        {
            var list = new List<DependencyObject>();

            if (element == null)
            {
                return list;
            }

            if (type == SelectorType.VisualTree)
            {
                try
                {
                    list.AddRange(GetVisualChildren(element));

                    /*else
                    {
                        children = LogicalTreeNodeProvider.GetChildren(element).Where(x => !(element is Visual || element is Visual3D)).ToList();
                        for (int i = 0; i < children.Count; i++)
                        {
                            var child = children[i];

                            if (child != null)
                            {
                                list.Add(child);
                            }
                        }
                    }*/

                }
                catch { }
            }
            else
            {
                list = GetLogicalChildren(element).ToList();
            }
            return list;
        }

        public IEnumerable<DependencyObject> GetLogicalChildren(DependencyObject element)
        {
            if (element == null)
            {
                return new List<DependencyObject>();
            }

            var a = LogicalTreeHelper.GetChildren(element)
                .Cast<object>()
                .OfType<DependencyObject>()
                .ToList();

            if (a.Count == 0)
            {
                if (element is ItemsPresenter i)
                {
                    a = GetChildrenOfLogicalParent(element, GetVisualChildren(element));
                }
                else if (element is ContentPresenter c)
                {
                    a = GetChildrenOfLogicalParent(element, GetVisualChildren(element));
                }
            }

            return a;
        }

        private List<DependencyObject> GetChildrenOfLogicalParent(DependencyObject searchParent, IEnumerable<DependencyObject> elements)
        {
            var list = new List<DependencyObject>();

            foreach (var element in elements)
            {
                var found = false;
                if (element is FrameworkElement f)
                {
                    if (f.Parent == searchParent ||
                        f.TemplatedParent == searchParent)
                    {
                        list.Add(f);
                        found = true;
                    }
                }
                else if (element is FrameworkContentElement fc)
                {
                    if (fc.Parent == searchParent ||
                        fc.TemplatedParent == searchParent)
                    {
                        list.Add(fc);
                        found = true;
                    }
                }

                if (!found)
                {
                    list.AddRange(GetChildrenOfLogicalParent(searchParent, GetVisualChildren(element)));
                }
            }

            return list;
        }

        public IEnumerable<DependencyObject> GetVisualChildren(DependencyObject element)
        {
            var list = new List<DependencyObject>();

            if (element == null)
            {
                return list;
            }

            try
            {
                if (element is Visual ||
                    element is Visual3D)
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

                //if (element is IContentHost)
                //{
                //    var children = GetLogicalChildren(element).ToList();
                //    for (int i = 0; i < children.Count; i++)
                //    {
                //        var child = children[i];

                //        if (child != null)
                //        {
                //            list.Add(child);
                //        }
                //    }
                //}

                //if (element is FrameworkContentElement fce)
                //{
                //    list.AddRange(GetLogicalChildren(fce));
                //}
                
            }
            catch { }
            return list;
        }


        public override bool IsInTree(DependencyObject element, SelectorType type)
        {
            //return VisualTreeNodeProvider.IsInTree(element);
            if (type == SelectorType.LogicalTree)
            {
                return IsInLogicalTree(element);
            }
            return IsInVisualTree(element);
        }
        private bool IsInVisualTree(DependencyObject element)
        {
            var p = GetVisualParent(element);
            if (p == null)
                return element is Window;// LogicalTreeHelper.GetParent(element) != null;

            return GetChildren(p, SelectorType.VisualTree).Contains(element);
        }
        private bool IsInLogicalTree(DependencyObject dependencyObject)
        {
            var p = GetLogicalParent(dependencyObject);
            if (p == null)
                return dependencyObject is Window;

            return GetChildren(p, SelectorType.LogicalTree).Contains(dependencyObject);
        }

        public override DependencyObject GetParent(DependencyObject element, SelectorType type)
        {
            if (element == null)
            {
                return null;
            }

            if (type == SelectorType.VisualTree)
            {
                if (element is Visual ||
                    element is Visual3D)
                {
                    return GetVisualParent(element);
                }
                else if (element is FrameworkContentElement)
                {
                    return GetLogicalParent(element);
                }
            }
            else
            {
                return GetLogicalParent(element);
            }

            return null;
        }

        private DependencyObject GetVisualParent(DependencyObject element)
        {
            if (element == null)
            {
                return null;
            }

            DependencyObject parent = null;
            if (element is Visual ||
                element is Visual3D)
            {
                return VisualTreeHelper.GetParent(element);
            }

            // LoadedDetection: would insert into Logical Dom Tree
            return null;// LogicalTreeHelper.GetParent(element);
            //return parent ?? LogicalTreeHelper.GetParent(element);
        }
        private DependencyObject GetLogicalParent(DependencyObject element)
        {
            if (element == null ||
                element is Window)
            {
                return null;
            }

            var p = LogicalTreeHelper.GetParent(element);

            if (p == null)
            {
                if (element is FrameworkElement f)
                {
                    p = f.TemplatedParent;
                }
            }

            return p;
        }

    }
}

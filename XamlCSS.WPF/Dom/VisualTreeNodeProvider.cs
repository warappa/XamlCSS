using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using XamlCSS.Dom;

namespace XamlCSS.WPF.Dom
{
    public class VisualTreeNodeProvider : TreeNodeProviderBase<DependencyObject, Style, DependencyProperty>
    {
        public VisualTreeNodeProvider(IDependencyPropertyService<DependencyObject, Style, DependencyProperty> dependencyPropertyService)
            : base(dependencyPropertyService, SelectorType.VisualTree)
        {
        }

        public override IDomElement<DependencyObject> CreateTreeNode(DependencyObject dependencyObject)
        {
            return new VisualDomElement(dependencyObject, GetDomElement(GetParent(dependencyObject)), this, namespaceProvider);
        }

        public override IEnumerable<DependencyObject> GetChildren(DependencyObject element)
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
                /*else
                {
                    foreach (var child in LogicalTreeHelper.GetChildren(element))
                    {
                        if (child is DependencyObject c)
                        {
                            list.Add(c);
                        }
                    }
                }*/
            }
            catch { }
            return list;
        }

        public override DependencyObject GetParent(DependencyObject element)
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

        public override bool IsInTree(DependencyObject element)
        {
            var p = GetParent(element);
            if (p == null)
                return element is Window;// LogicalTreeHelper.GetParent(element) != null;

            return GetChildren(p).Contains(element);
        }
    }
}

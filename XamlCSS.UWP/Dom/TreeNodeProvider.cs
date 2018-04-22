using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using XamlCSS.Dom;

namespace XamlCSS.UWP.Dom
{
    public class TreeNodeProvider : TreeNodeProviderBase<DependencyObject, Style, DependencyProperty>
    {
        public TreeNodeProvider(IDependencyPropertyService<DependencyObject, Style, DependencyProperty> dependencyPropertyService)
            : base(dependencyPropertyService)
        {
        }

        public override IDomElement<DependencyObject> CreateTreeNode(DependencyObject dependencyObject)
        {
            return new DomElement(dependencyObject, GetDomElement(GetParent(dependencyObject, SelectorType.VisualTree)), GetDomElement(GetParent(dependencyObject, SelectorType.LogicalTree)), this);
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
                }
                catch { }
            }
            else
            {
                list = GetLogicalChildren(element, element).ToList();
            }
            return list;
        }

        private List<DependencyObject> GetLogicalChildren(DependencyObject parent, DependencyObject currentChild)
        {
            var listFound = new List<DependencyObject>();
            var listToCheckFurther = new List<DependencyObject>();

            var count = VisualTreeHelper.GetChildrenCount(currentChild);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(currentChild, i);

                var childsParent = GetParent(child);
                if (childsParent == parent)
                {
                    listFound.Add(child);
                }
                else if (childsParent != null)
                {
                    listToCheckFurther.Add(child);
                }
            }
            foreach (var item in listToCheckFurther)
            {
                listFound.AddRange(GetLogicalChildren(parent, item));
            }

            return listFound;
        }

        public IEnumerable<DependencyObject> GetVisualChildren(DependencyObject element)
        {
            var list = new List<DependencyObject>();

            try
            {
                var count = VisualTreeHelper.GetChildrenCount(element);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(element, i);

                    list.Add(child);
                }
            }
            catch
            {
            }

            return list;
        }

        public override bool IsInTree(DependencyObject element, SelectorType type)
        {
            if (type == SelectorType.LogicalTree)
            {
                return IsInLogicalTree(element);
            }
            return IsInVisualTree(element);
        }

        private DependencyObject GetParent(DependencyObject element)
        {
            return (element as FrameworkElement)?.Parent;
        }

        public bool IsInLogicalTree(DependencyObject dependencyObject)
        {
            var p = GetLogicalParent(dependencyObject);
            if (p == null)
                return dependencyObject is Frame;

            return GetChildren(p, SelectorType.LogicalTree).Contains(dependencyObject);
        }

        public bool IsInVisualTree(DependencyObject element)
        {
            var p = GetVisualParent(element);
            if (p == null)
                return element is Frame;

            return GetChildren(p, SelectorType.VisualTree).Contains(element);
        }

        public override DependencyObject GetParent(DependencyObject element, SelectorType type)
        {
            if (element == null)
            {
                return null;
            }

            if (type == SelectorType.VisualTree)
            {
                return GetVisualParent(element);
            }
            else
            {
                return GetLogicalParent(element);
            }
        }

        private DependencyObject GetVisualParent(DependencyObject element)
        {
            if (element == null)
            {
                return null;
            }

            return VisualTreeHelper.GetParent(element);
        }
        private DependencyObject GetLogicalParent(DependencyObject element)
        {
            if (element == null ||
                element is Frame)
            {
                return null;
            }

            return (element as FrameworkElement)?.Parent;
        }
    }
}

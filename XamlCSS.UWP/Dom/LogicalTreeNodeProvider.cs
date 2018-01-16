using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using XamlCSS.Dom;

namespace XamlCSS.UWP.Dom
{
    public class LogicalTreeNodeProvider : TreeNodeProviderBase<DependencyObject, Style, DependencyProperty>, ISwitchableTreeNodeProvider<DependencyObject>
    {
        public SelectorType CurrentSelectorType => SelectorType.LogicalTree;

        public LogicalTreeNodeProvider(IDependencyPropertyService<DependencyObject, Style, DependencyProperty> dependencyPropertyService)
            : base(dependencyPropertyService, SelectorType.LogicalTree)
        {
        }

        public override IDomElement<DependencyObject> CreateTreeNode(DependencyObject dependencyObject)
        {
            return new LogicalDomElement(dependencyObject, GetDomElement(GetParent(dependencyObject)), this, namespaceProvider);
        }

        public override bool IsCorrectTreeNode(IDomElement<DependencyObject> node)
        {
            return node is LogicalDomElement;
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

        public override IEnumerable<DependencyObject> GetChildren(DependencyObject element)
        {
            var list = new List<DependencyObject>();

            try
            {
                list = GetLogicalChildren(element, element);
            }
            catch
            {
            }

            return list;
        }

        public override DependencyObject GetParent(DependencyObject element)
        {
            return (element as FrameworkElement)?.Parent;
        }

        public override bool IsInTree(DependencyObject dependencyObject)
        {
            var p = GetParent(dependencyObject);
            if (p == null)
                return dependencyObject is Frame;

            return GetChildren(p).Contains(dependencyObject);
        }

        public void Switch(SelectorType type)
        {

        }
    }
}

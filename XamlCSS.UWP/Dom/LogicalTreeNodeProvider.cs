using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using XamlCSS.Dom;

namespace XamlCSS.UWP.Dom
{
    public class LogicalTreeNodeProvider : TreeNodeProviderBase<DependencyObject, Style, DependencyProperty>
    {
        public LogicalTreeNodeProvider(IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyPropertyService)
            : base(dependencyPropertyService)
        {
        }

        protected override IDomElement<DependencyObject> CreateTreeNode(DependencyObject dependencyObject)
        {
            return new LogicalDomElement(dependencyObject, this);
        }

        protected override bool IsCorrectTreeNode(IDomElement<DependencyObject> node)
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
                else if(childsParent != null)
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
    }
}

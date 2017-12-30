using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using XamlCSS.Dom;

namespace XamlCSS.UWP.Dom
{
    public class VisualTreeNodeProvider : TreeNodeProviderBase<DependencyObject, Style, DependencyProperty>
    {
        public VisualTreeNodeProvider(IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyPropertyService)
            : base(dependencyPropertyService, SelectorType.VisualTree)
        {
        }

        public override IDomElement<DependencyObject> CreateTreeNode(DependencyObject dependencyObject)
        {
            return new VisualDomElement(dependencyObject, this, namespaceProvider);
        }

        public override bool IsCorrectTreeNode(IDomElement<DependencyObject> node)
        {
            return node is VisualDomElement;
        }

        public override IEnumerable<DependencyObject> GetChildren(DependencyObject element)
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

        public override DependencyObject GetParent(DependencyObject element)
        {
            if (element == null)
            {
                return null;
            }

            return VisualTreeHelper.GetParent(element);
        }

        public override bool IsInTree(DependencyObject tUIElement)
        {
            return true;
        }
    }
}

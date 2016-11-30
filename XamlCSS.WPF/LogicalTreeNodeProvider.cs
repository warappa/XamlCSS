using System.Collections.Generic;
using System.Linq;
using System.Windows;
using XamlCSS.Dom;
using XamlCSS.WPF.Dom;

namespace XamlCSS.WPF
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

        public override IEnumerable<DependencyObject> GetChildren(DependencyObject element)
        {
            if (element == null)
            {
                return new List<DependencyObject>();
            }

            return LogicalTreeHelper.GetChildren(element)
                .Cast<object>()
                .OfType<DependencyObject>()
                .ToList();
        }

        public override DependencyObject GetParent(DependencyObject element)
        {
            if (element == null)
            {
                return null;
            }

            return LogicalTreeHelper.GetParent(element);
        }
    }
}

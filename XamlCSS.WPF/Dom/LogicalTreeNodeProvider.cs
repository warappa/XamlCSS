using System.Collections.Generic;
using System.Linq;
using System.Windows;
using XamlCSS.Dom;

namespace XamlCSS.WPF.Dom
{
    public class LogicalTreeNodeProvider : TreeNodeProviderBase<DependencyObject, Style, DependencyProperty>
    {
        public LogicalTreeNodeProvider(IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyPropertyService)
            : base(dependencyPropertyService)
        {
            
        }

        protected internal override IDomElement<DependencyObject> CreateTreeNode(DependencyObject dependencyObject)
        {
            return new LogicalDomElement(dependencyObject, this, namespaceProvider);
        }

        protected internal override bool IsCorrectTreeNode(IDomElement<DependencyObject> node)
        {
            return node is LogicalDomElement;
        }

        public override IEnumerable<DependencyObject> GetChildren(DependencyObject element)
        {
            if (element == null)
            {
                return new List<DependencyObject>();
            }

            var a = LogicalTreeHelper.GetChildren(element)
                .Cast<object>()
                .OfType<DependencyObject>()
                .ToList();

            return a;
        }

        public override DependencyObject GetParent(DependencyObject element)
        {
            if (element == null)
            {
                return null;
            }

            return LogicalTreeHelper.GetParent(element);
        }

        public override bool IsInTree(DependencyObject tUIElement)
        {
            var p = GetParent(tUIElement);
            if (p == null)
                return true;

            return GetChildren(p).Contains(tUIElement);
        }
    }
}

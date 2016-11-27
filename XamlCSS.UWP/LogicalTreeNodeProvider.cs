using Windows.UI.Xaml;
using XamlCSS.Dom;
using XamlCSS.UWP.Dom;

namespace XamlCSS.UWP
{
    public class LogicalTreeNodeProvider : TreeNodeProviderBase
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
    }
}

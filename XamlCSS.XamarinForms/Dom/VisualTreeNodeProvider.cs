using System.Collections.Generic;
using Xamarin.Forms;
using XamlCSS.Dom;
using XamlCSS.Windows.Media;

namespace XamlCSS.XamarinForms.Dom
{
    public class VisualTreeNodeProvider : TreeNodeProviderBase<BindableObject, Style, BindableProperty>
    {
        public VisualTreeNodeProvider(IDependencyPropertyService<BindableObject, BindableObject, Style, BindableProperty> dependencyPropertyService)
            : base(dependencyPropertyService)
        {
        }

        protected internal override IDomElement<BindableObject> CreateTreeNode(BindableObject dependencyObject)
        {
            return new VisualDomElement(dependencyObject, this);
        }

        protected internal override bool IsCorrectTreeNode(IDomElement<BindableObject> node)
        {
            return node is VisualDomElement;
        }

        public override IEnumerable<BindableObject> GetChildren(BindableObject element)
        {
            return VisualTreeHelper.GetChildren(element as Element);
        }

        public override BindableObject GetParent(BindableObject element)
        {
            if (element == null)
            {
                return null;
            }

            return VisualTreeHelper.GetParent(element as Element);
        }

        public override bool IsInTree(BindableObject tUIElement)
        {
            return true;
        }
    }
}

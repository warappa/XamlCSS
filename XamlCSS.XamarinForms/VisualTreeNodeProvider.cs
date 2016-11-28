using System.Collections.Generic;
using Xamarin.Forms;
using XamlCSS.Dom;
using XamlCSS.Windows.Media;
using XamlCSS.XamarinForms.Dom;

namespace XamlCSS.XamarinForms
{
    public class VisualTreeNodeProvider : TreeNodeProviderBase
    {
        public VisualTreeNodeProvider(IDependencyPropertyService<BindableObject, BindableObject, Style, BindableProperty> dependencyPropertyService)
            : base(dependencyPropertyService)
        {
        }

        protected override IDomElement<BindableObject> CreateTreeNode(BindableObject BindableObject)
        {
            return new VisualDomElement(BindableObject, this);
        }

        protected override bool IsCorrectTreeNode(IDomElement<BindableObject> node)
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
    }
}

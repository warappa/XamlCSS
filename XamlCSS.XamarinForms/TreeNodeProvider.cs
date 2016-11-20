using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using XamlCSS.Dom;
using XamlCSS.Windows.Media;
using XamlCSS.XamarinForms.Dom;

namespace XamlCSS.XamarinForms
{
    public class TreeNodeProvider : ITreeNodeProvider<BindableObject>
    {
        readonly IDependencyPropertyService<BindableObject, Element, Style, BindableProperty> dependencyPropertyService;

        public TreeNodeProvider(IDependencyPropertyService<BindableObject, Element, Style, BindableProperty> dependencyPropertyService)
        {
            this.dependencyPropertyService = dependencyPropertyService;
        }
        public IEnumerable<BindableObject> GetChildren(BindableObject element)
        {
            return VisualTreeHelper.GetChildren(element as Element);
        }

        public IEnumerable<IDomElement<BindableObject>> GetChildren(IDomElement<BindableObject> node)
        {
            return this.GetChildren(node.Element)
                .Select(x => GetLogicalTree(x))
                .ToList();
        }

        public BindableObject GetParent(BindableObject tUIElement)
        {
            if (tUIElement is Element)
                return (tUIElement as Element).Parent;

            return null;
        }

        public IDomElement<BindableObject> GetLogicalTreeParent(BindableObject obj)
        {
            return GetLogicalTree(GetParent(obj));
        }
        public IDomElement<BindableObject> GetLogicalTree(BindableObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            var cached = GetFromDependencyObject(obj);

            if (cached != null &&
                cached is LogicalDomElement)
            {
                return cached;
            }
            cached = new LogicalDomElement(obj, GetLogicalTreeParent);
            dependencyPropertyService.SetDomElement(obj, cached);

            return cached;
        }

        public IDomElement<BindableObject> GetVisualTreeParent(BindableObject obj)
        {
            return GetVisualTree((obj as Element)?.Parent);
        }
        public IDomElement<BindableObject> GetVisualTree(BindableObject obj)
        {
            var cached = GetFromDependencyObject(obj);

            if (cached != null &&
                cached is VisualDomElement)
            {
                return cached;
            }

            cached = new VisualDomElement(obj, GetVisualTreeParent);
            dependencyPropertyService.SetDomElement(obj, cached);

            return cached;
        }

        private IDomElement<BindableObject> GetFromDependencyObject(BindableObject obj)
        {
            return dependencyPropertyService.GetDomElement(obj);
        }
    }
}

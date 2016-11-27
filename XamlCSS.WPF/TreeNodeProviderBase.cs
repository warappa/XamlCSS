using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using XamlCSS.Dom;

namespace XamlCSS.WPF
{
    public abstract class TreeNodeProviderBase : ITreeNodeProvider<DependencyObject>
    {
        readonly IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyPropertyService;

        public TreeNodeProviderBase(IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyPropertyService)
        {
            this.dependencyPropertyService = dependencyPropertyService;
        }

        protected abstract IDomElement<DependencyObject> CreateTreeNode(DependencyObject dependencyObject);

        protected abstract bool IsCorrectTreeNode(IDomElement<DependencyObject> node);

        public abstract DependencyObject GetParent(DependencyObject tUIElement);

        public abstract IEnumerable<DependencyObject> GetChildren(DependencyObject element);

        public IEnumerable<IDomElement<DependencyObject>> GetDomElementChildren(IDomElement<DependencyObject> node)
        {
            return this.GetChildren(node.Element as DependencyObject)
                .Select(x => GetDomElement(x))
                .ToList();
        }

        public IDomElement<DependencyObject> GetTreeParentNode(DependencyObject obj)
        {
            return GetDomElement(GetParent(obj));
        }
        public IDomElement<DependencyObject> GetDomElement(DependencyObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            var cached = GetFromDependencyObject(obj);

            if (cached != null &&
                IsCorrectTreeNode(cached))
            {
                return cached;
            }

            cached = CreateTreeNode(obj);
            dependencyPropertyService.SetDomElement(obj, cached);

            return cached;
        }

        private IDomElement<DependencyObject> GetFromDependencyObject(DependencyObject obj)
        {
            return dependencyPropertyService.GetDomElement(obj);
        }
    }
}

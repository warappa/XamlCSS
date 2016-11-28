using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using XamlCSS.Dom;

namespace XamlCSS.XamarinForms
{
    public abstract class TreeNodeProviderBase : ITreeNodeProvider<BindableObject>
    {
        readonly IDependencyPropertyService<BindableObject, BindableObject, Style, BindableProperty> dependencyPropertyService;

        public TreeNodeProviderBase(IDependencyPropertyService<BindableObject, BindableObject, Style, BindableProperty> dependencyPropertyService)
        {
            this.dependencyPropertyService = dependencyPropertyService;
        }

        protected abstract IDomElement<BindableObject> CreateTreeNode(BindableObject dependencyObject);

        protected abstract bool IsCorrectTreeNode(IDomElement<BindableObject> node);

        public abstract BindableObject GetParent(BindableObject tUIElement);
        
        public abstract IEnumerable<BindableObject> GetChildren(BindableObject element);

        public IEnumerable<IDomElement<BindableObject>> GetDomElementChildren(IDomElement<BindableObject> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Element == null) throw new ArgumentNullException(nameof(node.Element));

            return this.GetChildren(node.Element as BindableObject)
                .Select(x => this.GetDomElement(x))
                .ToList();
        }

        public IDomElement<BindableObject> GetTreeParentNode(BindableObject obj)
        {
            return GetDomElement(GetParent(obj));
        }
        public IDomElement<BindableObject> GetDomElement(BindableObject obj)
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

        private IDomElement<BindableObject> GetFromDependencyObject(BindableObject obj)
        {
            return dependencyPropertyService.GetDomElement(obj);
        }
    }
}

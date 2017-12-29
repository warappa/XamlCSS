using System;
using System.Collections.Generic;
using System.Linq;

namespace XamlCSS.Dom
{
    public abstract class TreeNodeProviderBase<TDependencyObject, TStyle, TDependencyProperty> : ITreeNodeProvider<TDependencyObject>
        where TDependencyObject : class
        where TStyle : class
        where TDependencyProperty : class
    {
        protected readonly IDependencyPropertyService<TDependencyObject, TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService;
        protected readonly INamespaceProvider<TDependencyObject> namespaceProvider;
        protected SelectorType selectorType;

        public TreeNodeProviderBase(IDependencyPropertyService<TDependencyObject, TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            SelectorType selectorType)
        {
            this.dependencyPropertyService = dependencyPropertyService;
            this.namespaceProvider = new NamespaceProvider<TDependencyObject, TDependencyObject, TStyle, TDependencyProperty>(dependencyPropertyService);
            this.selectorType = selectorType;
        }

        protected internal abstract IDomElement<TDependencyObject> CreateTreeNode(TDependencyObject dependencyObject);

        protected internal abstract bool IsCorrectTreeNode(IDomElement<TDependencyObject> node);

        public abstract TDependencyObject GetParent(TDependencyObject tUIElement);

        public abstract IEnumerable<TDependencyObject> GetChildren(TDependencyObject element);

        public IEnumerable<IDomElement<TDependencyObject>> GetDomElementChildren(IDomElement<TDependencyObject> node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Element == null) throw new ArgumentNullException(nameof(node.Element));

            return this.GetChildren(node.Element as TDependencyObject)
                .Select(x => this.GetDomElement(x))
                .ToList();
        }

        public IDomElement<TDependencyObject> GetTreeParentNode(TDependencyObject obj)
        {
            return GetDomElement(GetParent(obj));
        }

        public IDomElement<TDependencyObject> GetDomElement(TDependencyObject obj)
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
            dependencyPropertyService.SetDomElement(obj, cached, selectorType);

            return cached;
        }

        public abstract bool IsInTree(TDependencyObject tUIElement);

        private IDomElement<TDependencyObject> GetFromDependencyObject(TDependencyObject obj)
        {
            return dependencyPropertyService.GetDomElement(obj, selectorType);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using XamlCSS.Utils;

namespace XamlCSS.Dom
{
    public abstract class TreeNodeProviderBase<TDependencyObject, TStyle, TDependencyProperty> : ITreeNodeProvider<TDependencyObject>
        where TDependencyObject : class
        where TStyle : class
        where TDependencyProperty : class
    {
        protected readonly IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService;
        protected readonly INamespaceProvider<TDependencyObject> namespaceProvider;
        protected SelectorType selectorType;

        public TreeNodeProviderBase(IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            SelectorType selectorType)
        {
            this.dependencyPropertyService = dependencyPropertyService;
            this.namespaceProvider = new NamespaceProvider<TDependencyObject, TStyle, TDependencyProperty>(dependencyPropertyService);
            this.selectorType = selectorType;
        }

        public abstract IDomElement<TDependencyObject> CreateTreeNode(TDependencyObject dependencyObject);

        public abstract TDependencyObject GetParent(TDependencyObject dependencyObject);

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

            if (cached != null)
            {
                return cached;
            }

            cached = "CreateTreeNode".Measure(() => CreateTreeNode(obj));
            "SetDomElement".Measure(() => dependencyPropertyService.SetDomElement(obj, cached, selectorType));

            return cached;
        }

        public abstract bool IsInTree(TDependencyObject dependencyObject);

        private IDomElement<TDependencyObject> GetFromDependencyObject(TDependencyObject obj)
        {
            return dependencyPropertyService.GetDomElement(obj, selectorType);
        }
    }
}

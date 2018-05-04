using System;
using System.Collections.Generic;

namespace XamlCSS.Dom
{
    public abstract class TreeNodeProviderBase<TDependencyObject, TStyle, TDependencyProperty> : ITreeNodeProvider<TDependencyObject, TDependencyProperty>
        where TDependencyObject : class
        where TStyle : class
        where TDependencyProperty : class
    {
        protected readonly IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService;

        public TreeNodeProviderBase(IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService)
        {
            this.dependencyPropertyService = dependencyPropertyService;
        }

        public abstract IDomElement<TDependencyObject, TDependencyProperty> CreateTreeNode(TDependencyObject dependencyObject);

        public abstract TDependencyObject GetParent(TDependencyObject dependencyObject, SelectorType type);

        public abstract IEnumerable<TDependencyObject> GetChildren(TDependencyObject element, SelectorType type);

        public IEnumerable<IDomElement<TDependencyObject, TDependencyProperty>> GetDomElementChildren(IDomElement<TDependencyObject, TDependencyProperty> node, SelectorType type)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Element == null) throw new ArgumentNullException(nameof(node.Element));

            if (type == SelectorType.LogicalTree)
            {
                return node.LogicalChildNodes;
            }
            else if (type == SelectorType.VisualTree)
            {
                return node.ChildNodes;
            }

            throw new Exception("Invalid SelectorType " + type.ToString());
        }

        public IDomElement<TDependencyObject, TDependencyProperty> GetDomElement(TDependencyObject obj)
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

            cached = CreateTreeNode(obj);
            dependencyPropertyService.SetDomElement(obj, cached);

            return cached;
        }

        public abstract bool IsInTree(TDependencyObject dependencyObject, SelectorType type);

        private IDomElement<TDependencyObject, TDependencyProperty> GetFromDependencyObject(TDependencyObject obj)
        {
            return dependencyPropertyService.GetDomElement(obj);
        }
    }
}

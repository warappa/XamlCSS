using System.Collections.Generic;
using AngleSharp.Dom;
using XamlCSS.Dom;
using Windows.UI.Xaml;
using System;
using System.Linq;

namespace XamlCSS.UWP.Dom
{
    public abstract class DomElement : DomElementBase<DependencyObject, DependencyProperty>, IDisposable
    {
        public DomElement(DependencyObject dependencyObject, ITreeNodeProvider<DependencyObject> treeNodeProvider)
            : base(dependencyObject, treeNodeProvider)
        {
            RegisterChildrenChangeHandler();
        }

        private void RegisterChildrenChangeHandler()
        {
            LoadedDetectionHelper.SubTreeAdded += DomElementAdded;
            LoadedDetectionHelper.SubTreeRemoved += DomElementRemoved;
        }

        void IDisposable.Dispose()
        {
            UnregisterChildrenChangeHandler();

            base.Dispose();
        }

        private void UnregisterChildrenChangeHandler()
        {
            LoadedDetectionHelper.SubTreeAdded -= DomElementAdded;
            LoadedDetectionHelper.SubTreeRemoved -= DomElementRemoved;
        }

        protected override IHtmlCollection<IElement> CreateCollection(IEnumerable<IElement> list)
        {
            return new ElementCollection(list, treeNodeProvider);
        }
        protected override INamedNodeMap CreateNamedNodeMap(DependencyObject dependencyObject)
        {
            return new NamedNodeMap(dependencyObject);
        }

        protected override IHtmlCollection<IElement> GetChildElements(DependencyObject dependencyObject)
        {
            return new ElementCollection(this, treeNodeProvider);
        }
        protected override INodeList GetChildNodes(DependencyObject dependencyObject)
        {
            return new NamedNodeList(this, treeNodeProvider);
        }
        protected override INodeList CreateNodeList(IEnumerable<INode> nodes)
        {
            return new NamedNodeList(nodes, treeNodeProvider);
        }
        protected override ITokenList GetClassList(DependencyObject dependencyObject)
        {
            var list = new TokenList();
            var classNames = Css.GetClass(dependencyObject)?.Split(' ');

            if (classNames != null)
            {
                list.AddRange(classNames);
            }

            return list;
        }
        protected override string GetId(DependencyObject dependencyObject)
        {
            return dependencyObject.ReadLocalValue(FrameworkElement.NameProperty) as string;
        }
    }
}

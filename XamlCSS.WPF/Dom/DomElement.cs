using System;
using System.Collections.Generic;
using System.Windows;
using AngleSharp.Dom;
using XamlCSS.Dom;
using System.Windows.Media;
using System.Linq;

namespace XamlCSS.WPF.Dom
{
    public abstract class DomElement : DomElementBase<DependencyObject, DependencyProperty>
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

        public new void Dispose()
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

            var classNames = Css.GetClass(dependencyObject)?.Split(classSplitter, StringSplitOptions.RemoveEmptyEntries);
            if (classNames?.Length > 0)
            {
                list.AddRange(classNames);
            }

            return list;
        }
        protected override string GetId(DependencyObject dependencyObject)
        {
            if (dependencyObject is FrameworkElement)
            {
                return dependencyObject.ReadLocalValue(FrameworkElement.NameProperty) as string;
            }

            return dependencyObject.ReadLocalValue(FrameworkContentElement.NameProperty) as string;
        }
        public override bool Equals(object obj)
        {
            var other = obj as DomElement;
            if (other == null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.dependencyObject == other.dependencyObject;
        }

        public static bool operator ==(DomElement a, DomElement b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            return a?.Equals(b) == true;
        }
        public static bool operator !=(DomElement a, DomElement b)
        {
            return !(a == b);
        }
    }
}

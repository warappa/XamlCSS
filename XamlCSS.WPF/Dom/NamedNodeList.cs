using System.Collections.Generic;
using System.Windows;
using AngleSharp.Dom;
using XamlCSS.Dom;

namespace XamlCSS.WPF.Dom
{
    public class NamedNodeList : NamedNodeListBase<DependencyObject, DependencyProperty>
    {
        public NamedNodeList(DomElementBase<DependencyObject, DependencyProperty> node, ITreeNodeProvider<DependencyObject> treeNodeProvider)
            : base(node, treeNodeProvider)
        {

        }

        public NamedNodeList(IEnumerable<INode> nodes, ITreeNodeProvider<DependencyObject> treeNodeProvider)
            : base(nodes, treeNodeProvider)
        {

        }

        protected override INode CreateNode(DependencyObject dependencyObject, IDomElement<DependencyObject> parentNode)
        {
            return treeNodeProvider.GetDomElement(dependencyObject);
        }
        protected override IEnumerable<DependencyObject> GetChildren(DependencyObject dependencyObject)
        {
            return treeNodeProvider.GetChildren(dependencyObject);
        }
    }
}

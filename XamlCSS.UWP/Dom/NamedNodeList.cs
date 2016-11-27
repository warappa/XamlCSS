using System.Collections.Generic;
using AngleSharp.Dom;
using XamlCSS.Dom;
using Windows.UI.Xaml;
using System;

namespace XamlCSS.UWP.Dom
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

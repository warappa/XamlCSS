using System.Collections.Generic;
using AngleSharp.Dom;
using XamlCSS.Dom;
using Windows.UI.Xaml;

namespace XamlCSS.UWP.Dom
{
	public class NamedNodeList : NamedNodeListBase<DependencyObject, DependencyProperty>
	{
		public NamedNodeList(DomElementBase<DependencyObject, DependencyProperty> node)
			: base(node)
		{

		}

		public NamedNodeList(IEnumerable<INode> nodes)
			: base(nodes)
		{

		}

		protected override INode CreateNode(DependencyObject dependencyObject, IDomElement<DependencyObject> parentNode)
		{
			return new LogicalDomElement(dependencyObject, parentNode);
		}
		protected override IEnumerable<DependencyObject> GetChildren(DependencyObject dependencyObject)
		{
			return new TreeNodeProvider(new DependencyPropertyService()).GetChildren(dependencyObject);
		}
	}
}

using System.Collections.Generic;
using AngleSharp.Dom;
using XamlCSS.Dom;
using Xamarin.Forms;
using XamlCSS.Windows.Media;

namespace XamlCSS.XamarinForms.Dom
{
	public class NamedNodeList : NamedNodeListBase<BindableObject, BindableProperty>
	{
		public NamedNodeList(DomElementBase<BindableObject, BindableProperty> node)
			: base(node)
		{

		}

		public NamedNodeList(IEnumerable<INode> nodes)
			: base(nodes)
		{

		}

		protected override INode CreateNode(BindableObject dependencyObject, IDomElement<BindableObject> parentNode)
		{
			return new LogicalDomElement(dependencyObject, parentNode);
		}
		protected override IEnumerable<BindableObject> GetChildren(BindableObject dependencyObject)
		{
			return VisualTreeHelper.GetChildren(dependencyObject as Element);
		}
	}
}

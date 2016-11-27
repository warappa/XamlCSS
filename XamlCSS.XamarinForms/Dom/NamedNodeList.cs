using System.Collections.Generic;
using AngleSharp.Dom;
using XamlCSS.Dom;
using Xamarin.Forms;
using XamlCSS.Windows.Media;

namespace XamlCSS.XamarinForms.Dom
{
	public class NamedNodeList : NamedNodeListBase<BindableObject, BindableProperty>
	{
		public NamedNodeList(DomElementBase<BindableObject, BindableProperty> node, ITreeNodeProvider<BindableObject> treeNodeProvider)
			: base(node, treeNodeProvider)
		{

		}

		public NamedNodeList(IEnumerable<INode> nodes, ITreeNodeProvider<BindableObject> treeNodeProvider)
			: base(nodes, treeNodeProvider)
		{

		}

		protected override INode CreateNode(BindableObject dependencyObject, IDomElement<BindableObject> parentNode)
		{
            return treeNodeProvider.GetDomElement(dependencyObject);
        }
		protected override IEnumerable<BindableObject> GetChildren(BindableObject dependencyObject)
		{
			return VisualTreeHelper.GetChildren(dependencyObject as Element);
		}
	}
}

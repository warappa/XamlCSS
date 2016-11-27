using System.Collections.Generic;
using AngleSharp.Dom;
using XamlCSS.Dom;
using Xamarin.Forms;
using XamlCSS.Windows.Media;

namespace XamlCSS.XamarinForms.Dom
{
	public class ElementCollection : ElementCollectionBase<BindableObject>
	{
		public ElementCollection(IDomElement<BindableObject> node, ITreeNodeProvider<BindableObject> treeNodeProvider)
			: base(node, treeNodeProvider)
		{

		}

		public ElementCollection(IEnumerable<IElement> elements, ITreeNodeProvider<BindableObject> treeNodeProvider)
			: base(elements, treeNodeProvider)
		{

		}

		protected override IElement CreateElement(BindableObject dependencyObject, IDomElement<BindableObject> parentNode)
		{
			return new LogicalDomElement(dependencyObject, treeNodeProvider);
		}
		protected override IEnumerable<BindableObject> GetChildren(BindableObject dependencyObject)
		{
			return VisualTreeHelper.GetChildren(dependencyObject as Element);
		}
		protected override string GetId(BindableObject dependencyObject)
		{
			return Css.GetId(dependencyObject);
		}
	}
}

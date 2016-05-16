using System.Collections.Generic;
using AngleSharp.Dom;
using XamlCSS.Dom;
using Xamarin.Forms;
using XamlCSS.Windows.Media;

namespace XamlCSS.XamarinForms.Dom
{
	public class ElementCollection : ElementCollectionBase<BindableObject>
	{
		public ElementCollection(IDomElement<BindableObject> node)
			: base(node)
		{

		}

		public ElementCollection(IEnumerable<IElement> elements)
			: base(elements)
		{

		}

		protected override IElement CreateElement(BindableObject dependencyObject, IDomElement<BindableObject> parentNode)
		{
			return new LogicalDomElement(dependencyObject, parentNode);
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

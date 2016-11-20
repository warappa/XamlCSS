using System.Collections.Generic;
using AngleSharp.Dom;
using XamlCSS.Dom;
using Windows.UI.Xaml;

namespace XamlCSS.UWP.Dom
{
	public class ElementCollection : ElementCollectionBase<DependencyObject>
	{
		public ElementCollection(IDomElement<DependencyObject> node)
			: base(node)
		{

		}

		public ElementCollection(IEnumerable<IElement> elements)
			: base(elements)
		{

		}

		protected override IElement CreateElement(DependencyObject dependencyObject, IDomElement<DependencyObject> parentNode)
		{
			return new LogicalDomElement(dependencyObject, parentNode);
		}
		protected override IEnumerable<DependencyObject> GetChildren(DependencyObject dependencyObject)
		{
			return new TreeNodeProvider(new DependencyPropertyService()).GetChildren(dependencyObject);
		}
		protected override string GetId(DependencyObject dependencyObject)
		{
			return dependencyObject.ReadLocalValue(FrameworkElement.NameProperty) as string;
		}
	}
}

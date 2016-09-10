using System.Collections.Generic;
using System.Windows;
using AngleSharp.Dom;
using XamlCSS.Dom;
using System.Linq;
using System.Windows.Controls;

namespace XamlCSS.WPF.Dom
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
			if (dependencyObject is Window)
			{
				return new List<DependencyObject>() { (dependencyObject as Window).Content as DependencyObject };
			}

			if (dependencyObject is Page)
			{
				return new List<DependencyObject>() { (dependencyObject as Page).Content as DependencyObject };
			}

			if (dependencyObject is Panel)
			{
				return new List<DependencyObject>((dependencyObject as Panel).Children.Cast<DependencyObject>());
			}

			var list = new List<DependencyObject>();

			var res = LogicalTreeHelper.GetChildren(dependencyObject);
			foreach (var i in res)
			{
				if (i is DependencyObject)
				{
					list.Add((DependencyObject)i);
				}
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
	}
}

using System.Windows;
using AngleSharp.Dom;
using XamlCSS.Dom;

namespace XamlCSS.WPF.Dom
{
	public class NamedNodeMap : NamedNodeMapBase<DependencyObject, DependencyProperty>
	{
		public NamedNodeMap(DependencyObject dependencyObject)
			: base(dependencyObject)
		{

		}
		protected override IAttr CreateAttribute(DependencyObject dependencyObject, DependencyProperty property)
		{
			return new ElementAttribute(dependencyObject, property);
		}
	}
}

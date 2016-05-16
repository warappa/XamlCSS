using System;
using System.Windows;
using XamlCSS.Dom;

namespace XamlCSS.WPF.Dom
{
	public class ElementAttribute : ElementAttributeBase<DependencyObject, DependencyProperty>
	{
		public ElementAttribute(DependencyObject dependencyObject, DependencyProperty property)
			: base(dependencyObject, property)
		{

		}

		public override string Value
		{
			get
			{
				return this.dependencyObject.ReadLocalValue(property) as string;
			}

			set
			{
				throw new NotImplementedException();
			}
		}
	}
}

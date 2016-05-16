using System;
using Xamarin.Forms;
using XamlCSS.Dom;

namespace XamlCSS.XamarinForms.Dom
{
	public class ElementAttribute : ElementAttributeBase<BindableObject, BindableProperty>
	{
		public ElementAttribute(BindableObject dependencyObject, BindableProperty property)
			: base(dependencyObject, property)
		{

		}

		public override string Value
		{
			get
			{
				return this.dependencyObject.GetValue(property) as string;
			}

			set
			{
				throw new NotImplementedException();
			}
		}
	}
}
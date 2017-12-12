using System;
using XamlCSS.Dom;
using Windows.UI.Xaml;

namespace XamlCSS.UWP.Dom
{
	public class ElementAttribute : ElementAttributeBase<DependencyObject, DependencyProperty>
	{
        private string propertyName;

        public ElementAttribute(DependencyObject dependencyObject, DependencyProperty property, string propertyName)
			: base(dependencyObject, property)
		{
            this.propertyName = propertyName;
        }

        public override string LocalName => propertyName;
        public override string Name => propertyName;

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

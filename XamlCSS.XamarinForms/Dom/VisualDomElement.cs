using System;
using Xamarin.Forms;
using XamlCSS.Dom;

namespace XamlCSS.XamarinForms.Dom
{
	public class VisualDomElement : DomElement
	{
		public VisualDomElement(BindableObject dependencyObject, IDomElement<BindableObject> parent)
			: base(dependencyObject, parent)
		{
        }
		public VisualDomElement(BindableObject dependencyObject, Func<BindableObject, IDomElement<BindableObject>> getParent)
			: base(dependencyObject, getParent)
        {
        }

        public override bool Equals(object obj)
		{
			var otherNode = obj as VisualDomElement;
			return otherNode != null ?
				otherNode.dependencyObject.Equals(dependencyObject) :
				false;
		}

		public override int GetHashCode()
		{
			return dependencyObject.GetHashCode();
		}
	}
}

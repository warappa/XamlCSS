using Xamarin.Forms;
using XamlCSS.Dom;

namespace XamlCSS.XamarinForms.Dom
{
	public class VisualDomElement : DomElement
	{
		public VisualDomElement(BindableObject dependencyObject, IDomElement<BindableObject> parent) :
			base(dependencyObject, parent)
		{
		}


		public override bool Equals(object obj)
		{
			var otherNode = obj as VisualDomElement;
			return otherNode != null ? otherNode.dependencyObject.Equals(dependencyObject) : false;
		}

		public override int GetHashCode()
		{
			return dependencyObject.GetHashCode();
		}
	}
}

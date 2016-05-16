using System.Diagnostics;
using Xamarin.Forms;
using XamlCSS.Dom;

namespace XamlCSS.XamarinForms.Dom
{
	[DebuggerDisplay("Id={Id} Name={Name} Class={Class}")]
	public class LogicalDomElement : DomElement
	{
		public LogicalDomElement(BindableObject dependencyObject, IDomElement<BindableObject> parent)
			: base(dependencyObject, parent)
		{

		}

		public override bool Equals(object obj)
		{
			LogicalDomElement otherNode = obj as LogicalDomElement;
			return otherNode != null ? otherNode.dependencyObject.Equals(dependencyObject) : false;
		}

		public override int GetHashCode()
		{
			return dependencyObject.GetHashCode();
		}
	}
}

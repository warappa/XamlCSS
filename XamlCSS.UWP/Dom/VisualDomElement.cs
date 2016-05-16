using System.Diagnostics;
using Windows.UI.Xaml;
using XamlCSS.Dom;

namespace XamlCSS.UWP.Dom
{
	[DebuggerDisplay("Id={Id} Name={Name} Class={Class}")]
	public class VisualDomElement : DomElement
	{
		public VisualDomElement(DependencyObject dependencyObject, IDomElement<DependencyObject> parent) :
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

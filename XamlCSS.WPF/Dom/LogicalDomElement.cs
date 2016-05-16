using System.Diagnostics;
using XamlCSS.Dom;
using System.Windows;

namespace XamlCSS.WPF.Dom
{
	[DebuggerDisplay("Id={Id} Name={Name} Class={Class}")]
	public class LogicalDomElement : DomElement
	{
		public LogicalDomElement(DependencyObject dependencyObject, IDomElement<DependencyObject> parent)
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

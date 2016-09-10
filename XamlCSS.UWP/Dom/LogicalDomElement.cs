using Windows.UI.Xaml;
using System.Diagnostics;
using XamlCSS.Dom;
using System;

namespace XamlCSS.UWP.Dom
{
	[DebuggerDisplay("Id={Id} Name={Name} Class={Class}")]
	public class LogicalDomElement : DomElement
	{
		public LogicalDomElement(DependencyObject dependencyObject, IDomElement<DependencyObject> parent)
			: base(dependencyObject, parent)
		{

		}
		public LogicalDomElement(DependencyObject dependencyObject, Func<DependencyObject, IDomElement<DependencyObject>> getParent)
			: base(dependencyObject, getParent)
		{

		}

		public override bool Equals(object obj)
		{
			LogicalDomElement otherNode = obj as LogicalDomElement;

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

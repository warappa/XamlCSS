using Windows.UI.Xaml;
using System.Diagnostics;
using XamlCSS.Dom;
using System;

namespace XamlCSS.UWP.Dom
{
	[DebuggerDisplay("Id={Id} Name={Name} Class={Class}")]
	public class LogicalDomElement : DomElement
	{
		public LogicalDomElement(DependencyObject dependencyObject, ITreeNodeProvider<DependencyObject> treeNodeProvider)
			: base(dependencyObject, treeNodeProvider)
		{

		}

		public override bool Equals(object obj)
		{
			var otherNode = obj as LogicalDomElement;

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

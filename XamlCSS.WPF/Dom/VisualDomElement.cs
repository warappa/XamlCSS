using System;
using System.Diagnostics;
using System.Windows;
using XamlCSS.Dom;

namespace XamlCSS.WPF.Dom
{
	[DebuggerDisplay("Id={Id} Name={Name} Class={Class}")]
	public class VisualDomElement : DomElement
	{
		public VisualDomElement(DependencyObject dependencyObject, ITreeNodeProvider<DependencyObject> treeNodeProvider)
			: base(dependencyObject, treeNodeProvider)
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

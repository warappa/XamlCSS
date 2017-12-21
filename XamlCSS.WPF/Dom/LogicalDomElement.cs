using System.Diagnostics;
using XamlCSS.Dom;
using System.Windows;
using System;

namespace XamlCSS.WPF.Dom
{
	[DebuggerDisplay("({Element.GetType().Name}) Id={Id} Name={Name} Class={Class}")]
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

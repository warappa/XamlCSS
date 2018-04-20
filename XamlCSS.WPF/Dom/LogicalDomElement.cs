using System.Diagnostics;
using XamlCSS.Dom;
using System.Windows;
using System;

namespace XamlCSS.WPF.Dom
{
	[DebuggerDisplay("LOGICAL ({Element.GetType().Name}) Parent={Parent?.Element}  Id={Id} Name={Name} Class={Class}")]
	public class LogicalDomElement : DomElement
	{
		public LogicalDomElement(DependencyObject dependencyObject, IDomElement<DependencyObject> parent, IDomElement<DependencyObject> logicalParent, ITreeNodeProvider<DependencyObject> treeNodeProvider)
			: base(dependencyObject, parent, logicalParent, treeNodeProvider)
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

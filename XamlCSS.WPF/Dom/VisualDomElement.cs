using System;
using System.Diagnostics;
using System.Windows;
using XamlCSS.Dom;

namespace XamlCSS.WPF.Dom
{
 //   [DebuggerDisplay("Visual ({Element.GetType().Name}) Parent={Parent.Element}  Id={Id} Name={Name} Class={Class}")]
 //   public class VisualDomElement : DomElement
	//{
	//	public VisualDomElement(DependencyObject dependencyObject, IDomElement<DependencyObject> parent, ITreeNodeProvider<DependencyObject> treeNodeProvider, INamespaceProvider<DependencyObject> namespaceProvider)
	//		: base(dependencyObject, parent, treeNodeProvider, namespaceProvider)
	//	{

	//	}

	//	public override bool Equals(object obj)
	//	{
	//		var otherNode = obj as VisualDomElement;

	//		return otherNode != null ? 
	//			otherNode.dependencyObject.Equals(dependencyObject) : 
	//			false;
	//	}

	//	public override int GetHashCode()
	//	{
	//		return dependencyObject.GetHashCode();
	//	}
	//}
}

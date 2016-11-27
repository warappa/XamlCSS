using System;
using Xamarin.Forms;
using XamlCSS.Dom;

namespace XamlCSS.XamarinForms.Dom
{
	public class VisualDomElement : DomElement
	{
		public VisualDomElement(BindableObject dependencyObject, ITreeNodeProvider<BindableObject> treeNodeProvider)
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

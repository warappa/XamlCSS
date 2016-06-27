using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;
using Xamarin.Forms;
using XamlCSS.Dom;
using XamlCSS.Windows.Media;
using XamlCSS.XamarinForms.Dom;

namespace XamlCSS.XamarinForms
{
	public class TreeNodeProvider : ITreeNodeProvider<BindableObject>
	{
		public IEnumerable<BindableObject> GetChildren(BindableObject element)
		{
			return VisualTreeHelper.GetChildren(element as Element);
		}

		public IEnumerable<IDomElement<BindableObject>> GetChildren(IDomElement<BindableObject> node)
		{
			return this.GetChildren(node.Element as BindableObject)
				.Select(x => GetLogicalTree(x, node.Element as BindableObject))
				.ToArray();
		}

		public BindableObject GetParent(BindableObject tUIElement)
		{
			if (tUIElement is Element)
				return (tUIElement as Element).Parent;

			return null;
		}

		public IDomElement<BindableObject> GetLogicalTreeParent(BindableObject obj)
		{
			if (!(obj is Element) ||
				(obj as Element).Parent == null)
				return null;
			return new LogicalDomElement((obj as Element).Parent, GetLogicalTreeParent);
		}
		public IDomElement<BindableObject> GetLogicalTree(BindableObject obj, BindableObject parent)
		{
			return new LogicalDomElement(obj, GetLogicalTreeParent);
		}

		public IDomElement<BindableObject> GetVisualTreeParent(BindableObject obj)
		{
			if (!(obj is Element) ||
				(obj as Element).Parent == null)
				return null;
			return new VisualDomElement((obj as Element).Parent, GetVisualTreeParent);
		}
		public IDomElement<BindableObject> GetVisualTree(BindableObject obj, BindableObject parent)
		{
			return new VisualDomElement(obj, GetVisualTreeParent);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using XamlCSS.Dom;
using XamlCSS.WPF.Dom;

namespace XamlCSS.WPF
{
	public class TreeNodeProvider : ITreeNodeProvider<DependencyObject>
	{
		public IEnumerable<DependencyObject> GetChildren(DependencyObject element)
		{
			var list = new List<DependencyObject>();
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
			{
				var child = VisualTreeHelper.GetChild(element, i) as DependencyObject;

				if (child != null)
					list.Add(child);
			}

			return list;
		}

		public IEnumerable<IDomElement<DependencyObject>> GetChildren(IDomElement<DependencyObject> node)
		{
			return this.GetChildren(node.Element as DependencyObject)
				.Select(x => GetLogicalTree(x, node.Element as DependencyObject))
				.ToArray();
		}

		public DependencyObject GetParent(DependencyObject tUIElement)
		{
			if (tUIElement is FrameworkElement)
				return (tUIElement as FrameworkElement).Parent;
			else if (tUIElement is FrameworkContentElement)
				return (tUIElement as FrameworkContentElement).Parent;

			throw new InvalidOperationException("No parent found!");
		}

		public IDomElement<DependencyObject> GetLogicalTreeParent(DependencyObject obj)
		{
			if (obj is FrameworkElement &&
				(obj as FrameworkElement).Parent != null)
				return new LogicalDomElement((obj as FrameworkElement).Parent, GetLogicalTreeParent);
			if (obj is FrameworkContentElement &&
				(obj as FrameworkContentElement).Parent != null)
				return new LogicalDomElement((obj as FrameworkContentElement).Parent, GetLogicalTreeParent);
			return null;
		}
		public IDomElement<DependencyObject> GetLogicalTree(DependencyObject obj, DependencyObject parent)
		{
			return new LogicalDomElement(obj, GetLogicalTreeParent);
		}

		public IDomElement<DependencyObject> GetVisualTreeParent(DependencyObject obj)
		{
			if (obj is FrameworkElement)
				return new VisualDomElement((obj as FrameworkElement).Parent, GetVisualTreeParent);
			if (obj is FrameworkContentElement)
				return new VisualDomElement((obj as FrameworkContentElement).Parent, GetVisualTreeParent);
			return null;
		}
		public IDomElement<DependencyObject> GetVisualTree(DependencyObject obj, DependencyObject parent)
		{
			return new VisualDomElement(obj, GetVisualTreeParent);
		}
	}
}

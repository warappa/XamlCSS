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

		public IDomElement<DependencyObject> GetLogicalTree(DependencyObject obj, DependencyObject parent)
		{
			return new LogicalDomElement(obj, parent != null ? new LogicalDomElement(parent, null) : null);
		}

		public IDomElement<DependencyObject> GetVisualTree(DependencyObject obj, DependencyObject parent)
		{
			return new VisualDomElement(obj, parent != null ? new VisualDomElement(parent, null) : null);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using XamlCSS.Dom;
using XamlCSS.UWP.Dom;

namespace XamlCSS.UWP
{
	public class TreeNodeProvider : ITreeNodeProvider<DependencyObject>
	{
		public IEnumerable<DependencyObject> GetChildren(DependencyObject element)
		{
			var list = new List<DependencyObject>();

			var count = VisualTreeHelper.GetChildrenCount(element);
			for (int i = 0; i < count; i++)
			{
				var child = VisualTreeHelper.GetChild(element, i);

				list.Add(child);
			}

			return list;
		}

		public IEnumerable<IDomElement<DependencyObject>> GetChildren(IDomElement<DependencyObject> node)
		{
			return this.GetChildren(node.Element as DependencyObject)
				.Select(x => new LogicalDomElement(x, node as LogicalDomElement))
				.ToList();
		}

		public DependencyObject GetParent(DependencyObject tUIElement)
		{
            if (tUIElement is FrameworkElement)
            {
                return (tUIElement as FrameworkElement).Parent;
            }

			throw new InvalidOperationException("No parent found!");
		}

		public IDomElement<DependencyObject> GetLogicalTreeParent(DependencyObject obj)
		{
            if (!(obj is FrameworkElement) ||
                (obj as FrameworkElement).Parent == null)
            {
                return null;
            }

			return new LogicalDomElement((obj as FrameworkElement).Parent, GetLogicalTreeParent);
		}
		public IDomElement<DependencyObject> GetLogicalTree(DependencyObject obj, DependencyObject parent)
		{
			return new LogicalDomElement(obj, GetLogicalTreeParent);
		}

		public IDomElement<DependencyObject> GetVisualTreeParent(DependencyObject obj)
		{
			if (!(obj is FrameworkElement) ||
				(obj as FrameworkElement).Parent == null)
            { 
				return null;
            }

            return new VisualDomElement((obj as FrameworkElement).Parent, GetVisualTreeParent);
		}
		public IDomElement<DependencyObject> GetVisualTree(DependencyObject obj, DependencyObject parent)
		{
			return new VisualDomElement(obj, GetVisualTreeParent);
		}
	}
}

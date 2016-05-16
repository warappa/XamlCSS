using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace XamlCSS.Windows.Media
{
	public static class VisualTreeHelper
	{
		public static event EventHandler SubTreeAdded;
		public static event EventHandler SubTreeRemoved;

		private readonly static ConditionalWeakTable<Element, List<Element>> parentChildAssociations =
			new ConditionalWeakTable<Element, List<Element>>();

		private readonly static ConditionalWeakTable<Element, Element> childParentAssociations =
			new ConditionalWeakTable<Element, Element>();

		public static string PrintRealPath(Element e)
		{
			if (e == null)
				return "";
			return $".({e.GetType().Name} {e.Id}).{GetRealParent(e)}";
		}

		public static string GetRealParent(Element e)
		{
			var realParent = e.GetType().GetRuntimeProperties().Single(x => x.Name == "RealParent").GetValue(e) as Element;

			if (realParent == null)
				return $"(ROOT)";
			return GetRealParent(realParent) + $".({realParent.GetType().Name} {realParent.Id})";
		}

		public static void Initialize(Element root)
		{
			AttachedChild(root);
		}

		public static void Exclude(Element cell)
		{
			RemoveChildInternal(cell);
		}

		public static void Include(Element cell)
		{
			if (AttachedChild(cell))
				SubTreeAdded?.Invoke(cell, new EventArgs());
		}

		public static IEnumerable<Element> GetChildren(Element e)
		{
			List<Element> list = null;
			if (parentChildAssociations.TryGetValue(e, out list))
			{
				return list;
			}
			return Enumerable.Empty<Element>();
		}

		public static Element GetParent(Element e)
		{
			return e.Parent;
		}

		private static void Root_DescendantRemoved(object sender, ElementEventArgs e)
		{
			RemoveChildInternal(e.Element);
		}

		private static void Root_DescendantAdded(object sender, ElementEventArgs e)
		{
			AttachedChild(e.Element);
		}

		private static bool AttachedChild(Element child)
		{
			if (child == null)
				return false;
			Element dummy = null;
			if (childParentAssociations.TryGetValue(child, out dummy) == true)
				return false;

			if (child.Parent != null)
			{
				var p = child.Parent;

				List<Element> list = null;
				if (parentChildAssociations.TryGetValue(p, out list) == false)
				{
					list = new List<Element>();
					parentChildAssociations.Add(p, list);
				}
				list?.Add(child);
				try
				{
					childParentAssociations.Add(child, p);
				}
				catch { }
			}

			if (child is ViewCell)
			{
				var cell = child as ViewCell;

				AttachedChild(cell.View);
			}
			var layout = child as ILayoutController;
			if (layout != null)
			{
				foreach (var i in layout.Children)
					AttachedChild(i);
			}
			var contentPage = child as ContentPage;
			if (contentPage != null)
			{
				AttachedChild(contentPage.Content);
			}
			var app = child as Application;
			if (app != null)
				AttachedChild(app.MainPage);

			child.ChildAdded += ChildAddedHandler;
			child.ChildRemoved += ChildRemovedHandler;

			return true;
		}

		private static void ChildRemovedHandler(object sender, ElementEventArgs e)
		{
			RemoveChildInternal(e.Element);
		}
		private static void RemoveChildInternal(Element element)
		{
			if (CanUnattachChild(element))
			{
				SubTreeRemoved?.Invoke(element, new EventArgs());
				UnattachedChild(element);
			}
		}

		private static void ChildAddedHandler(object sender, ElementEventArgs e)
		{
			if (AttachedChild(e.Element))
				SubTreeAdded?.Invoke(e.Element, new EventArgs());
		}

		private static bool CanUnattachChild(Element child)
		{
			Element dummy = null;
			if (childParentAssociations.TryGetValue(child, out dummy) == false)
				return false;
			return true;
		}
		private static bool UnattachedChild(Element child)
		{
			Element dummy = null;
			if (childParentAssociations.TryGetValue(child, out dummy) == false)
				return false;

			child.ChildAdded -= ChildAddedHandler;
			child.ChildRemoved -= ChildRemovedHandler;

			if (child is ViewCell)
			{
				var cell = child as ViewCell;

				UnattachedChild(cell.View);
			}
			var layout = child as ILayoutController;
			if (layout != null)
			{
				foreach (var i in layout.Children)
					UnattachedChild(i);
			}

			Element parent = null;

			childParentAssociations.TryGetValue(child, out parent);
			if (parent == null)
				parent = child.Parent;
			childParentAssociations.Remove(child);

			if (parent != null)
			{
				List<Element> list = null;
				if (parentChildAssociations.TryGetValue(parent, out list) == false)
				{
					return true;
				}
				list.Remove(child);
				if (list.Count == 0)
					parentChildAssociations.Remove(parent);
			}

			return true;
		}
	}
}

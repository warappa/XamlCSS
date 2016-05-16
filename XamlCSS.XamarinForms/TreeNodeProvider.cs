using System.Collections.Generic;
using System.Linq;
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
			if (tUIElement is VisualElement)
				return (tUIElement as VisualElement).Parent;

			return null;
		}

		public IDomElement<BindableObject> GetLogicalTree(BindableObject obj, BindableObject parent)
		{
			return new LogicalDomElement(obj, parent != null ? new LogicalDomElement(parent, null) : null);
		}

		public IDomElement<BindableObject> GetVisualTree(BindableObject obj, BindableObject parent)
		{
			return new VisualDomElement(obj, parent != null ? new VisualDomElement(parent, null) : null);
		}
	}
}

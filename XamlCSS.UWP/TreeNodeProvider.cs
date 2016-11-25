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
        readonly IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyPropertyService;

        public TreeNodeProvider(IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyPropertyService)
        {
            this.dependencyPropertyService = dependencyPropertyService;
        }

        public IEnumerable<DependencyObject> GetChildren(DependencyObject element)
		{
			var list = new List<DependencyObject>();

            try
            {
                var count = VisualTreeHelper.GetChildrenCount(element);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(element, i);

                    list.Add(child);
                }
            }
            catch
            {
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

            return null;
		}

		public IDomElement<DependencyObject> GetLogicalTreeParent(DependencyObject obj)
		{
            return GetLogicalTree(GetParent(obj));
        }
		public IDomElement<DependencyObject> GetLogicalTree(DependencyObject obj)
		{
            if (obj == null)
            {
                return null;
            }

            var cached = GetFromDependencyObject(obj);

            if (cached != null &&
                cached is LogicalDomElement)
            {
                return cached;
            }

            cached = new LogicalDomElement(obj, GetLogicalTreeParent);
            dependencyPropertyService.SetDomElement(obj, cached);

            return cached;
        }

		public IDomElement<DependencyObject> GetVisualTreeParent(DependencyObject obj)
		{
            return GetVisualTree(GetParent(obj));
        }
		public IDomElement<DependencyObject> GetVisualTree(DependencyObject obj)
		{
            if (obj == null)
            {
                return null;
            }

            var cached = GetFromDependencyObject(obj);

            if (cached != null &&
                cached is VisualDomElement)
            {
                return cached;
            }

            cached = new VisualDomElement(obj, GetVisualTreeParent);
            dependencyPropertyService.SetDomElement(obj, cached);

            return cached;
        }

        private IDomElement<DependencyObject> GetFromDependencyObject(DependencyObject obj)
        {
            return dependencyPropertyService.GetDomElement(obj);
        }
    }
}

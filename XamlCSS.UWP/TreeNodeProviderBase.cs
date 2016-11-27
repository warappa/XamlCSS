using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using XamlCSS.Dom;
using XamlCSS.UWP.Dom;

namespace XamlCSS.UWP
{
    public abstract class TreeNodeProviderBase : ITreeNodeProvider<DependencyObject>
	{
        readonly IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyPropertyService;

        public TreeNodeProviderBase(IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyPropertyService)
        {
            this.dependencyPropertyService = dependencyPropertyService;
        }

        protected abstract IDomElement<DependencyObject> CreateTreeNode(DependencyObject dependencyObject);

        protected abstract bool IsCorrectTreeNode(IDomElement<DependencyObject> node);

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

		public IEnumerable<IDomElement<DependencyObject>> GetDomElementChildren(IDomElement<DependencyObject> node)
		{
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Element == null) throw new ArgumentNullException(nameof(node.Element));

            return this.GetChildren(node.Element as DependencyObject)
				.Select(x => new LogicalDomElement(x, this))
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

		public IDomElement<DependencyObject> GetTreeParentNode(DependencyObject obj)
		{
            return GetDomElement(GetParent(obj));
        }
		public IDomElement<DependencyObject> GetDomElement(DependencyObject obj)
		{
            if (obj == null)
            {
                return null;
            }

            var cached = GetFromDependencyObject(obj);

            if (cached != null &&
                IsCorrectTreeNode(cached))
            {
                return cached;
            }

            cached = CreateTreeNode(obj);
            dependencyPropertyService.SetDomElement(obj, cached);

            return cached;
        }

        private IDomElement<DependencyObject> GetFromDependencyObject(DependencyObject obj)
        {
            return dependencyPropertyService.GetDomElement(obj);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using XamlCSS.Dom;
using XamlCSS.Windows.Media;

namespace XamlCSS.XamarinForms.Dom
{
    public class TreeNodeProvider : TreeNodeProviderBase<BindableObject, Style, BindableProperty>
    {
        public TreeNodeProvider(IDependencyPropertyService<BindableObject, Style, BindableProperty> BindablePropertyService)
            : base(BindablePropertyService)
        {
        }

        public override IDomElement<BindableObject> CreateTreeNode(BindableObject BindableObject)
        {
            return new DomElement(BindableObject, GetDomElement(GetParent(BindableObject, SelectorType.VisualTree)), GetDomElement(GetParent(BindableObject, SelectorType.LogicalTree)), this);
        }

        private List<BindableObject> GetLogicalChildren(BindableObject parent, BindableObject currentChild)
        {
            var listFound = new List<BindableObject>();
            var listToCheckFurther = new List<BindableObject>();

            var children = VisualTreeHelper.GetChildren(currentChild as Element).ToList();
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                var childsParent = GetParent(child, SelectorType.LogicalTree);
                if (childsParent == parent)
                {
                    listFound.Add(child);
                }
                else if(childsParent != null)
                {
                    listToCheckFurther.Add(child);
                }
            }
            foreach (var item in listToCheckFurther)
            {
                listFound.AddRange(GetLogicalChildren(parent, item));
            }

            return listFound;
        }

        public override IEnumerable<BindableObject> GetChildren(BindableObject element, SelectorType type)
        {
            var list = new List<BindableObject>();

            if (type == SelectorType.LogicalTree)
            {
                try
                {
                    list = GetLogicalChildren(element, element);
                }
                catch
                {
                }
            }
            else
            {
                list = GetVisualChildren(element).ToList();
            }

            return list;
        }
        public IEnumerable<BindableObject> GetVisualChildren(BindableObject element)
        {
            return VisualTreeHelper.GetChildren(element as Element);
        }

        public override BindableObject GetParent(BindableObject element, SelectorType type)
        {
            if (type == SelectorType.LogicalTree)
            {
                return (element as Element)?.Parent;
            }
            else
            {
                return GetVisualParent(element);
            }
        }

        private BindableObject GetVisualParent(BindableObject element)
        {
            if (element == null)
            {
                return null;
            }

            return VisualTreeHelper.GetParent(element as Element);
        }

        public override bool IsInTree(BindableObject dependencyObject, SelectorType type)
        {
            return true;
        }
    }
}

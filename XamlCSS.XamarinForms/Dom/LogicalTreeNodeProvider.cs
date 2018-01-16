using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using XamlCSS.Dom;
using XamlCSS.Windows.Media;

namespace XamlCSS.XamarinForms.Dom
{
    public class LogicalTreeNodeProvider : TreeNodeProviderBase<BindableObject, Style, BindableProperty>, ISwitchableTreeNodeProvider<BindableObject>
    {
        public SelectorType CurrentSelectorType => SelectorType.LogicalTree;

        public LogicalTreeNodeProvider(IDependencyPropertyService<BindableObject, Style, BindableProperty> BindablePropertyService)
            : base(BindablePropertyService, SelectorType.LogicalTree)
        {
        }

        public override IDomElement<BindableObject> CreateTreeNode(BindableObject BindableObject)
        {
            return new LogicalDomElement(BindableObject, GetDomElement(GetParent(BindableObject)), this, namespaceProvider);
        }

        public override bool IsCorrectTreeNode(IDomElement<BindableObject> node)
        {
            return node is LogicalDomElement;
        }

        private List<BindableObject> GetLogicalChildren(BindableObject parent, BindableObject currentChild)
        {
            var listFound = new List<BindableObject>();
            var listToCheckFurther = new List<BindableObject>();

            var children = VisualTreeHelper.GetChildren(currentChild as Element).ToList();
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                var childsParent = GetParent(child);
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

        public override IEnumerable<BindableObject> GetChildren(BindableObject element)
        {
            var list = new List<BindableObject>();

            try
            {
                list = GetLogicalChildren(element, element);
            }
            catch
            {
            }

            return list;
        }

        public override BindableObject GetParent(BindableObject element)
        {
            return (element as Element)?.Parent;
        }
        
        public override bool IsInTree(BindableObject dependencyObject)
        {
            return true;
        }

        public void Switch(SelectorType type)
        {
            
        }
    }
}

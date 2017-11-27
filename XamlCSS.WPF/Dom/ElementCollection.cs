using System.Collections.Generic;
using System.Windows;
using AngleSharp.Dom;
using XamlCSS.Dom;
using System.Linq;
using System.Windows.Controls;

namespace XamlCSS.WPF.Dom
{
    public class ElementCollection : ElementCollectionBase<DependencyObject>
    {
        public ElementCollection(IDomElement<DependencyObject> node, ITreeNodeProvider<DependencyObject> treeNodeProvider)
            : base(node, treeNodeProvider)
        {

        }

        public ElementCollection(IEnumerable<IElement> elements, ITreeNodeProvider<DependencyObject> treeNodeProvider)
            : base(elements, treeNodeProvider)
        {

        }

        protected override IElement CreateElement(DependencyObject dependencyObject, IDomElement<DependencyObject> parentNode)
        {
            return treeNodeProvider.GetDomElement(dependencyObject);
        }
        protected override IEnumerable<DependencyObject> GetChildren(DependencyObject dependencyObject)
        {
            return treeNodeProvider.GetChildren(dependencyObject);
        }
        protected override string GetId(DependencyObject dependencyObject)
        {
            if (dependencyObject is FrameworkElement)
            {
                return dependencyObject.ReadLocalValue(FrameworkElement.NameProperty) as string;
            }

            return dependencyObject.ReadLocalValue(FrameworkContentElement.NameProperty) as string;
        }
    }
}

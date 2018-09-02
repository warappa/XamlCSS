using System.Collections.Generic;

namespace XamlCSS.Dom
{
    public interface ITreeNodeProvider<TDependencyObject, TDependencyProperty>
        where TDependencyObject : class
    {
        IDomElement<TDependencyObject, TDependencyProperty> GetDomElement(TDependencyObject obj);
        IEnumerable<IDomElement<TDependencyObject, TDependencyProperty>> GetDomElementChildren(IDomElement<TDependencyObject, TDependencyProperty> node, SelectorType type);
        IEnumerable<TDependencyObject> GetChildren(TDependencyObject element, SelectorType type);
        TDependencyObject GetParent(TDependencyObject dependencyObject, SelectorType type);
        bool IsInTree(TDependencyObject dependencyObject, SelectorType type);
        bool IsTopMost(TDependencyObject dependencyObject, SelectorType type);
        IDomElement<TDependencyObject, TDependencyProperty> CreateTreeNode(TDependencyObject dependencyObject);
    }
}

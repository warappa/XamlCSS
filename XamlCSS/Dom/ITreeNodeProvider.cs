using System.Collections.Generic;

namespace XamlCSS.Dom
{
    public interface ITreeNodeProvider<TDependencyObject>
        where TDependencyObject : class
    {
        IDomElement<TDependencyObject> GetDomElement(TDependencyObject obj);
        IEnumerable<IDomElement<TDependencyObject>> GetDomElementChildren(IDomElement<TDependencyObject> node, SelectorType type);
        IEnumerable<TDependencyObject> GetChildren(TDependencyObject element, SelectorType type);
        TDependencyObject GetParent(TDependencyObject dependencyObject, SelectorType type);
        bool IsInTree(TDependencyObject dependencyObject, SelectorType type);
        IDomElement<TDependencyObject> CreateTreeNode(TDependencyObject dependencyObject);
    }
    public interface ISwitchableTreeNodeProvider<TDependencyObject> : ITreeNodeProvider<TDependencyObject>
        where TDependencyObject : class
    {
        void Switch(SelectorType type);
        SelectorType CurrentSelectorType { get; }
    }
}

using System.Collections.Generic;

namespace XamlCSS.Dom
{
    public interface ITreeNodeProvider<TDependencyObject>
        where TDependencyObject : class
    {
        IDomElement<TDependencyObject> GetDomElement(TDependencyObject obj);
        IEnumerable<IDomElement<TDependencyObject>> GetDomElementChildren(IDomElement<TDependencyObject> node);
        IEnumerable<TDependencyObject> GetChildren(TDependencyObject element);
        TDependencyObject GetParent(TDependencyObject dependencyObject);
        bool IsInTree(TDependencyObject dependencyObject);
        IDomElement<TDependencyObject> CreateTreeNode(TDependencyObject dependencyObject);
        bool IsCorrectTreeNode(IDomElement<TDependencyObject> node);
    }
    public interface ISwitchableTreeNodeProvider<TDependencyObject> : ITreeNodeProvider<TDependencyObject>
        where TDependencyObject : class
    {
        void Switch(SelectorType type);
        SelectorType CurrentSelectorType { get; }
    }
}

using System;
using System.Collections.Generic;

namespace XamlCSS.Dom
{
    public interface IDomElement<TDependencyObject, TDependencyProperty> : IDisposable
    {
        TDependencyObject Element { get; }
        IList<StyleSheet> XamlCssStyleSheets { get; }
        IList<IDomElement<TDependencyObject, TDependencyProperty>> QuerySelectorAllWithSelf(StyleSheet styleSheet, ISelector selector, SelectorType type);
        IDomElement<TDependencyObject, TDependencyProperty> LogicalParent { get; }
        IDomElement<TDependencyObject, TDependencyProperty> Parent { get; }
        string Id { get; }
        HashSet<string> ClassList { get; }
        IList<IDomElement<TDependencyObject, TDependencyProperty>> LogicalChildNodes { get; }
        IList<IDomElement<TDependencyObject, TDependencyProperty>> ChildNodes { get; }
        string AssemblyQualifiedNamespaceName { get; }
        string TagName { get; }
        IDictionary<string, TDependencyProperty> Attributes { get; }
        bool HasAttribute(string attribute);
        object GetAttributeValue(TDependencyProperty dependencyProperty);

        bool IsInLogicalTree { get; }
        bool IsInVisualTree { get; }
        bool IsReady { get; }

        StyleUpdateInfo StyleInfo { get; set; }

        void EnsureAttributeWatcher(TDependencyProperty dependencyProperty);
        void ClearAttributeWatcher();
    }
}

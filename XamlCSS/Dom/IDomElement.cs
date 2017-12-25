using System;
using System.Collections.Generic;

namespace XamlCSS.Dom
{
    public interface IDomElement<TDependencyObject> : IDisposable
    {
        TDependencyObject Element { get; }
        IList<StyleSheet> XamlCssStyleSheets { get; }
        IDomElement<TDependencyObject> QuerySelectorWithSelf(string selectors);
        IList<IDomElement<TDependencyObject>> QuerySelectorAllWithSelf(string selectors);
        IDomElement<TDependencyObject> Parent { get; }
        string TagName { get; }
        string Id { get; }
        IList<string> ClassList { get; }
        IList<IDomElement<TDependencyObject>> ChildNodes { get; }
        string NamespaceUri { get; }
        string LocalName { get; }

        string LookupPrefix(string v);
        string LookupNamespaceUri(string v);
    }
}

using XamlCSS.Dom;

namespace XamlCSS
{
    public interface ISelector
    {
        string Value { get; }
        string Specificity { get; }
        int IdSpecificity { get; }
        int ClassSpecificity { get; }
        int SimpleSpecificity { get; }

        bool Match<TDependencyObject>(StyleSheet styleSheet, IDomElement<TDependencyObject> domElement)
            where TDependencyObject : class;
    }
}
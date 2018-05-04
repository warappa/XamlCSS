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

        MatchResult Match<TDependencyObject, TDependencyProperty>(StyleSheet styleSheet, IDomElement<TDependencyObject, TDependencyProperty> domElement)
            where TDependencyObject : class;

        bool StartOnVisualTree();
    }
}

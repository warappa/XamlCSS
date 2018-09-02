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

        MatchResult Match<TDependencyObject, TDependencyProperty>(StyleSheet styleSheet, IDomElement<TDependencyObject, TDependencyProperty> domElement, int startGroupIndex, int endGroupIndex)
            where TDependencyObject : class;

        bool StartOnVisualTree();
        int GroupCount { get; }
    }
}

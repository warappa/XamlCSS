using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class ClasssSelector : SelectorFragment
    {
        public ClasssSelector(CssNodeType type, string text) : base(type, text)
        {
            Text = text?.Substring(1);
        }

        public override bool Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, SelectorFragment[] fragments, ref int currentIndex)
        {
            return domElement.ClassList.Contains(Text);
        }
    }
}
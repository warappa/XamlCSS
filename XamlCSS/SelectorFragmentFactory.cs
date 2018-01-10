using XamlCSS.CssParsing;

namespace XamlCSS
{
    public class SelectorFragmentFactory
    {
        public SelectorFragment Create(CssNodeType type, string text)
        {
            if (type == CssNodeType.TypeSelector)
            {
                return new TypeSelector(type, text);
            }
            else if (type == CssNodeType.ClassSelector)
            {
                return new ClassSelector(type, text);
            }
            else if (type == CssNodeType.IdSelector)
            {
                return new IdSelector(type, text);
            }
            else if (type == CssNodeType.UniversalSelector)
            {
                return new UnivseralSelector(type, text);
            }
            return new SelectorFragment(type, text);
        }
    }
}
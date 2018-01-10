using System;
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
            else if (type == CssNodeType.PseudoSelector)
            {
                if (text.StartsWith(":nth-child", StringComparison.Ordinal))
                {
                    return new NthChildSelector(CssNodeType.PseudoSelector, text);
                }
                else if (text.StartsWith(":nth-of-type", StringComparison.Ordinal))
                {
                    return new NthOfTypeSelector(CssNodeType.PseudoSelector, text);
                }
                else if (text.StartsWith(":nth-last-child", StringComparison.Ordinal))
                {
                    return new NthLastChildSelector(CssNodeType.PseudoSelector, text);
                }
                else if (text.StartsWith(":nth-last-of-type", StringComparison.Ordinal))
                {
                    return new NthLastOfTypeSelector(CssNodeType.PseudoSelector, text);
                }
            }

            return new SelectorFragment(type, text);
        }
    }
}
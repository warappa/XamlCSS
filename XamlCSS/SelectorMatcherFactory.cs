using System;
using XamlCSS.CssParsing;

namespace XamlCSS
{
    public class SelectorMatcherFactory
    {
        public SelectorMatcher Create(CssNodeType type, string text)
        {
            if (type == CssNodeType.TypeSelector)
            {
                return new TypeMatcher(type, text);
            }
            else if (type == CssNodeType.ClassSelector)
            {
                return new ClassMatcher(type, text);
            }
            else if (type == CssNodeType.IdSelector)
            {
                return new IdMatcher(type, text);
            }
            else if (type == CssNodeType.UniversalSelector)
            {
                return new UnivseralMatcher(type, text);
            }
            else if (type == CssNodeType.PseudoSelector)
            {
                if (text.StartsWith(":nth-child", StringComparison.Ordinal))
                {
                    return new NthChildMatcher(CssNodeType.PseudoSelector, text);
                }
                else if (text.StartsWith(":nth-of-type", StringComparison.Ordinal))
                {
                    return new NthOfTypeMatcher(CssNodeType.PseudoSelector, text);
                }
                else if (text.StartsWith(":nth-last-child", StringComparison.Ordinal))
                {
                    return new NthLastChildMatcher(CssNodeType.PseudoSelector, text);
                }
                else if (text.StartsWith(":nth-last-of-type", StringComparison.Ordinal))
                {
                    return new NthLastOfTypeMatcher(CssNodeType.PseudoSelector, text);
                }
                else if (text.StartsWith(":first-child", StringComparison.Ordinal))
                {
                    return new FirstChildMatcher(CssNodeType.PseudoSelector, text);
                }
                else if (text.StartsWith(":last-child", StringComparison.Ordinal))
                {
                    return new LastChildMatcher(CssNodeType.PseudoSelector, text);
                }
                else if (text.StartsWith(":first-of-type", StringComparison.Ordinal))
                {
                    return new FirstOfTypeMatcher(CssNodeType.PseudoSelector, text);
                }
                else if (text.StartsWith(":last-of-type", StringComparison.Ordinal))
                {
                    return new LastOfTypeMatcher(CssNodeType.PseudoSelector, text);
                }
                else if (text.StartsWith(":only-of-type", StringComparison.Ordinal))
                {
                    return new OnlyOfTypeMatcher(CssNodeType.PseudoSelector, text);
                }
            }
            else if (type == CssNodeType.InheritedTypeSelector)
            {
                return new InheritedTypeMatcher(type, text);
            }

            return new SelectorMatcher(type, text);
        }
    }
}
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    public static class SassStyleTestExtensions
    {
        public static CssNode GetRootStyleRuleNode(this CssNode node, int nthRule = 0)
        {
            if (node == null)
            {
                return null;
            }

            if (node.Type == CssNodeType.Document)
            {
                node = node.Children.ToList()[nthRule];
            }

            return node;
        }

        public static CssNode GetSubStyleRuleNode(this CssNode node, int nthRule = 0)
        {
            if (node == null)
            {
                return null;
            }

            if (node.Type == CssNodeType.StyleRule)
            {
                node = node.Children
                    .Where(x => x.Type == CssNodeType.StyleDeclarationBlock)
                    .FirstOrDefault();
            }

            if (node?.Type == CssNodeType.StyleDeclarationBlock)
            {
                node = node.Children
                    .Where(x => x.Type == CssNodeType.StyleRule)
                    .Skip(nthRule)
                    .FirstOrDefault();
            }

            return node;
        }

        public static CssNode GetSelectorNode(this CssNode node, int nthRule = 0, int nthSelector = 0, int nthSelectorFragment = 0)
        {
            if (node.Type == CssNodeType.Document)
            {
                node = node.Children.ToList()[nthRule];
            }
            if (node.Type == CssNodeType.StyleRule)
            {
                node = node.Children.ToList()[nthSelector];
            }
            if (node.Type == CssNodeType.Selectors)
            {
                node = node.Children.ToList()[0];
            }
            if (node.Type == CssNodeType.Selector)
            {
                node = node.Children.ToList()[nthSelectorFragment];
            }

            return node;
        }
    }
}

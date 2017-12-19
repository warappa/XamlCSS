using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class MergedStyleSheetTests
    {
        [Test]
        public void MergeStyleSheet_merges_other_StyleSheets_rules_with_own_rules()
        {
            var styleSheetA = CssParser.Parse(@"
.header
{
    BackgroundColor: Black;
    TextColor: Blue;
}");

            var styleSheetB = CssParser.Parse(@"
.header
{
    BackgroundColor: White;
}");

            var mergedStyleSheet = new StyleSheet()
            {
                Content = @"
.header
{
    Padding: 15, 15, 15, 15;
}
"
            };

            mergedStyleSheet.Rules.Count.Should().Be(1);
            mergedStyleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("Padding");
            mergedStyleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("15, 15, 15, 15");

            mergedStyleSheet.BaseStyleSheets = new StyleSheetCollection(new[] { styleSheetA, styleSheetB });

            mergedStyleSheet.Rules.Count.Should().Be(1);
            mergedStyleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            mergedStyleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("White");

            mergedStyleSheet.Rules[0].DeclarationBlock[1].Property.Should().Be("TextColor");
            mergedStyleSheet.Rules[0].DeclarationBlock[1].Value.Should().Be("Blue");

            mergedStyleSheet.Rules[0].DeclarationBlock[2].Property.Should().Be("Padding");
            mergedStyleSheet.Rules[0].DeclarationBlock[2].Value.Should().Be("15, 15, 15, 15");
        }

        [Test]
        public void MergeStyleSheet_merges_other_namespaces_with_own()
        {
            var styleSheetA = CssParser.Parse(@"
@namespace alias ""styleSheetA"";
@namespace secondAlias ""styleSheetA"";");

            var styleSheetB = CssParser.Parse(@"@namespace alias ""styleSheetB"";");

            var mergedStyleSheet = new StyleSheet()
            {
                Content = @"@namespace thirdAlias ""mergedStyleSheet"";"
            };

            mergedStyleSheet.Namespaces[0].Alias.Should().Be("thirdAlias");
            mergedStyleSheet.Namespaces[0].Namespace.Should().Be("mergedStyleSheet");

            mergedStyleSheet.BaseStyleSheets = new StyleSheetCollection(new[] { styleSheetA, styleSheetB });

            mergedStyleSheet.Rules.Count.Should().Be(0);
            mergedStyleSheet.Namespaces[0].Alias.Should().Be("alias");
            mergedStyleSheet.Namespaces[0].Namespace.Should().Be("styleSheetB");
            mergedStyleSheet.Namespaces[1].Alias.Should().Be("secondAlias");
            mergedStyleSheet.Namespaces[1].Namespace.Should().Be("styleSheetA");
            mergedStyleSheet.Namespaces[2].Alias.Should().Be("thirdAlias");
            mergedStyleSheet.Namespaces[2].Namespace.Should().Be("mergedStyleSheet");
        }

        [Test]
        public void StyleSheet_merges_parent_StyleSheets_rules_with_own_rules()
        {
            var parentRoot = new object();
            var parentFirstLevel = new object();
            var currentNode = new object();

            var styleSheetA = CssParser.Parse(@"
.header
{
    BackgroundColor: Black;
    TextColor: Blue;
}");

            var styleSheetB = CssParser.Parse(@"
.header
{
    BackgroundColor: White;
}");

            var mergedStyleSheet = new StyleSheet()
            {
                Content = @"
.header
{
    Padding: 15, 15, 15, 15;
}
"
            };

            StyleSheet.GetParent = (obj) =>
            {
                if (obj == currentNode)
                {
                    return parentFirstLevel;
                }
                else if (obj == parentFirstLevel)
                {
                    return parentRoot;
                }

                return null;
            };

            StyleSheet.GetStyleSheet = (obj) =>
            {
                if (obj == parentFirstLevel)
                {
                    return styleSheetB;
                }
                else if (obj == parentRoot)
                {
                    return styleSheetA;
                }

                return null;
            };

            mergedStyleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("Padding");
            mergedStyleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("15, 15, 15, 15");

            mergedStyleSheet.AttachedTo = currentNode;

            mergedStyleSheet.Rules.Count.Should().Be(1);
            mergedStyleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            mergedStyleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("White");

            mergedStyleSheet.Rules[0].DeclarationBlock[1].Property.Should().Be("TextColor");
            mergedStyleSheet.Rules[0].DeclarationBlock[1].Value.Should().Be("Blue");

            mergedStyleSheet.Rules[0].DeclarationBlock[2].Property.Should().Be("Padding");
            mergedStyleSheet.Rules[0].DeclarationBlock[2].Value.Should().Be("15, 15, 15, 15");
        }

        [Test]
        public void StyleSheet_merges_other_namespaces_with_own()
        {
            var parentRoot = new object();
            var parentFirstLevel = new object();
            var currentNode = new object();

            var styleSheetA = CssParser.Parse(@"
@namespace alias ""styleSheetA"";
@namespace secondAlias ""styleSheetA"";");

            var styleSheetB = CssParser.Parse(@"@namespace alias ""styleSheetB"";");

            StyleSheet.GetParent = (obj) =>
            {
                if (obj == currentNode)
                {
                    return parentFirstLevel;
                }
                else if (obj == parentFirstLevel)
                {
                    return parentRoot;
                }

                return null;
            };

            StyleSheet.GetStyleSheet = (obj) =>
            {
                if (obj == parentFirstLevel)
                {
                    return styleSheetB;
                }
                else if (obj == parentRoot)
                {
                    return styleSheetA;
                }

                return null;
            };

            var mergedStyleSheet = new StyleSheet()
            {
                Content = @"@namespace thirdAlias ""mergedStyleSheet"";"
            };

            mergedStyleSheet.Namespaces[0].Alias.Should().Be("thirdAlias");
            mergedStyleSheet.Namespaces[0].Namespace.Should().Be("mergedStyleSheet");

            mergedStyleSheet.AttachedTo = currentNode;

            mergedStyleSheet.Rules.Count.Should().Be(0);
            mergedStyleSheet.Namespaces[0].Alias.Should().Be("alias");
            mergedStyleSheet.Namespaces[0].Namespace.Should().Be("styleSheetB");
            mergedStyleSheet.Namespaces[1].Alias.Should().Be("secondAlias");
            mergedStyleSheet.Namespaces[1].Namespace.Should().Be("styleSheetA");
            mergedStyleSheet.Namespaces[2].Alias.Should().Be("thirdAlias");
            mergedStyleSheet.Namespaces[2].Namespace.Should().Be("mergedStyleSheet");
        }
    }
}

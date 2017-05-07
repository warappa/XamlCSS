using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class AstErrorTests
    {
        [Test]
        public void Invalid_namespace_declaration_adds_error()
        {
            string css = @"
@namespace XamlCss

.class {

}
";
            var generator = new AstGenerator();
            var result = generator.GetAst(css);
            var doc = result.Root;

            result.Errors.Count.Should().Be(1);
            result.Errors[0].Message.Should().Contain("got 'Whitespace'");
            result.Errors[0].Line.Should().Be(2);
            result.Errors[0].Column.Should().Be(19);

            var node = doc.Children.FirstOrDefault(x => x.Type == CssNodeType.StyleRule)
                ?.Children.FirstOrDefault(x => x.Type == CssNodeType.Selectors)
                ?.Children.FirstOrDefault(x => x.Type == CssNodeType.Selector)
                ?.Children.FirstOrDefault(x => x.Type == CssNodeType.SelectorFragment)
                ?.Text.Should().Be(".class");

            Assert.NotNull(node);
        }

        [Test]
        public void Invalid_styledeclarationblock_declaration_adds_error()
        {
            string css = @"
.class {
asdfasdfasdf#
background: red;
}

.class2 {
}
";
            var generator = new AstGenerator();
            var result = generator.GetAst(css);
            var doc = result.Root;

            result.Errors.Count.Should().Be(1);
            result.Errors[0].Message.Should().Contain("was 'Whitespace'");
            result.Errors[0].Line.Should().Be(3);
            result.Errors[0].Column.Should().Be(14);


            // faulty style-declaration removed, good one added
            doc.GetStyleRule(0).GetStyleDeclaration("background")
                .GetValue().Should().Be("red");

            // good style-rule parsed and added properly
            doc.GetStyleRule(1).GetSelector(".class2")
                .Should().NotBeNull();
        }

        [Test]
        public void Invalid_styledeclarationblock_declaration_adds_error_2()
        {
            string css = @"
Button {
Width: ;
background: red;
}

.class2 {
}
";
            var generator = new AstGenerator();
            var result = generator.GetAst(css);
            var doc = result.Root;

            result.Errors.Count.Should().Be(1);
            result.Errors[0].Message.Should().Contain("No value for key");
            result.Errors[0].Line.Should().Be(3);
            result.Errors[0].Column.Should().Be(8);


            // faulty style-declaration removed, good one added
            doc.GetStyleRule(0).GetStyleDeclaration("background")
                .GetValue().Should().Be("red");

            // good style-rule parsed and added properly
            doc.GetStyleRule(1).GetSelector(".class2")
                .Should().NotBeNull();
        }


    }

    public static class AstTestExtensions
    {
        public static CssNode GetStyleRule(this CssNode document, int index)
        {
            if (document.Type != CssNodeType.Document)
            {
                throw new System.Exception("No document node!");
            }

            return document.Children.Where(x => x.Type == CssNodeType.StyleRule)
                .ToList()[index];
        }
        
        public static CssNode GetStyleDeclaration(this CssNode styleRule, int index)
        {
            if (styleRule.Type != CssNodeType.StyleRule)
            {
                throw new System.Exception("No style-rule node!");
            }

            return styleRule
                .Children.FirstOrDefault(x => x.Type == CssNodeType.StyleDeclarationBlock)
                .Children.Where(x => x.Type == CssNodeType.StyleDeclaration)
                .ToList()[index];
        }

        public static CssNode GetSelector(this CssNode styleRule, string selectorFragment)
        {
            if (styleRule.Type != CssNodeType.StyleRule)
            {
                throw new System.Exception("No style-rule node!");
            }

            return styleRule
                .Children.FirstOrDefault(x => x.Type == CssNodeType.Selectors)
                .Children.FirstOrDefault(x => x.Type == CssNodeType.Selector &&
                    x.Children.Any(y => y.Type == CssNodeType.SelectorFragment &&
                    y.Text == selectorFragment))
                .Children.FirstOrDefault(y => y.Type == CssNodeType.SelectorFragment &&
                    y.Text == selectorFragment);
        }

        public static CssNode GetStyleDeclaration(this CssNode styleRule, string key)
        {
            if (styleRule.Type != CssNodeType.StyleRule)
            {
                throw new System.Exception("No style-rule node!");
            }

            return styleRule
                .Children.FirstOrDefault(x => x.Type == CssNodeType.StyleDeclarationBlock)
                .Children.Where(x => 
                    x.Type == CssNodeType.StyleDeclaration &&
                    x.Children.Any(y => y.Type == CssNodeType.Key && y.Text == key))
                .FirstOrDefault();
        }

        public static string GetValue(this CssNode styleDeclaration)
        {
            if (styleDeclaration.Type != CssNodeType.StyleDeclaration)
            {
                throw new System.Exception("No style-declaration node!");
            }

            return styleDeclaration
                .Children.First(x => x.Type == CssNodeType.Value)
                .Text;
        }
    }
}

using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class AstGeneratorTests
    {
        string css = @"
@namespace xamlcss ""XamlCss"";
.main .sub>div xamlcss|Button {
	background-color: red;
	background: #00ff00, solid, url('aaa');
	Grid.Row: 1;
}
";
        [Test]
        public void Can_generate_ast()
        {
            var doc = new AstGenerator().GetAst(css).Root;

            var mainClassSelector = doc.Children.FirstOrDefault(x => x.Type == CssNodeType.StyleRule)
                ?.Children.FirstOrDefault(x => x.Type == CssNodeType.Selectors)
                ?.Children.FirstOrDefault(x => x.Type == CssNodeType.Selector)
                ?.Children.FirstOrDefault(x => x.Type == CssNodeType.SimpleSelectorSequence)
                ?.Children.FirstOrDefault(x => x.Type == CssNodeType.ClassSelector);

            mainClassSelector.Should().NotBeNull();
            mainClassSelector.Text.Should().Be(".main");

            var children = doc.Children.FirstOrDefault(x => x.Type == CssNodeType.StyleRule)
                ?.Children.FirstOrDefault(x => x.Type == CssNodeType.Selectors)
                ?.Children.FirstOrDefault(x => x.Type == CssNodeType.Selector)
                ?.Children.ToList();

            children.Should().NotBeNull();

            children[0].Type.Should().Be(CssNodeType.SimpleSelectorSequence);
            children[0].Children.ToList()[0].Type.Should().Be(CssNodeType.ClassSelector);
            children[0].Children.ToList()[0].Text.Should().Be(".main");

            children[1].Type.Should().Be(CssNodeType.GeneralDescendantCombinator);
            children[1].Children.Should().BeEmpty();

            children[2].Type.Should().Be(CssNodeType.SimpleSelectorSequence);
            children[2].Children.ToList()[0].Type.Should().Be(CssNodeType.ClassSelector);
            children[2].Children.ToList()[0].Text.Should().Be(".sub");

            children[3].Type.Should().Be(CssNodeType.DirectDescendantCombinator);
            children[3].Children.Should().BeEmpty();
            
            children[4].Type.Should().Be(CssNodeType.SimpleSelectorSequence);
            children[4].Children.ToList()[0].Type.Should().Be(CssNodeType.TypeSelector);
            children[4].Children.ToList()[0].Text.Should().Be("div");

            children[5].Type.Should().Be(CssNodeType.GeneralDescendantCombinator);
            children[5].Children.Should().BeEmpty();

            children[6].Type.Should().Be(CssNodeType.SimpleSelectorSequence);
            children[6].Children.ToList()[0].Type.Should().Be(CssNodeType.TypeSelector);
            children[6].Children.ToList()[0].Text.Should().Be("xamlcss|Button");

            children.Count.Should().Be(7);

            var node = doc.Children.FirstOrDefault(x => x.Type == CssNodeType.StyleRule)
                ?.Children.FirstOrDefault(x => x.Type == CssNodeType.StyleDeclarationBlock)
                ?.Children.FirstOrDefault(x => x.Type == CssNodeType.StyleDeclaration)
                ?.Children.FirstOrDefault(x =>
                    x.Type == CssNodeType.Value &&
                    x.Text == "red")
                ;

            Assert.NotNull(node);
        }

        [Test]
        public void Can_handle_whitespace_after_property_name_in_styledeclaration()
        {
            var doc = new AstGenerator().GetAst(".test { background : red;}").Root;

            var node = doc.Children.FirstOrDefault(x => x.Type == CssNodeType.StyleRule)
                ?.Children.FirstOrDefault(x => x.Type == CssNodeType.StyleDeclarationBlock)
                ?.Children.FirstOrDefault(x => x.Type == CssNodeType.StyleDeclaration)
                ?.Children.FirstOrDefault(x =>
                    x.Type == CssNodeType.Value &&
                    x.Text == "red")
                ;

            Assert.NotNull(node);
        }
    }
}

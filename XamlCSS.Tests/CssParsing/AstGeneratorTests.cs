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
            var doc = new AstGenerator().GetAst(css);

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

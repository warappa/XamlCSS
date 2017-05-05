using NUnit.Framework;
using System.Linq;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class TokenizerTests
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
        public void Can_tokenize_css_string()
        {
            var tokens = Tokenizer.Tokenize(css).ToList();

            var expectedTokens = new[]
            {
                CssTokenType.Whitespace, CssTokenType.Whitespace, CssTokenType.Whitespace, CssTokenType.At, CssTokenType.Identifier,
                CssTokenType.Whitespace, CssTokenType.Identifier, CssTokenType.Whitespace, CssTokenType.DoubleQuotes, CssTokenType.Identifier,
                CssTokenType.DoubleQuotes, CssTokenType.Semicolon, CssTokenType.Whitespace, CssTokenType.Whitespace, CssTokenType.Dot,
                CssTokenType.Identifier, CssTokenType.Whitespace, CssTokenType.Dot, CssTokenType.Identifier, CssTokenType.AngleBraketClose,
                CssTokenType.Identifier, CssTokenType.Whitespace, CssTokenType.Identifier, CssTokenType.Pipe, CssTokenType.Identifier,
                CssTokenType.Whitespace, CssTokenType.BraceOpen, CssTokenType.Whitespace, CssTokenType.Whitespace, CssTokenType.Whitespace,
                CssTokenType.Identifier, CssTokenType.Colon, CssTokenType.Whitespace, CssTokenType.Identifier, CssTokenType.Semicolon,
                CssTokenType.Whitespace, CssTokenType.Whitespace, CssTokenType.Whitespace, CssTokenType.Identifier, CssTokenType.Colon,
                CssTokenType.Whitespace, CssTokenType.Hash, CssTokenType.Identifier, CssTokenType.Comma, CssTokenType.Whitespace,
                CssTokenType.Identifier, CssTokenType.Comma, CssTokenType.Whitespace, CssTokenType.Identifier, CssTokenType.ParenthesisOpen,
                CssTokenType.SingleQuotes, CssTokenType.Identifier, CssTokenType.SingleQuotes, CssTokenType.ParenthesisClose,
                CssTokenType.Semicolon, CssTokenType.Whitespace, CssTokenType.Whitespace, CssTokenType.Whitespace, CssTokenType.Identifier,
                CssTokenType.Dot, CssTokenType.Identifier, CssTokenType.Colon, CssTokenType.Whitespace, CssTokenType.Identifier,
                CssTokenType.Semicolon, CssTokenType.Whitespace, CssTokenType.Whitespace, CssTokenType.BraceClose, CssTokenType.Whitespace,
                CssTokenType.Whitespace
            };

            Assert.Contains(new CssToken(CssTokenType.Identifier, "red", 0,0), tokens);
        }
    }
}

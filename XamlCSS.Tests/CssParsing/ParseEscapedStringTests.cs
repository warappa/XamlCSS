using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class ParseEscapedStringTests
    {
        string css = @".fa{Text:""\f0c9\f0c9"";}";

        [Test]
        public void Can_tokenize_escaped_unicode_character_string()
        {
            var tokens = Tokenizer.Tokenize(css).ToList();

            var expectedTokens = new[]
            {
                CssTokenType.Dot,
                CssTokenType.Identifier,
                CssTokenType.BraceOpen,
                CssTokenType.Identifier,
                CssTokenType.Colon,
                CssTokenType.DoubleQuotes,
                CssTokenType.Identifier,
                CssTokenType.DoubleQuotes,
                CssTokenType.Semicolon,
                CssTokenType.BraceClose
            };

            var list2 = tokens.Select(x => "CssTokenType."+x.Type.ToString()).ToList();
            var str = string.Join(", ", list2);

            var list = tokens.Select(x => x.Type).ToList();
            Assert.AreEqual(expectedTokens, list);

            Assert.Contains(new CssToken(CssTokenType.Identifier, @"\f0c9\f0c9", 1, 11) { EscapedUnicodeCharacterCount = 2 }, tokens);
            Assert.Contains(new CssToken(CssTokenType.DoubleQuotes, "\"", 1, 21), tokens);
        }

        [Test]
        public void Can_generate_ast_for_escaped_unicode_character_string()
        {
            var ast = new AstGenerator().GetAst(css);

            var escapedText = ast.Root.Children.First().Children.Skip(1).First().Children.First().Children.Skip(1).First();

            escapedText.Text.Should().Be("");

        }

        [Test]
        public void Can_generate_ast_for_escaped_unicode_character_string_with_correct_space_handling()
        {
            var ast = new AstGenerator().GetAst(@".fa{Text:""\f0c9 abc\f0c9  def\00f0c9  ghi"";}");

            var escapedText = ast.Root.Children.First().Children.Skip(1).First().Children.First().Children.Skip(1).First();

            escapedText.Text.Should().Be("abc def ghi");

        }
    }
}

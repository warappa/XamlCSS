using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
	[TestFixture]
	public class CssParserTests
	{
		string test1 = @"
@namespace xamlcss ""XamlCss"";
.main .sub>div xamlcss|Button {
	background-color: red;
	background: #00ff00, solid, url('aaa');
}
";
		[Test]
		public void TestTokenize()
		{
			var tokens = CssParser.Tokenize(test1).ToList();
			Assert.Contains(new CssToken(CssTokenType.Identifier, "red"), tokens);
		}

		[Test]
		public void TestGetAst()
		{
			var doc = CssParser.GetAst(test1);

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
		public void TestParseCss()
		{
			var styleSheet = CssParser.Parse(test1);

			Assert.AreEqual(1, styleSheet.Rules.Count);
		}
	}
}

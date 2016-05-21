using System.Collections.Generic;
using System.Linq;

namespace XamlCSS.CssParsing
{
	public class CssParser
	{
		private static string defaultCssNamespace;

		public static void Initialize(string defaultCssNamespace)
		{
			CssParser.defaultCssNamespace = defaultCssNamespace;
		}

		internal static CssNode GetAst(string cssDocument)
		{
			var doc = new CssNode(CssNodeType.Document, null, "");

			var currentNode = doc;

			var tokens = Tokenize(cssDocument).ToArray();

			for (var i = 0; i < tokens.Length;)
			{
				var t = tokens[i++];
				CssNode n = null;
				switch (t.Type)
				{
					case CssTokenType.At:

						if (currentNode.Type == CssNodeType.Document)
						{
							n = new CssNode(CssNodeType.NamespaceDeclaration, currentNode, "");
							currentNode.Children.Add(n);
							currentNode = n;
						}
						break;

					case CssTokenType.Identifier:
						if (currentNode.Type == CssNodeType.NamespaceDeclaration)
						{
							n = new CssNode(CssNodeType.NamespaceKeyword, currentNode, "@" + t.Text);
							currentNode.Children.Add(n);
							currentNode = n;
						}
						else if (currentNode.Type == CssNodeType.NamespaceKeyword)
						{
							currentNode = currentNode.Parent;
							n = new CssNode(CssNodeType.NamespaceAlias, currentNode, t.Text);
							currentNode.Children.Add(n);
							currentNode = n;
						}
						else if (currentNode.Type == CssNodeType.NamespaceValue)
						{
							currentNode.Text += t.Text;
						}
						else if (currentNode.Type == CssNodeType.Selectors)
						{
							n = new CssNode(CssNodeType.Selector, currentNode, "");
							currentNode.Children.Add(n);
							var fragment = new CssNode(CssNodeType.SelectorFragment, n, t.Text);
							n.Children.Add(fragment);
							currentNode = fragment;
						}
						else if (currentNode.Type == CssNodeType.Selector)
						{
							n = new CssNode(CssNodeType.SelectorFragment, currentNode, t.Text);
							currentNode.Children.Add(n);
							currentNode = n;
						}
						else if (currentNode.Type == CssNodeType.SelectorFragment)
						{
							currentNode.Text += t.Text;
						}
						else if (currentNode.Type == CssNodeType.StyleDeclarationBlock)
						{
							n = new CssNode(CssNodeType.StyleDeclaration, currentNode, "");
							currentNode.Children.Add(n);
							currentNode = n;

							n = new CssNode(CssNodeType.Key, currentNode, t.Text);
							currentNode.Children.Add(n);
							currentNode = n;
						}
						else if (currentNode.Type == CssNodeType.StyleDeclaration)
						{
							n = new CssNode(CssNodeType.Value, currentNode, t.Text);
							currentNode.Children.Add(n);
							currentNode = n;
						}
						else if (currentNode.Type == CssNodeType.Key)
						{
							currentNode.Text += t.Text;
						}
						else if (currentNode.Type == CssNodeType.Value)
						{
							currentNode.Text += t.Text;
						}
						else if (currentNode.Type == CssNodeType.Document)
						{
							n = new CssNode(CssNodeType.StyleRule, currentNode, "");
							var selectors = new CssNode(CssNodeType.Selectors, n, "");
							n.Children.Add(selectors);

							currentNode.Children.Add(n);
							currentNode = selectors;

							var selector = new CssNode(CssNodeType.Selector, currentNode, "");
							currentNode.Children.Add(selector);
							currentNode = selector;

							var selectorFragment = new CssNode(CssNodeType.SelectorFragment, currentNode, t.Text);
							currentNode.Children.Add(selectorFragment);
							currentNode = selectorFragment;
						}
						break;
					case CssTokenType.DoubleQuotes:
						if (currentNode.Type == CssNodeType.NamespaceKeyword)
						{
							currentNode = currentNode.Parent;
							currentNode.Children.Add(new CssNode(CssNodeType.NamespaceAlias, currentNode, ""));
							n = new CssNode(CssNodeType.NamespaceValue, currentNode, t.Text);
							currentNode.Children.Add(n);
							currentNode = n;
						}
						else if (currentNode.Type == CssNodeType.NamespaceAlias)
						{
							currentNode = currentNode.Parent;
							n = new CssNode(CssNodeType.NamespaceValue, currentNode, t.Text);
							currentNode.Children.Add(n);
							currentNode = n;
						}
						else if (currentNode.Type == CssNodeType.NamespaceValue)
						{
							currentNode.Text += t.Text;
						}
						break;
					case CssTokenType.SingleQuotes:
						if (currentNode.Type == CssNodeType.StyleDeclaration)
						{
							n = new CssNode(CssNodeType.Value, currentNode, "");
							currentNode.Children.Add(n);
							currentNode = n;
						}
						else if (currentNode.Type == CssNodeType.Value)
						{
							currentNode.Text += "";
						}
						break;
					case CssTokenType.Colon:
						if (currentNode.Type == CssNodeType.Key)
						{
							currentNode = currentNode.Parent;
						}
						else if (currentNode.Type == CssNodeType.SelectorFragment)
						{
							currentNode.Text += t.Text;
						}
						break;
					case CssTokenType.Semicolon:
						if (currentNode.Type == CssNodeType.Value)
						{
							currentNode = currentNode.Parent;
						}
						else if (currentNode.Type == CssNodeType.NamespaceValue)
						{
							currentNode = currentNode.Parent;
						}
						currentNode = currentNode.Parent;
						break;
					case CssTokenType.Dot:
						if (currentNode.Type == CssNodeType.Document)
						{
							n = new CssNode(CssNodeType.StyleRule, currentNode, "");
							var selectors = new CssNode(CssNodeType.Selectors, n, "");
							n.Children.Add(selectors);

							currentNode.Children.Add(n);
							currentNode = selectors;

							var selector = new CssNode(CssNodeType.Selector, currentNode, "");
							currentNode.Children.Add(selector);
							currentNode = selector;

							var selectorFragment = new CssNode(CssNodeType.SelectorFragment, currentNode, t.Text);
							currentNode.Children.Add(selectorFragment);
							currentNode = selectorFragment;
						}
						else if (currentNode.Type == CssNodeType.NamespaceAlias)
						{
							currentNode.Text += t.Text;
						}
						else if (currentNode.Type == CssNodeType.NamespaceValue)
						{
							currentNode.Text += t.Text;
						}
						else if (currentNode.Type == CssNodeType.Selectors)
						{
							var selector = new CssNode(CssNodeType.Selector, currentNode, "");
							currentNode.Children.Add(selector);
							currentNode = selector;
							currentNode.Children.Add(new CssNode(CssNodeType.SelectorFragment, currentNode, "." + tokens[i++].Text));
						}
						else if (currentNode.Type == CssNodeType.Selector)
						{
							var selectorFragment = new CssNode(CssNodeType.SelectorFragment, currentNode, ".");
							currentNode.Children.Add(selectorFragment);
							currentNode = selectorFragment;
						}
						else if (currentNode.Type == CssNodeType.SelectorFragment)
						{
							currentNode.Text += t.Text;
						}
						else if (currentNode.Type == CssNodeType.Key)
						{
							currentNode.Text += t.Text;
						}
						break;
					case CssTokenType.Hash:
						if (currentNode.Type == CssNodeType.Document)
						{
							n = new CssNode(CssNodeType.StyleRule, currentNode, "");
							var selectors = new CssNode(CssNodeType.Selectors, n, "");
							n.Children.Add(selectors);

							currentNode.Children.Add(n);
							currentNode = selectors;

							var selector = new CssNode(CssNodeType.Selector, currentNode, "");
							currentNode.Children.Add(selector);
							currentNode = selector;

							var selectorFragment = new CssNode(CssNodeType.SelectorFragment, currentNode, t.Text);
							currentNode.Children.Add(selectorFragment);
							currentNode = selectorFragment;
						}
						else if (currentNode.Type == CssNodeType.Selectors)
						{
							var selector = new CssNode(CssNodeType.Selector, currentNode, "");
							currentNode.Children.Add(selector);
							currentNode = selector;
							currentNode.Children.Add(new CssNode(CssNodeType.SelectorFragment, currentNode, "." + tokens[i++].Text));
						}
						else if (currentNode.Type == CssNodeType.Selector)
						{
							var selectorFragment = new CssNode(CssNodeType.SelectorFragment, currentNode, ".");
							currentNode.Children.Add(selectorFragment);
							currentNode = selectorFragment;
						}
						else if (currentNode.Type == CssNodeType.SelectorFragment)
						{
							currentNode.Text += t.Text;
						}
						else if (currentNode.Type == CssNodeType.StyleDeclaration)
						{
							n = new CssNode(CssNodeType.Value, currentNode, t.Text);
							currentNode.Children.Add(n);
							currentNode = n;
						}
						break;
					case CssTokenType.AngleBraketClose:
						if (currentNode.Type == CssNodeType.SelectorFragment)
						{
							currentNode = currentNode.Parent;
						}
						if (currentNode.Type == CssNodeType.Selector)
						{
							currentNode.Children.Add(new CssNode(CssNodeType.SelectorFragment, currentNode, ">"));
						}
						break;
					case CssTokenType.ParenthesisOpen:
					case CssTokenType.ParenthesisClose:
						if (currentNode.Type == CssNodeType.Value)
						{
							currentNode.Text += t.Text;
						}
						else if (currentNode.Type == CssNodeType.SelectorFragment)
						{
							currentNode.Text += t.Text;
						}
						break;
					case CssTokenType.Comma:
						if (currentNode.Type == CssNodeType.SelectorFragment)
						{
							currentNode = currentNode.Parent;
						}
						if (currentNode.Type == CssNodeType.Selector)
						{
							currentNode = currentNode.Parent;
						}
						if (currentNode.Type == CssNodeType.Value)
						{
							currentNode.Text += t.Text;
						}
						else if (currentNode.Type == CssNodeType.NamespaceValue)
						{
							currentNode.Text += t.Text;
						}
						break;
					case CssTokenType.Pipe:
						if (currentNode.Type == CssNodeType.SelectorFragment)
						{
							currentNode.Text += t.Text;
						}
						else if (currentNode.Type == CssNodeType.Key)
						{
							currentNode.Text += t.Text;
						}
						break;
					case CssTokenType.BraceOpen:
						currentNode.Text = currentNode.Text.Trim();

						if (currentNode.Type == CssNodeType.SelectorFragment)
						{
							currentNode = currentNode.Parent;
						}
						if (currentNode.Type == CssNodeType.Selector)
						{
							currentNode = currentNode.Parent;
						}
						if (currentNode.Type == CssNodeType.Selectors)
						{
							currentNode = currentNode.Parent;
						}
						n = new CssNode(CssNodeType.StyleDeclarationBlock, currentNode, "");
						currentNode.Children.Add(n);
						currentNode = n;
						break;
					case CssTokenType.BraceClose:
						currentNode = currentNode.Parent.Parent;
						break;
					case CssTokenType.Whitespace:
						currentNode.Text += t.Text;
						break;
				}
			}

			return doc;
		}

		public static StyleSheet Parse(string cssDocument, string defaultCssNamespace = null)
		{
			var ast = GetAst(cssDocument);

			var styleSheet = new StyleSheet();

			styleSheet.Namespaces = ast.Children.Where(x => x.Type == CssNodeType.NamespaceDeclaration)
					.Select(x => new CssNamespace(
						x.Children.First(y => y.Type == CssNodeType.NamespaceAlias).Text.Trim(),
						x.Children.First(y => y.Type == CssNodeType.NamespaceValue).Text.Trim('"')))
					.ToList();

			if (string.IsNullOrEmpty(defaultCssNamespace) == true)
				defaultCssNamespace = CssParser.defaultCssNamespace;

			if (styleSheet.Namespaces.Any(x => x.Alias == "") == false &&
				string.IsNullOrEmpty(defaultCssNamespace) == false)
			{
				styleSheet.Namespaces.Add(new CssNamespace("", defaultCssNamespace));
			}

			foreach (var astRule in ast.Children.Where(x => x.Type == CssNodeType.StyleRule).ToArray())
			{
				var rule = new StyleRule();

				rule.SelectorType = SelectorType.LogicalTree;
				var selectors = astRule.Children.Single(x => x.Type == CssNodeType.Selectors);
				string[] selectorTexts = selectors
					.Children
						.Select(x => string.Join(" ", x.Children // selector
							.Select(y => y.Text)))
					.ToArray();
				rule.Selector = string.Join(",", selectorTexts);

				var astBlock = astRule.Children.Single(x => x.Type == CssNodeType.StyleDeclarationBlock);

				var styleDeclarations = astBlock.Children.Select(x => new StyleDeclaration
				{
					Property = x.Children.Single(y => y.Type == CssNodeType.Key).Text,
					Value = x.Children.Single(y => y.Type == CssNodeType.Value).Text,
				})
				.ToArray();

				rule.DeclarationBlock.AddRange(styleDeclarations);

				styleSheet.Rules.Add(rule);
			}

			var grouped = styleSheet.Rules.GroupBy(x => x.Selector);

			var newRuleset = grouped
				.Select(x => new StyleRule()
				{
					Selector = x.Key,
					SelectorType = x.First().SelectorType,
					DeclarationBlock = new StyleDeclarationBlock(x.SelectMany(y =>
						y.DeclarationBlock.Select(z => z).ToArray()))
				})
				.ToList();

			styleSheet.Rules.Clear();
			styleSheet.Rules.AddRange(newRuleset);

			return styleSheet;
		}

		internal static IEnumerable<CssToken> Tokenize(string cssDocument)
		{
			var strs = cssDocument.Split(new[] { ' ', '\t', '\n', '\r' }).SelectMany(x => new[] { " ", x }).ToArray();
			strs = strs.Select(x => x == "" ? " " : x).ToArray();
			var strs2 = new List<string>(strs.Length);

			var prevousWasWhitespace = false;
			for (var i = 0; i < strs.Length; i++)
			{
				var isWhitespace = string.IsNullOrWhiteSpace(strs[i]);
				if (isWhitespace == false)
					strs2.Add(strs[i]);
				if (isWhitespace &&
					prevousWasWhitespace == false)
				{
					strs2.Add(strs[i]);
				}
				prevousWasWhitespace = isWhitespace;
			}
			strs = strs2.ToArray();

			strs = strs
				.SplitThem('.')
				.SplitThem(';')
				.SplitThem('|')
				.SplitThem('>')
				.SplitThem('<')
				.SplitThem('@')
				.SplitThem('"')
				.SplitThem('\'')
				.SplitThem(':')
				.SplitThem(',')
				.SplitThem(')')
				.SplitThem('(')
				.SplitThem(' ')
				.SplitThem('#')
				.SplitThem('{')
				.SplitThem('}')
				.ToArray();
			var strsIndex = 0;

			List<CssToken> tokens = new List<CssToken>();

			string c;
			while (strsIndex < strs.Length)
			{
				c = strs[strsIndex++];
				CssToken t = null;

				if (c == "@")
				{
					t = new CssToken(CssTokenType.At, c);
				}
				else if (c == "{")
				{
					t = new CssToken(CssTokenType.BraceOpen, c);
				}
				else if (c == "}")
				{
					t = new CssToken(CssTokenType.BraceClose, c);
				}
				else if (c == ";")
				{
					t = new CssToken(CssTokenType.Semicolon, c);
				}
				else if (c == ",")
				{
					t = new CssToken(CssTokenType.Comma, c);
				}
				else if (c == ":")
				{
					t = new CssToken(CssTokenType.Colon, c);
				}
				else if (c == ".")
				{
					t = new CssToken(CssTokenType.Dot, c);
				}
				else if (c == "<")
				{
					t = new CssToken(CssTokenType.AngleBraketOpen, c);
				}
				else if (c == ">")
				{
					t = new CssToken(CssTokenType.AngleBraketClose, c);
				}
				else if (c == "|")
				{
					t = new CssToken(CssTokenType.Pipe, c);
				}
				else if (c == "\"")
				{
					t = new CssToken(CssTokenType.DoubleQuotes, c);
				}
				else if (c == "'")
				{
					t = new CssToken(CssTokenType.SingleQuotes, c);
				}
				else if (c == "(")
				{
					t = new CssToken(CssTokenType.ParenthesisOpen, c);
				}
				else if (c == ")")
				{
					t = new CssToken(CssTokenType.ParenthesisClose, c);
				}
				else if (c == "#")
				{
					t = new CssToken(CssTokenType.Hash, c);
				}
				else if (c == " " ||
					c == "\t" ||
					c == "\r" ||
					c == "\n"
					)
				{
					t = new CssToken(CssTokenType.Whitespace, c);
				}
				else
				{
					t = new CssToken(CssTokenType.Identifier, c);
				}
				tokens.Add(t);
			}

			return tokens;
		}
	}
}

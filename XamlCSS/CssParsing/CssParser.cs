using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            var tokens = Tokenize(cssDocument).ToList();

            for (var i = 0; i < tokens.Count; i++)
            {
                var t = tokens[i];

                CssNode n;

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
                            currentNode.Text.Append(t.Text);
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
                            currentNode.Text.Append(t.Text);
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
                            currentNode.Text.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.Value)
                        {
                            currentNode.Text.Append(t.Text);
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
                            currentNode.Text.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.StyleDeclaration)
                        {
                            n = new CssNode(CssNodeType.Value, currentNode, "");
                            currentNode.Children.Add(n);
                            currentNode = n;
                        }
                        else if (currentNode.Type == CssNodeType.Key)
                        {
                            n = new CssNode(CssNodeType.Value, currentNode.Parent, "");
                            currentNode.Children.Add(n);
                            currentNode = n;
                        }
                        if (currentNode.Type == CssNodeType.Value)
                        {
                            n = new CssNode(CssNodeType.DoubleQuoteText, currentNode, "");
                            currentNode.Children.Add(n);
                            currentNode = n;

                            i++;
                            do
                            {
                                if (tokens[i].Type == CssTokenType.Backslash)
                                {
                                    i++;
                                    currentNode.Text.Append(tokens[i].Text);
                                }
                                else if (tokens[i].Type == CssTokenType.DoubleQuotes)
                                {
                                    currentNode = currentNode.Parent;
                                    break;
                                }
                                else
                                {
                                    currentNode.Text.Append(tokens[i].Text);
                                }
                                i++;
                            } while (i < tokens.Count);
                        }
                        else if (currentNode.Type == CssNodeType.DoubleQuoteText)
                        {
                            currentNode = currentNode.Parent;
                        }
                        break;
                    case CssTokenType.SingleQuotes:
                        if (currentNode.Type == CssNodeType.StyleDeclaration)
                        {
                            n = new CssNode(CssNodeType.Value, currentNode, "");
                            currentNode.Children.Add(n);
                            currentNode = n;
                        }
                        else if (currentNode.Type == CssNodeType.Key)
                        {
                            n = new CssNode(CssNodeType.Value, currentNode.Parent, "");
                            currentNode.Children.Add(n);
                            currentNode = n;
                        }
                        if (currentNode.Type == CssNodeType.Value)
                        {
                            n = new CssNode(CssNodeType.SingleQuoteText, currentNode, "");
                            currentNode.Children.Add(n);
                            currentNode = n;
                            i++;

                            do
                            {
                                if (tokens[i].Type == CssTokenType.Backslash)
                                {
                                    i++;
                                    currentNode.Text.Append(tokens[i].Text);
                                }
                                else if (tokens[i].Type == CssTokenType.SingleQuotes)
                                {
                                    currentNode = currentNode.Parent;
                                    break;
                                }
                                else
                                {
                                    currentNode.Text.Append(tokens[i].Text);
                                }
                                i++;
                            } while (i < tokens.Count);
                        }
                        else if (currentNode.Type == CssNodeType.SingleQuoteText)
                        {
                            currentNode = currentNode.Parent;
                        }
                        break;
                    case CssTokenType.Colon:
                        if (currentNode.Type == CssNodeType.Key)
                        {
                            currentNode = currentNode.Parent;
                        }
                        else if (currentNode.Type == CssNodeType.SelectorFragment)
                        {
                            currentNode.Text.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.Value)
                        {
                            currentNode.Text.Append(t.Text);
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
                            currentNode.Text.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.NamespaceValue)
                        {
                            currentNode.Text.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.Selectors)
                        {
                            var selector = new CssNode(CssNodeType.Selector, currentNode, "");
                            currentNode.Children.Add(selector);

                            var selectorFragment = new CssNode(CssNodeType.SelectorFragment, selector, tokens[i].Text);
                            selector.Children.Add(selectorFragment);

                            currentNode = selectorFragment;
                        }
                        else if (currentNode.Type == CssNodeType.Selector)
                        {
                            var selectorFragment = new CssNode(CssNodeType.SelectorFragment, currentNode, ".");
                            currentNode.Children.Add(selectorFragment);
                            currentNode = selectorFragment;
                        }
                        else if (currentNode.Type == CssNodeType.SelectorFragment)
                        {
                            currentNode.Text.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.Key)
                        {
                            currentNode.Text.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.Value)
                        {
                            currentNode.Text.Append(t.Text);
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
                            var selectorFragment = new CssNode(CssNodeType.SelectorFragment, currentNode, t.Text);
                            currentNode.Children.Add(selectorFragment);
                            currentNode = selectorFragment;
                        }
                        else if (currentNode.Type == CssNodeType.Selector)
                        {
                            var selectorFragment = new CssNode(CssNodeType.SelectorFragment, currentNode, ".");
                            currentNode.Children.Add(selectorFragment);
                            currentNode = selectorFragment;
                        }
                        else if (currentNode.Type == CssNodeType.SelectorFragment)
                        {
                            currentNode.Text.Append(t.Text);
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
                            currentNode.Text.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.SelectorFragment)
                        {
                            currentNode.Text.Append(t.Text);
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
                            currentNode.Text.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.NamespaceValue)
                        {
                            currentNode.Text.Append(t.Text);
                        }
                        break;
                    case CssTokenType.Pipe:
                        if (currentNode.Type == CssNodeType.SelectorFragment)
                        {
                            currentNode.Text.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.Key)
                        {
                            currentNode.Text.Append(t.Text);
                        }
                        break;
                    case CssTokenType.BraceOpen:
                        currentNode.Text = new StringBuilder(currentNode.Text.ToString().Trim());
                        if (currentNode.Type == CssNodeType.StyleDeclaration)
                        {
                            n = new CssNode(CssNodeType.Value, currentNode, t.Text);
                            currentNode.Children.Add(n);
                            currentNode = n;
                        }
                        else
                        {
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
                        }
                        break;
                    case CssTokenType.BraceClose:
                        if (currentNode.Type == CssNodeType.Value)
                        {
                            currentNode.Text.Append(t.Text);
                            currentNode = currentNode.Parent;
                        }
                        else
                            currentNode = currentNode.Parent.Parent;
                        break;
                    case CssTokenType.Whitespace:
                        currentNode.Text.Append(t.Text);
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
                        x.Children.First(y => y.Type == CssNodeType.NamespaceAlias).Text.ToString().Trim(),
                        x.Children.First(y => y.Type == CssNodeType.NamespaceValue).Text.ToString().Trim('"')))
                    .ToList();

            if (string.IsNullOrEmpty(defaultCssNamespace) == true)
            {
                defaultCssNamespace = CssParser.defaultCssNamespace;
            }

            if (styleSheet.Namespaces.Any(x => x.Alias == "") == false &&
                string.IsNullOrEmpty(defaultCssNamespace) == false)
            {
                styleSheet.Namespaces.Add(new CssNamespace("", defaultCssNamespace));
            }

            var styleRules = ast.Children
                .Where(x => x.Type == CssNodeType.StyleRule)
                .ToList();

            foreach (var astRule in styleRules)
            {
                var rule = new StyleRule();

                rule.SelectorType = SelectorType.LogicalTree;

                var selectors = astRule.Children
                    .Single(x => x.Type == CssNodeType.Selectors);

                rule.Selectors = selectors.Children
                    .Select(x =>
                    {
                        return new Selector
                        {
                            Value = string.Join(" ", x.Children /* selectors */.Select(y => y.Text))
                        };
                    })
                    .ToList();

                rule.SelectorString = string.Join(",", rule.Selectors.Select(x => x.Value));

                var astBlock = astRule.Children
                    .Single(x => x.Type == CssNodeType.StyleDeclarationBlock);

                var styleDeclarations = astBlock.Children
                    .Select(x => new StyleDeclaration
                    {
                        Property = x.Children
                            .Single(y => y.Type == CssNodeType.Key).Text.ToString(),

                        Value = x.Children
                            .Single(y => y.Type == CssNodeType.Value).Text.ToString() != "" ?
                                x.Children.Single(y => y.Type == CssNodeType.Value).Text.ToString() :
                                x.Children.Single(y => y.Type == CssNodeType.Value).Children
                                    .Select(y => y.Text.ToString())
                                    .Aggregate("", (a, b) => a + (a != "" ? " " : "") + b)
                    })
                    .ToList();

                rule.DeclarationBlock.AddRange(styleDeclarations);

                styleSheet.Rules.Add(rule);
            }

            var splitAndOrderedRules = styleSheet.Rules
                .SelectMany(rule =>
                {
                    return rule.Selectors.Select(selector =>
                    {
                        return new StyleRule
                        {
                            Selectors = new List<Selector>(new[] { selector }),
                            DeclarationBlock = rule.DeclarationBlock,
                            SelectorString = selector.Value,
                            SelectorType = rule.SelectorType
                        };
                    });
                })
                .OrderBy(x => x.Selectors[0].IdSpecificity)
                .ThenBy(x => x.Selectors[0].ClassSpecificity)
                .ThenBy(x => x.Selectors[0].SimpleSpecificity)
                .ToList();

            styleSheet.Rules.Clear();
            styleSheet.Rules.AddRange(splitAndOrderedRules);

            return styleSheet;
        }

        internal static IEnumerable<CssToken> Tokenize(string cssDocument)
        {
            var rawTokens = cssDocument.Split(new[] { ' ', '\t', '\n', '\r' })
                .SelectMany(x => new[] { " ", x })
                .ToList();

            rawTokens = rawTokens
                .Select(x => x == "" ? " " : x)
                .ToList();

            var strs2 = new List<string>(rawTokens.Count);

            var prevousWasWhitespace = false;
            for (var i = 0; i < rawTokens.Count; i++)
            {
                var isWhitespace = string.IsNullOrWhiteSpace(rawTokens[i]);
                if (isWhitespace == false)
                    strs2.Add(rawTokens[i]);
                if (isWhitespace &&
                    prevousWasWhitespace == false)
                {
                    strs2.Add(rawTokens[i]);
                }
                prevousWasWhitespace = isWhitespace;
            }
            rawTokens = strs2.ToList();

            rawTokens = rawTokens
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
                .SplitThem('\\')
                .ToList();

            var strsIndex = 0;

            var tokens = new List<CssToken>();

            string c;
            while (strsIndex < rawTokens.Count)
            {
                c = rawTokens[strsIndex++];
                CssToken t;

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
                else if (c == "\\")
                {
                    t = new CssToken(CssTokenType.Backslash, c);
                }
                else if (
                    c == " " ||
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

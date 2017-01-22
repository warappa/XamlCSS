using System;
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
                            currentNode.TextBuilder.Append(t.Text);
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
                            currentNode.TextBuilder.Append(t.Text);
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
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.Value)
                        {
                            currentNode.TextBuilder.Append(t.Text);
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
                            currentNode.TextBuilder.Append(t.Text);
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
                                    currentNode.TextBuilder.Append(tokens[i].Text);
                                }
                                else if (tokens[i].Type == CssTokenType.DoubleQuotes)
                                {
                                    currentNode = currentNode.Parent;
                                    break;
                                }
                                else
                                {
                                    currentNode.TextBuilder.Append(tokens[i].Text);
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
                                    currentNode.TextBuilder.Append(tokens[i].Text);
                                }
                                else if (tokens[i].Type == CssTokenType.SingleQuotes)
                                {
                                    currentNode = currentNode.Parent;
                                    break;
                                }
                                else
                                {
                                    currentNode.TextBuilder.Append(tokens[i].Text);
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

                            n = new CssNode(CssNodeType.Value, currentNode, "");
                            currentNode.Children.Add(n);
                            currentNode = n;

                            i++;
                            while (t.Type == CssTokenType.Whitespace)
                            {
                                i++;
                                t = tokens[i];
                            }
                        }
                        else if (currentNode.Type == CssNodeType.SelectorFragment)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.Value)
                        {
                            currentNode.TextBuilder.Append(t.Text);
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
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.NamespaceValue)
                        {
                            currentNode.TextBuilder.Append(t.Text);
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
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.Key)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.Value)
                        {
                            currentNode.TextBuilder.Append(t.Text);
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
                            currentNode.TextBuilder.Append(t.Text);
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
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.SelectorFragment)
                        {
                            currentNode.TextBuilder.Append(t.Text);
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
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.NamespaceValue)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        break;
                    case CssTokenType.Pipe:
                        if (currentNode.Type == CssNodeType.SelectorFragment)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.Key)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        break;
                    case CssTokenType.BraceOpen:

                        if (currentNode.Type == CssNodeType.Key)
                        {
                            var keyValue = currentNode.Text;

                            var styleDeclaration = currentNode.Parent;
                            styleDeclaration.Children.Remove(currentNode);

                            var subRule = styleDeclaration;
                            subRule.Type = CssNodeType.StyleRule;

                            var selectors = new CssNode(CssNodeType.Selectors, subRule, "");
                            subRule.Children.Add(selectors);

                            currentNode = selectors;

                            var selector = new CssNode(CssNodeType.Selector, currentNode, "");
                            currentNode.Children.Add(selector);
                            currentNode = selector;

                            var selectorFragment = new CssNode(CssNodeType.SelectorFragment, currentNode, keyValue);
                            currentNode.Children.Add(selectorFragment);
                            currentNode = selectorFragment;
                        }

                        currentNode.TextBuilder = new StringBuilder(currentNode.TextBuilder.ToString().Trim());
                        if (currentNode.Type == CssNodeType.StyleDeclaration)
                        {
                            n = new CssNode(CssNodeType.Value, currentNode, t.Text);
                            currentNode.Children.Add(n);
                            currentNode = n;
                        }
                        else if (currentNode.Type == CssNodeType.Value)
                        {
                            currentNode.TextBuilder.Append(t.Text);
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
                            currentNode.TextBuilder.Append(t.Text);
                            currentNode = currentNode.Parent;
                        }
                        else
                            currentNode = currentNode.Parent.Parent;
                        break;
                    case CssTokenType.Whitespace:
                        currentNode.TextBuilder.Append(t.Text);
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
                        x.Children.First(y => y.Type == CssNodeType.NamespaceAlias).TextBuilder.ToString().Trim(),
                        x.Children.First(y => y.Type == CssNodeType.NamespaceValue).TextBuilder.ToString().Trim('"')))
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
                GetStyleRules(styleSheet, astRule);
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

        private static List<string> GetAllRuleSelectors(List<List<string>> allSelectorLayers)
        {
            return GetAllRuleSelectorsSub(null, allSelectorLayers);
        }
        private static List<string> GetAllRuleSelectorsSub(string baseSelector, List<List<string>> remainingSelectorLayers)
        {
            if (remainingSelectorLayers.Count() == 1)
            {
                return remainingSelectorLayers.First()
                    .Select(x => baseSelector == null ? x : baseSelector + " " + x)
                    .ToList();
            }

            var newRemainingSelectorLayers = remainingSelectorLayers.Skip(1).ToList();

            var currentLayerSelectors = remainingSelectorLayers.First();

            var ruleSelectors = new List<string>();
            foreach (var selector in currentLayerSelectors)
            {
                ruleSelectors.AddRange(GetAllRuleSelectorsSub(selector, newRemainingSelectorLayers));
            }

            return ruleSelectors;
        }

        private static void GetStyleRules(StyleSheet styleSheet, CssNode astRule)
        {
            var astStyleDeclarationBlock = astRule.Children
                   .Single(x => x.Type == CssNodeType.StyleDeclarationBlock);

            var styleDeclarations = astStyleDeclarationBlock.Children
                .Where(x => x.Type == CssNodeType.StyleDeclaration)
                .Select(x => new StyleDeclaration
                {
                    Property = x.Children
                        .Single(y => y.Type == CssNodeType.Key).TextBuilder.ToString(),

                    Value = x.Children
                        .Single(y => y.Type == CssNodeType.Value).TextBuilder.ToString() != "" ?
                            x.Children.Single(y => y.Type == CssNodeType.Value).TextBuilder.ToString() :
                            x.Children.Single(y => y.Type == CssNodeType.Value).Children
                                .Select(y => y.TextBuilder.ToString())
                                .Aggregate("", (a, b) => a + (a != "" ? " " : "") + b)
                })
                .ToList();

            var parentSelectorList = GetParentsSelectorAsts(astRule);
            var parentSelectors = (parentSelectorList?.Select(x => GetSelectorStringsFromSelectorsCssNode(x)) ?? new List<List<string>>()).ToList();


            var currentLevelSelectors = astRule.Children
               .Single(x => x.Type == CssNodeType.Selectors);

            // add current level to parentlevels
            var allSelectorLayers = parentSelectors.Concat(new[] { GetSelectorStringsFromSelectorsCssNode(currentLevelSelectors) })
                .ToList();

            var allSelectorsToUse = GetAllRuleSelectors(allSelectorLayers);


            foreach (var ruleSelectorToUse in allSelectorsToUse)
            {
                var rule = new StyleRule();

                rule.SelectorType = SelectorType.LogicalTree;

                rule.Selectors = new List<Selector>(new[] { new Selector() { Value = ruleSelectorToUse } });

                rule.SelectorString = string.Join(",", rule.Selectors.Select(x => x.Value));

                rule.DeclarationBlock.AddRange(styleDeclarations);
                styleSheet.Rules.Add(rule);
            }

            ResolveSubRules(styleSheet, astStyleDeclarationBlock);
        }

        private static List<string> GetSelectorStringsFromSelectorsCssNode(CssNode selectors)
        {
            return selectors.Children
                            .Select(x => string.Join(" ", x.Children /* selectors */.Select(y => y.Text)))
                            .ToList();
        }

        private static List<Selector> GetSelectorFromSelectorsCssNode(CssNode selectors)
        {
            return selectors.Children
                            .Select(x =>
                            {
                                return new Selector
                                {
                                    Value = string.Join(" ", x.Children /* selectors */.Select(y => y.Text))
                                };
                            })
                            .ToList();
        }

        private static void ResolveSubRules(StyleSheet styleSheet, CssNode astStyleDeclarationBlock)
        {
            var subRuleAsts = astStyleDeclarationBlock.Children
                .Where(x => x.Type == CssNodeType.StyleRule)
                .ToList();

            foreach (var subRuleAst in subRuleAsts)
            {
                GetStyleRules(styleSheet, subRuleAst);
            }
        }

        private static IEnumerable<CssNode> GetParentsSelectorAsts(CssNode astRule)
        {
            var list = new List<CssNode>();

            if (astRule.Type == CssNodeType.StyleDeclarationBlock)
            {
                astRule = astRule.Parent;
            }
            astRule = astRule.Parent;
            while (astRule != null &&
                astRule.Type != CssNodeType.StyleRule)
            {
                astRule = astRule.Parent;
            }

            if (astRule == null)
            {
                return new List<CssNode>();
            }

            var selectors = astRule.Children
                .Single(x => x.Type == CssNodeType.Selectors);

            var fromParents = GetParentsSelectorAsts(astRule);

            return fromParents.Concat(new[] { selectors });
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

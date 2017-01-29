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

        internal static CssToken Peek(List<CssToken> tokens, int currentIndex, CssTokenType type = CssTokenType.Unknown)
        {
            currentIndex++;
            if (type == CssTokenType.Unknown)
            {
                if (currentIndex >= tokens.Count)
                {
                    return null;
                }

                return tokens[currentIndex];
            }

            while (currentIndex < tokens.Count)
            {
                var token = tokens[currentIndex];
                if (token.Type == type)
                {
                    return token;
                }

                currentIndex++;
            }

            return null;
        }

        internal static CssNode GetAst(string cssDocument)
        {
            var doc = new CssNode(CssNodeType.Document, null, "");

            var currentNode = doc;

            var tokens = Tokenizer.Tokenize(cssDocument).ToList();

            for (var i = 0; i < tokens.Count; i++)
            {
                var t = tokens[i];

                CssNode n;

                switch (t.Type)
                {
                    case CssTokenType.At:

                        if (currentNode.Type == CssNodeType.Document)
                        {
                            var identifier = Peek(tokens, i, CssTokenType.Identifier);

                            if (identifier.Text == "keyframes")
                            {
                                n = new CssNode(CssNodeType.KeyframesDeclaration, currentNode, "");
                                currentNode.Children.Add(n);
                                currentNode = n;
                            }
                            else if (identifier.Text == "namespace")
                            {
                                n = new CssNode(CssNodeType.NamespaceDeclaration, currentNode, "");
                                currentNode.Children.Add(n);
                                currentNode = n;
                            }
                        }
                        else if (currentNode.Type == CssNodeType.Value ||
                            currentNode.Type == CssNodeType.VariableValue)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.StyleDeclarationBlock)
                        {
                            var identifier = Peek(tokens, i, CssTokenType.Identifier);

                            if (identifier.Text == "Property")
                            {
                                n = new CssNode(CssNodeType.PropertyTrigger, currentNode, "");
                                currentNode.Children.Add(n);
                                currentNode = n;
                            }
                            else if (identifier.Text == "Data")
                            {
                                n = new CssNode(CssNodeType.DataTrigger, currentNode, "");
                                currentNode.Children.Add(n);
                                currentNode = n;
                            }
                            else if (identifier.Text == "Event")
                            {
                                n = new CssNode(CssNodeType.EventTrigger, currentNode, "");
                                currentNode.Children.Add(n);
                                currentNode = n;
                            }
                        }
                        break;
                    case CssTokenType.Dollar:
                        if (currentNode.Type == CssNodeType.Document ||
                            currentNode.Type == CssNodeType.StyleDeclarationBlock)
                        {
                            n = new CssNode(CssNodeType.VariableDeclaration, currentNode, "");
                            currentNode.Children.Add(n);
                            currentNode = n;

                            n = new CssNode(CssNodeType.VariableName, currentNode, t.Text);
                            currentNode.Children.Add(n);
                            currentNode = n;
                        }
                        else if (currentNode.Type == CssNodeType.Value)
                        {
                            n = new CssNode(CssNodeType.VariableReference, currentNode, "$");
                            currentNode.Children.Add(n);
                            currentNode = n;
                        }
                        break;
                    case CssTokenType.Identifier:
                        if (currentNode.Type == CssNodeType.PropertyTrigger)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.PropertyTriggerProperty)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.DataTrigger)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.DataTriggerBinding)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.PropertyTriggerValue)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.DataTriggerValue)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.VariableName)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.VariableValue)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.VariableReference)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.NamespaceDeclaration)
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

                            ReadDoubleQuoteText(ref currentNode, tokens, ref i);
                        }
                        else if (currentNode.Type == CssNodeType.VariableValue)
                        {
                            i++;

                            ReadDoubleQuoteText(ref currentNode, tokens, ref i);
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

                            ReadSingleQuoteText(ref currentNode, tokens, ref i);
                        }
                        else if (currentNode.Type == CssNodeType.VariableValue)
                        {
                            i++;

                            ReadSingleQuoteText(ref currentNode, tokens, ref i);
                        }
                        else if (currentNode.Type == CssNodeType.SingleQuoteText)
                        {
                            currentNode = currentNode.Parent;
                        }
                        break;
                    case CssTokenType.Colon:
                        if (currentNode.Type == CssNodeType.VariableName)
                        {
                            currentNode = currentNode.Parent;

                            n = new CssNode(CssNodeType.VariableValue, currentNode, "");
                            currentNode.Children.Add(n);
                            currentNode = n;
                        }
                        else if (currentNode.Type == CssNodeType.Key)
                        {
                            var nextToken = FirstTokenTypeOf(tokens, i, new[] { CssTokenType.Semicolon, CssTokenType.DoubleQuotes, CssTokenType.BraceOpen });

                            // normal value
                            if (nextToken == CssTokenType.Semicolon ||
                                nextToken == CssTokenType.DoubleQuotes)
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
                            else // selector
                            {
                                currentNode.TextBuilder.Append(t.Text);
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
                        if (currentNode.Type == CssNodeType.VariableReference)
                        {
                            currentNode = currentNode.Parent;
                        }
                        if (currentNode.Type == CssNodeType.Value)
                        {
                            currentNode = currentNode.Parent;
                        }
                        else if (currentNode.Type == CssNodeType.NamespaceValue)
                        {
                            currentNode = currentNode.Parent;
                        }
                        else if (currentNode.Type == CssNodeType.VariableValue)
                        {
                            currentNode = currentNode.Parent;
                        }
                        currentNode = currentNode.Parent;
                        break;
                    case CssTokenType.Dot:
                        if (currentNode.Type == CssNodeType.Document ||
                            currentNode.Type == CssNodeType.StyleDeclarationBlock)
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
                        else if (currentNode.Type == CssNodeType.PropertyTriggerProperty ||
                            currentNode.Type == CssNodeType.PropertyTriggerValue ||
                            currentNode.Type == CssNodeType.DataTriggerBinding ||
                            currentNode.Type == CssNodeType.DataTriggerValue)
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
                        else if (currentNode.Type == CssNodeType.Value)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.VariableValue)
                        {
                            currentNode.TextBuilder.Append(t.Text);
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
                        else if (currentNode.Type == CssNodeType.Key) // selector
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        else if (currentNode.Type == CssNodeType.SelectorFragment)
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        break;
                    case CssTokenType.Comma:
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
                        else if (currentNode.Type == CssNodeType.PropertyTriggerValue)
                        {
                            currentNode = currentNode.Parent;

                            n = new CssNode(CssNodeType.StyleDeclarationBlock, currentNode, t.Text);
                            currentNode.Children.Add(n);
                            currentNode = n;
                        }
                        else if (currentNode.Type == CssNodeType.DataTriggerValue)
                        {
                            currentNode = currentNode.Parent;

                            n = new CssNode(CssNodeType.StyleDeclarationBlock, currentNode, t.Text);
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
                            currentNode.TextBuilder.Append(t.Text);
                            currentNode = currentNode.Parent;
                        }
                        else
                            currentNode = currentNode.Parent.Parent;
                        break;
                    case CssTokenType.Whitespace:
                        if (currentNode.Type == CssNodeType.DataTrigger)
                        {
                            n = new CssNode(CssNodeType.DataTriggerBinding, currentNode, "");
                            currentNode.Children.Add(n);
                            currentNode = n;
                        }
                        else if (currentNode.Type == CssNodeType.DataTriggerBinding)
                        {
                            currentNode = currentNode.Parent;

                            n = new CssNode(CssNodeType.DataTriggerValue, currentNode, "");
                            currentNode.Children.Add(n);
                            currentNode = n;
                        }
                        else if (currentNode.Type == CssNodeType.PropertyTrigger)
                        {
                            n = new CssNode(CssNodeType.PropertyTriggerProperty, currentNode, "");
                            currentNode.Children.Add(n);
                            currentNode = n;
                        }
                        else if (currentNode.Type == CssNodeType.PropertyTriggerProperty)
                        {
                            currentNode = currentNode.Parent;

                            n = new CssNode(CssNodeType.PropertyTriggerValue, currentNode, "");
                            currentNode.Children.Add(n);
                            currentNode = n;
                        }
                        else
                        {
                            currentNode.TextBuilder.Append(t.Text);
                        }
                        break;
                }
            }

            return doc;
        }

        private static void ReadSingleQuoteText(ref CssNode currentNode, List<CssToken> tokens, ref int i)
        {
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

        private static void ReadDoubleQuoteText(ref CssNode currentNode, List<CssToken> tokens, ref int i)
        {
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

        public static StyleSheet Parse(string cssDocument, string defaultCssNamespace = null)
        {
            var ast = GetAst(cssDocument);

            var styleSheet = new StyleSheet();

            var localNamespaces = ast.Children.Where(x => x.Type == CssNodeType.NamespaceDeclaration)
                    .Select(x => new CssNamespace(
                        x.Children.First(y => y.Type == CssNodeType.NamespaceAlias).TextBuilder.ToString().Trim(),
                        x.Children.First(y => y.Type == CssNodeType.NamespaceValue).TextBuilder.ToString().Trim('"')))
                    .ToList();

            styleSheet.LocalNamespaces.AddRange(localNamespaces);

            if (string.IsNullOrEmpty(defaultCssNamespace) == true)
            {
                defaultCssNamespace = CssParser.defaultCssNamespace;
            }

            if (styleSheet.LocalNamespaces.Any(x => x.Alias == "") == false &&
                string.IsNullOrEmpty(defaultCssNamespace) == false)
            {
                styleSheet.LocalNamespaces.Add(new CssNamespace("", defaultCssNamespace));
            }

            var styleRules = ast.Children
                .Where(x => x.Type == CssNodeType.StyleRule)
                .ToList();

            foreach (var astRule in styleRules)
            {
                GetStyleRules(styleSheet, astRule);
            }

            var splitAndOrderedRules = styleSheet.LocalRules
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

            styleSheet.LocalRules.Clear();
            styleSheet.LocalRules.AddRange(splitAndOrderedRules);

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
                    .Select(x =>
                    {
                        return CombineSelectors(baseSelector, x);
                    })
                    .ToList();
            }

            var newRemainingSelectorLayers = remainingSelectorLayers.Skip(1).ToList();

            var currentLayerSelectors = remainingSelectorLayers.First();

            var ruleSelectors = new List<string>();
            foreach (var currentLayerSelector in currentLayerSelectors)
            {
                var selector = currentLayerSelector.StartsWith("&") ? currentLayerSelector.Substring(1) : currentLayerSelector;

                ruleSelectors.AddRange(GetAllRuleSelectorsSub(CombineSelectors(baseSelector, selector), newRemainingSelectorLayers));
            }

            return ruleSelectors;
        }

        private static string CombineSelectors(string baseSelector, string currentSelector)
        {
            var isConcatSelector = currentSelector.StartsWith("&");
            var hasBaseSelector = baseSelector != null;
            return $"{(!hasBaseSelector ? "" : baseSelector)}{(!isConcatSelector && hasBaseSelector ? " " : "")}{(isConcatSelector ? "" + currentSelector.Substring(1) : currentSelector)}";
        }

        private static string GetVariableValue(CssNode variableReferenceAst)
        {
            var variableName = variableReferenceAst.Text;

            var current = variableReferenceAst.Parent;
            while (current != null)
            {
                var foundDeclaration = current.Children
                    .FirstOrDefault(x =>
                        x.Type == CssNodeType.VariableDeclaration &&
                        x.Children.Any(y => y.Type == CssNodeType.VariableName && y.Text == variableName));

                if (foundDeclaration != null)
                {
                    return foundDeclaration.Children.First(y => y.Type == CssNodeType.VariableValue).Text;
                }
                current = current.Parent;
            }

            throw new InvalidOperationException($"Variable {variableName} not found!");
        }

        private static List<StyleDeclaration> GetStyleDeclarationsFromBlock(CssNode astStyleDeclarationBlock)
        {
            return astStyleDeclarationBlock.Children
                .Where(x => x.Type == CssNodeType.StyleDeclaration)
                .Select(x =>
                {
                    var keyAst = x.Children
                             .Single(y => y.Type == CssNodeType.Key);
                    var valueAst = x.Children
                             .Single(y => y.Type == CssNodeType.Value);
                    return new StyleDeclaration
                    {
                        Property = keyAst.Text,
                        Value = valueAst.Text != "" ?
                                 valueAst.Text.Trim() :
                                 valueAst.Children
                                     .Select(y => y.Type == CssNodeType.VariableReference ? GetVariableValue(y) : y.Text)
                                     .Aggregate("", (a, b) => a + (a != "" ? " " : "") + b).Trim()
                    };
                })
                .ToList();
        }

        private static void GetStyleRules(StyleSheet styleSheet, CssNode astRule)
        {
            var astStyleDeclarationBlock = astRule.Children
                   .Single(x => x.Type == CssNodeType.StyleDeclarationBlock);

            var styleDeclarations = GetStyleDeclarationsFromBlock(astStyleDeclarationBlock);

            var propertyTriggers = GetPropertyTriggers(astStyleDeclarationBlock);
            var dataTriggers = GetDataTriggers(astStyleDeclarationBlock);

            var triggers = propertyTriggers.Concat(dataTriggers).ToList();

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
                rule.DeclarationBlock.Triggers = triggers;

                styleSheet.LocalRules.Add(rule);
            }

            ResolveSubRules(styleSheet, astStyleDeclarationBlock);
        }

        private static List<ITrigger> GetPropertyTriggers(CssNode astStyleDeclarationBlock)
        {
            return astStyleDeclarationBlock.Children
                            .Where(x => x.Type == CssNodeType.PropertyTrigger)
                            .Select(x =>
                            {
                                var propertyAst = x.Children
                                         .Single(y => y.Type == CssNodeType.PropertyTriggerProperty);
                                var valueAst = x.Children
                                         .Single(y => y.Type == CssNodeType.PropertyTriggerValue);

                                var astTriggerStyleDeclarationBlock = x.Children
                                         .Single(y => y.Type == CssNodeType.StyleDeclarationBlock);

                                return new Trigger
                                {
                                    Property = propertyAst.Text.Trim(),
                                    Value = valueAst.Text != "" ?
                                             valueAst.Text.Trim() :
                                             valueAst.Children
                                                 .Select(y => y.Type == CssNodeType.VariableReference ? GetVariableValue(y) : y.Text)
                                                 .Aggregate("", (a, b) => a + (a != "" ? " " : "") + b).Trim(),
                                    StyleDeclaraionBlock = new StyleDeclarationBlock(GetStyleDeclarationsFromBlock(astTriggerStyleDeclarationBlock))
                                };
                            })
                            .ToList<ITrigger>();
        }

        private static List<ITrigger> GetDataTriggers(CssNode astStyleDeclarationBlock)
        {
            return astStyleDeclarationBlock.Children
                            .Where(x => x.Type == CssNodeType.DataTrigger)
                            .Select(x =>
                            {
                                var bindingAst = x.Children
                                         .Single(y => y.Type == CssNodeType.DataTriggerBinding);
                                var valueAst = x.Children
                                         .Single(y => y.Type == CssNodeType.DataTriggerValue);

                                var astTriggerStyleDeclarationBlock = x.Children
                                         .Single(y => y.Type == CssNodeType.StyleDeclarationBlock);

                                return new DataTrigger
                                {
                                    Binding = bindingAst.Text.Trim(),
                                    Value = valueAst.Text != "" ?
                                             valueAst.Text.Trim() :
                                             valueAst.Children
                                                 .Select(y => y.Type == CssNodeType.VariableReference ? GetVariableValue(y) : y.Text)
                                                 .Aggregate("", (a, b) => a + (a != "" ? " " : "") + b).Trim(),
                                    StyleDeclaraionBlock = new StyleDeclarationBlock(GetStyleDeclarationsFromBlock(astTriggerStyleDeclarationBlock))
                                };
                            })
                            .ToList<ITrigger>();
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

        private static bool IsNextTokenOfType(List<CssToken> tokens, int index, CssTokenType type, bool ignoreWhitespace = true)
        {
            return IsNextTokenOfTypes(tokens, index, new[] { type }, ignoreWhitespace);
        }

        private static bool IsNextTokenOfTypes(List<CssToken> tokens, int index, CssTokenType[] types, bool ignoreWhitespace = true)
        {
            int typesIndex = 0;
            index++;

            while (index < tokens.Count)
            {
                if (ignoreWhitespace &&
                    tokens[index].Type == CssTokenType.Whitespace)
                {
                    index++;
                    continue;
                }

                if (tokens[index].Type == types[typesIndex])
                {
                    typesIndex++;
                    if (typesIndex == types.Length)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }

                index++;
            }

            return false;
        }

        private static CssTokenType FirstTokenTypeOf(List<CssToken> tokens, int index, CssTokenType[] types, bool ignoreWhitespace = true)
        {
            index++;

            while (index < tokens.Count)
            {
                if (ignoreWhitespace &&
                    tokens[index].Type == CssTokenType.Whitespace)
                {
                    index++;
                    continue;
                }

                foreach (var type in types)
                {
                    if (tokens[index].Type == type)
                    {
                        return type;
                    }
                }

                index++;
            }

            return CssTokenType.Unknown;
        }
    }
}

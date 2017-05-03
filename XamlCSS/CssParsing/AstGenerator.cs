using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XamlCSS.CssParsing
{
    public static class AstGenerator
    {
        public static CssNode GetAst(string cssDocument)
        {
            var doc = new CssNode(CssNodeType.Document, null, "");

            var currentNode = doc;

            var tokens = Tokenizer.Tokenize(cssDocument).ToList();

            for (var currentIndex = 0; currentIndex < tokens.Count; currentIndex++)
            {
                var currentToken = tokens[currentIndex];

                switch (currentToken.Type)
                {
                    case CssTokenType.Slash:
                        if (tokens[currentIndex + 1].Text == "/")
                        {
                            ReadLineCommentText(ref currentNode, tokens, ref currentIndex);
                        }
                        else if (tokens[currentIndex + 1].Text == "*")
                        {
                            ReadInlineCommentText(ref currentNode, tokens, ref currentIndex);
                        }
                        break;
                    case CssTokenType.At:
                        currentNode = ReadAtAst(currentNode, tokens, ref currentIndex);
                        break;
                    case CssTokenType.Dollar:
                        currentNode = ReadDollarAst(currentNode, currentToken);
                        break;
                    case CssTokenType.Identifier:
                        currentNode = ReadIdentifierAst(currentNode, currentToken);
                        break;
                    case CssTokenType.DoubleQuotes:
                        currentNode = ReadDoubleQuotesAst(currentNode, tokens, ref currentIndex);
                        break;
                    case CssTokenType.SingleQuotes:
                        currentNode = ReadSingleQuotesAst(currentNode, tokens, ref currentIndex);
                        break;
                    case CssTokenType.Colon:
                        currentNode = ReadColonAst(currentNode, tokens, ref currentIndex);
                        break;
                    case CssTokenType.Semicolon:
                        currentNode = ReadSemicolonAst(currentNode);
                        break;
                    case CssTokenType.Dot:
                        currentNode = ReadDotAst(currentNode, tokens, currentIndex, currentToken);
                        break;
                    case CssTokenType.Hash:
                        currentNode = ReadHashAst(currentNode, currentToken);
                        break;
                    case CssTokenType.AngleBraketClose:
                        currentNode = ReadAngleBraketCloseAst(currentNode);
                        break;
                    case CssTokenType.ParenthesisOpen:
                    case CssTokenType.ParenthesisClose:
                        currentNode = ReadParenthesisAst(currentNode, currentToken);
                        break;
                    case CssTokenType.Comma:
                        currentNode = ReadCommaAst(currentNode, currentToken);
                        break;
                    case CssTokenType.Pipe:
                        currentNode = ReadPipeAst(currentNode, currentToken);
                        break;
                    case CssTokenType.BraceOpen:
                        currentNode = ReadBraceOpenAst(currentNode, currentToken);
                        break;
                    case CssTokenType.BraceClose:
                        currentNode = ReadBraceCloseAst(currentNode, currentToken);
                        break;
                    case CssTokenType.Whitespace:
                        currentNode = ReadWhitespaceAst(currentNode, currentToken);
                        break;
                    case CssTokenType.Unknown:
                        break;
                    case CssTokenType.AngleBraketOpen:
                        break;
                    case CssTokenType.Backslash:
                        break;
                    case CssTokenType.SquareBracketOpen:
                        currentNode.TextBuilder.Append(currentToken.Text);
                        break;
                    case CssTokenType.SquareBracketClose:
                        currentNode.TextBuilder.Append(currentToken.Text);
                        break;
                }
            }

            return doc;
        }

        private static CssNode ReadWhitespaceAst(CssNode currentNode, CssToken currentToken)
        {
            CssNode n = null;

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
            else if (currentNode.Type == CssNodeType.EventTrigger)
            {
                n = new CssNode(CssNodeType.EventTriggerEvent, currentNode, "");
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }

            return currentNode;
        }

        private static CssNode ReadBraceCloseAst(CssNode currentNode, CssToken currentToken)
        {
            if (currentNode.Type == CssNodeType.Value)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
                currentNode = currentNode.Parent;
            }
            else
            {
                currentNode = currentNode.Parent.Parent;
            }

            if (currentNode.Type == CssNodeType.StyleDeclaration)
            {
                currentNode = currentNode.Parent;
            }

            return currentNode;
        }

        private static CssNode ReadBraceOpenAst(CssNode currentNode, CssToken currentToken)
        {
            CssNode n = null;

            TrimCurrentNode(currentNode);

            if (currentNode.Type == CssNodeType.MixinParameter)
            {
                currentNode = currentNode.Parent;
            }
            if (currentNode.Type == CssNodeType.MixinParameters)
            {
                currentNode = currentNode.Parent;
            }

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
                n = new CssNode(CssNodeType.Value, currentNode, currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.Value)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.PropertyTriggerValue)
            {
                currentNode = currentNode.Parent;

                n = new CssNode(CssNodeType.StyleDeclarationBlock, currentNode, currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.DataTriggerValue)
            {
                currentNode = currentNode.Parent;

                n = new CssNode(CssNodeType.StyleDeclarationBlock, currentNode, currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.EventTriggerEvent)
            {
                currentNode = currentNode.Parent;

                n = new CssNode(CssNodeType.ActionDeclarationBlock, currentNode, currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.ActionDeclaration)
            {
                n = new CssNode(CssNodeType.ActionParameterBlock, currentNode, "");
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.EnterAction ||
                currentNode.Type == CssNodeType.ExitAction)
            {
                n = new CssNode(CssNodeType.ActionDeclarationBlock, currentNode, "");
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

            return currentNode;
        }

        private static CssNode ReadPipeAst(CssNode currentNode, CssToken currentToken)
        {
            if (currentNode.Type == CssNodeType.SelectorFragment)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.Key)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }

            return currentNode;
        }

        private static CssNode ReadCommaAst(CssNode currentNode, CssToken currentToken)
        {
            if (currentNode.Type == CssNodeType.MixinIncludeParameter)
            {
                currentNode = currentNode.Parent;
            }
            else if (currentNode.Type == CssNodeType.MixinParameter)
            {
                currentNode = currentNode.Parent;
            }
            else if (currentNode.Type == CssNodeType.Key)
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
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.NamespaceValue)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }

            return currentNode;
        }

        private static void TrimCurrentNode(CssNode currentNode)
        {
            currentNode.TextBuilder = new StringBuilder(currentNode.Text.Trim());
        }

        private static CssNode ReadParenthesisAst(CssNode currentNode, CssToken currentToken)
        {
            if (currentNode.Type == CssNodeType.MixinIncludeParameter)
            {
                currentNode = currentNode.Parent;
            }
            if (currentNode.Type == CssNodeType.MixinIncludeParameters)
            {
                currentNode = currentNode.Parent;
            }

            if (currentNode.Type == CssNodeType.MixinParameterDefaultValue)
            {
                currentNode = currentNode.Parent;
            }
            if (currentNode.Type == CssNodeType.MixinParameter)
            {
                currentNode = currentNode.Parent;
            }
            if (currentNode.Type == CssNodeType.MixinParameters)
            {
                currentNode = currentNode.Parent;
            }

            if (currentNode.Type == CssNodeType.MixinInclude)
            {
                if (currentToken.Text == "(")
                {
                    var node = new CssNode(CssNodeType.MixinIncludeParameters, currentNode, "");
                    currentNode.Children.Add(node);
                    currentNode = node;
                }
                else
                {
                    TrimCurrentNode(currentNode);
                }
            }
            else if (currentNode.Type == CssNodeType.Value)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.Key) // selector
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.SelectorFragment)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.MixinDeclaration)
            {
                if (currentToken.Text == "(")
                {
                    var node = new CssNode(CssNodeType.MixinParameters, currentNode, "");
                    currentNode.Children.Add(node);
                    currentNode = node;
                }
            }

            return currentNode;
        }

        private static CssNode ReadAngleBraketCloseAst(CssNode currentNode)
        {
            if (currentNode.Type == CssNodeType.SelectorFragment)
            {
                currentNode = currentNode.Parent;
            }
            if (currentNode.Type == CssNodeType.Selector)
            {
                currentNode.Children.Add(new CssNode(CssNodeType.SelectorFragment, currentNode, ">"));
            }

            return currentNode;
        }

        private static CssNode ReadHashAst(CssNode currentNode, CssToken currentToken)
        {
            CssNode n = null;

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

                var selectorFragment = new CssNode(CssNodeType.SelectorFragment, currentNode, currentToken.Text);
                currentNode.Children.Add(selectorFragment);
                currentNode = selectorFragment;
            }
            else if (currentNode.Type == CssNodeType.Selectors)
            {
                var selector = new CssNode(CssNodeType.Selector, currentNode, "");
                currentNode.Children.Add(selector);
                currentNode = selector;
                var selectorFragment = new CssNode(CssNodeType.SelectorFragment, currentNode, currentToken.Text);
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
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.StyleDeclaration)
            {
                n = new CssNode(CssNodeType.Value, currentNode, currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.Value)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.VariableValue)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }

            return currentNode;
        }

        private static CssNode ReadDotAst(CssNode currentNode, List<CssToken> tokens, int currentIndex, CssToken currentToken)
        {
            CssNode n = null;

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

                var selectorFragment = new CssNode(CssNodeType.SelectorFragment, currentNode, currentToken.Text);
                currentNode.Children.Add(selectorFragment);
                currentNode = selectorFragment;
            }
            else if (currentNode.Type == CssNodeType.NamespaceAlias)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.NamespaceValue)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.Selectors)
            {
                var selector = new CssNode(CssNodeType.Selector, currentNode, "");
                currentNode.Children.Add(selector);

                var selectorFragment = new CssNode(CssNodeType.SelectorFragment, selector, tokens[currentIndex].Text);
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
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.Key)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.Value)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.PropertyTriggerProperty ||
                currentNode.Type == CssNodeType.PropertyTriggerValue ||
                currentNode.Type == CssNodeType.DataTriggerBinding ||
                currentNode.Type == CssNodeType.DataTriggerValue ||
                currentNode.Type == CssNodeType.EventTriggerEvent)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }

            return currentNode;
        }

        private static CssNode ReadSemicolonAst(CssNode currentNode)
        {
            TrimCurrentNode(currentNode);

            if (currentNode.Type == CssNodeType.ImportDeclaration)
            {
                AddImportedStyle(currentNode);
            }
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
            else if (currentNode.Type == CssNodeType.MixinParameterDefaultValue)
            {
                currentNode = currentNode.Parent;
            }

            TrimCurrentNode(currentNode);

            currentNode = currentNode.Parent;

            return currentNode;
        }

        private static void AddImportedStyle(CssNode currentNode)
        {
            var content = CssParser.cssFileProvider?.LoadFrom(currentNode.Text);

            if (content != null)
            {
                var ast = GetAst(content);

                var document = currentNode.Parent;

                document.Children.AddRange(ast.Children);
            }
        }

        private static CssNode ReadColonAst(CssNode currentNode, List<CssToken> tokens, ref int currentIndex)
        {
            CssNode n = null;
            CssToken currentToken = tokens[currentIndex];

            if (currentNode.Type == CssNodeType.VariableName)
            {
                currentNode = currentNode.Parent;

                n = new CssNode(CssNodeType.VariableValue, currentNode, "");
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.MixinParameter)
            {
                n = new CssNode(CssNodeType.MixinParameterDefaultValue, currentNode, "");
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.Key)
            {
                var nextToken = FirstTokenTypeOf(tokens, currentIndex, new[] { CssTokenType.Semicolon, CssTokenType.DoubleQuotes, CssTokenType.BraceOpen });

                // normal value
                if (nextToken == CssTokenType.Semicolon ||
                    nextToken == CssTokenType.DoubleQuotes)
                {
                    currentNode = currentNode.Parent;

                    n = new CssNode(CssNodeType.Value, currentNode, "");
                    currentNode.Children.Add(n);
                    currentNode = n;

                    currentIndex++;
                    while (currentToken.Type == CssTokenType.Whitespace)
                    {
                        currentIndex++;
                        currentToken = tokens[currentIndex];
                    }
                }
                else if (currentNode.Parent.Type == CssNodeType.ActionDeclaration)
                {
                    currentNode = currentNode.Parent;
                }
                else // selector
                {
                    currentNode.TextBuilder.Append(currentToken.Text);
                }
            }
            else if (currentNode.Type == CssNodeType.SelectorFragment)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.EnterAction ||
                currentNode.Type == CssNodeType.ExitAction)
            {

            }
            else if (currentNode.Type == CssNodeType.Value)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }

            return currentNode;
        }

        private static CssNode ReadSingleQuotesAst(CssNode currentNode, List<CssToken> tokens, ref int currentIndex)
        {
            CssNode n = null;
            CssToken currentToken = tokens[currentIndex];

            if (currentNode.Type == CssNodeType.MixinParameter)
            {
                currentIndex++;

                ReadSingleQuoteText(ref currentNode, tokens, ref currentIndex);
            }
            else if (currentNode.Type == CssNodeType.MixinParameterDefaultValue)
            {
                currentIndex++;

                ReadSingleQuoteText(ref currentNode, tokens, ref currentIndex);
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
                n = new CssNode(CssNodeType.SingleQuoteText, currentNode, "");
                currentNode.Children.Add(n);
                currentNode = n;
                currentIndex++;

                ReadSingleQuoteText(ref currentNode, tokens, ref currentIndex);
            }
            else if (currentNode.Type == CssNodeType.VariableValue)
            {
                currentIndex++;

                ReadSingleQuoteText(ref currentNode, tokens, ref currentIndex);
            }
            else if (currentNode.Type == CssNodeType.SingleQuoteText)
            {
                currentNode = currentNode.Parent;
            }
            else if (currentNode.Type == CssNodeType.SelectorFragment)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.ImportDeclaration)
            {
                currentIndex++;

                ReadSingleQuoteText(ref currentNode, tokens, ref currentIndex, false);
            }

            return currentNode;
        }

        private static CssNode ReadDoubleQuotesAst(CssNode currentNode, List<CssToken> tokens, ref int currentIndex)
        {
            CssNode n = null;
            CssToken currentToken = tokens[currentIndex];

            if (currentNode.Type == CssNodeType.MixinParameter)
            {
                currentIndex++;

                ReadDoubleQuoteText(ref currentNode, tokens, ref currentIndex);
            }
            else if (currentNode.Type == CssNodeType.MixinParameterDefaultValue)
            {
                currentIndex++;

                ReadDoubleQuoteText(ref currentNode, tokens, ref currentIndex);
            }
            else if (currentNode.Type == CssNodeType.NamespaceKeyword)
            {
                currentNode = currentNode.Parent;
                currentNode.Children.Add(new CssNode(CssNodeType.NamespaceAlias, currentNode, ""));
                n = new CssNode(CssNodeType.NamespaceValue, currentNode, currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.NamespaceAlias)
            {
                currentNode = currentNode.Parent;
                n = new CssNode(CssNodeType.NamespaceValue, currentNode, currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.NamespaceValue)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
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

                currentIndex++;

                ReadDoubleQuoteText(ref currentNode, tokens, ref currentIndex);
            }
            else if (currentNode.Type == CssNodeType.VariableValue)
            {
                currentIndex++;

                ReadDoubleQuoteText(ref currentNode, tokens, ref currentIndex);
            }
            else if (currentNode.Type == CssNodeType.DoubleQuoteText)
            {
                currentNode = currentNode.Parent;
            }
            else if (currentNode.Type == CssNodeType.SelectorFragment)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.ImportDeclaration)
            {
                currentIndex++;

                ReadDoubleQuoteText(ref currentNode, tokens, ref currentIndex, false);
            }

            return currentNode;
        }

        private static CssNode ReadIdentifierAst(CssNode currentNode, CssToken currentToken)
        {
            CssNode n = null;

            if (currentNode.Type == CssNodeType.ActionDeclarationBlock)
            {
                n = new CssNode(CssNodeType.ActionDeclaration, currentNode, "");
                currentNode.Children.Add(n);
                currentNode = n;

                n = new CssNode(CssNodeType.Key, currentNode, currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.ActionParameterBlock)
            {
                n = new CssNode(CssNodeType.ActionParameter, currentNode, "");
                currentNode.Children.Add(n);
                currentNode = n;

                n = new CssNode(CssNodeType.Key, currentNode, currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.ImportDeclaration)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.MixinIncludeParameters)
            {
                n = new CssNode(CssNodeType.MixinIncludeParameter, currentNode, currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.MixinInclude)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.MixinDeclaration)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.MixinParameter)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.MixinParameterDefaultValue)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.EventTrigger)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.EventTriggerEvent)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.PropertyTrigger)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.PropertyTriggerProperty)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.DataTrigger)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.DataTriggerBinding)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.PropertyTriggerValue)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.DataTriggerValue)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.VariableName)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.VariableValue)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.VariableReference)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.NamespaceDeclaration)
            {
                n = new CssNode(CssNodeType.NamespaceKeyword, currentNode, "@" + currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.NamespaceKeyword)
            {
                currentNode = currentNode.Parent;
                n = new CssNode(CssNodeType.NamespaceAlias, currentNode, currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.NamespaceValue)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.Selectors)
            {
                n = new CssNode(CssNodeType.Selector, currentNode, "");
                currentNode.Children.Add(n);
                var fragment = new CssNode(CssNodeType.SelectorFragment, n, currentToken.Text);
                n.Children.Add(fragment);
                currentNode = fragment;
            }
            else if (currentNode.Type == CssNodeType.Selector)
            {
                n = new CssNode(CssNodeType.SelectorFragment, currentNode, currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.SelectorFragment)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.StyleDeclarationBlock)
            {
                n = new CssNode(CssNodeType.StyleDeclaration, currentNode, "");
                currentNode.Children.Add(n);
                currentNode = n;

                n = new CssNode(CssNodeType.Key, currentNode, currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.StyleDeclaration)
            {
                n = new CssNode(CssNodeType.Value, currentNode, currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.Key)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.Value)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
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

                var selectorFragment = new CssNode(CssNodeType.SelectorFragment, currentNode, currentToken.Text);
                currentNode.Children.Add(selectorFragment);
                currentNode = selectorFragment;
            }

            return currentNode;
        }

        private static CssNode ReadDollarAst(CssNode currentNode, CssToken currentToken)
        {
            CssNode n = null;
            if (currentNode.Type == CssNodeType.MixinDeclaration)
            {
                n = new CssNode(CssNodeType.MixinParameters, currentNode, "");
                currentNode.Children.Add(n);
                currentNode = n;
            }

            if (currentNode.Type == CssNodeType.MixinParameters)
            {
                n = new CssNode(CssNodeType.MixinParameter, currentNode, currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.Document ||
                currentNode.Type == CssNodeType.StyleDeclarationBlock)
            {
                n = new CssNode(CssNodeType.VariableDeclaration, currentNode, "");
                currentNode.Children.Add(n);
                currentNode = n;

                n = new CssNode(CssNodeType.VariableName, currentNode, currentToken.Text);
                currentNode.Children.Add(n);
                currentNode = n;
            }
            else if (currentNode.Type == CssNodeType.Value)
            {
                n = new CssNode(CssNodeType.VariableReference, currentNode, "$");
                currentNode.Children.Add(n);
                currentNode = n;
            }

            return currentNode;
        }

        private static CssNode ReadAtAst(CssNode currentNode, List<CssToken> tokens, ref int currentIndex)
        {
            CssToken currentToken = tokens[currentIndex];
            CssNode n = null;

            if (currentNode.Type == CssNodeType.Document)
            {
                var identifier = Peek(tokens, currentIndex, CssTokenType.Identifier);

                if (identifier.Text == "keyframes")
                {
                    n = new CssNode(CssNodeType.KeyframesDeclaration, currentNode, "");
                    currentNode.Children.Add(n);
                    currentNode = n;
                }
                else if (identifier.Text == "import")
                {
                    n = new CssNode(CssNodeType.ImportDeclaration, currentNode, "");
                    currentNode.Children.Add(n);
                    currentNode = n;

                    currentIndex++;
                }
                else if (identifier.Text == "namespace")
                {
                    n = new CssNode(CssNodeType.NamespaceDeclaration, currentNode, "");
                    currentNode.Children.Add(n);
                    currentNode = n;
                }
                else if (identifier.Text == "mixin")
                {
                    n = new CssNode(CssNodeType.MixinDeclaration, currentNode, "");
                    currentNode.Children.Add(n);
                    currentNode = n;

                    currentIndex++;
                }
            }
            else if (currentNode.Type == CssNodeType.Value ||
                currentNode.Type == CssNodeType.VariableValue)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
            }
            else if (currentNode.Type == CssNodeType.StyleDeclarationBlock)
            {
                var identifier = Peek(tokens, currentIndex, CssTokenType.Identifier);

                if (identifier.Text == "include")
                {
                    n = new CssNode(CssNodeType.MixinInclude, currentNode, "");
                    currentNode.Children.Add(n);
                    currentNode = n;

                    currentIndex++;
                }
                else if (identifier.Text == "Property")
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
                else if (identifier.Text == "Enter")
                {
                    n = new CssNode(CssNodeType.EnterAction, currentNode, "");
                    currentNode.Children.Add(n);
                    currentNode = n;

                    currentIndex++;

                    //currentNode = ReadUntilSemicolon(tokens, currentNode, ref currentIndex);
                }
                else if (identifier.Text == "Exit")
                {
                    n = new CssNode(CssNodeType.ExitAction, currentNode, "");
                    currentNode.Children.Add(n);
                    currentNode = n;

                    currentIndex++;

                    //currentNode = ReadUntilSemicolon(tokens, currentNode, ref currentIndex);
                }
            }

            return currentNode;
        }

        private static CssNode ReadUntilSemicolon(List<CssToken> tokens, CssNode currentNode, ref int currentIndex)
        {
            while (currentIndex < tokens.Count &&
                tokens[currentIndex].Type != CssTokenType.Semicolon)
            {
                currentNode.TextBuilder.Append(tokens[currentIndex].Text);

                currentIndex++;
            }

            return currentNode.Parent;
        }

        private static void ReadSingleQuoteText(ref CssNode currentNode, List<CssToken> tokens, ref int i, bool goToParent = true)
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
                    if (goToParent)
                    {
                        currentNode = currentNode.Parent;
                    }
                    break;
                }
                else
                {
                    currentNode.TextBuilder.Append(tokens[i].Text);
                }
                i++;
            } while (i < tokens.Count);
        }

        private static void ReadDoubleQuoteText(ref CssNode currentNode, List<CssToken> tokens, ref int i, bool goToParent = true)
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
                    if (goToParent)
                    {
                        currentNode = currentNode.Parent;
                    }
                    break;
                }
                else
                {
                    currentNode.TextBuilder.Append(tokens[i].Text);
                }
                i++;
            } while (i < tokens.Count);
        }

        private static void ReadLineCommentText(ref CssNode currentNode, List<CssToken> tokens, ref int i)
        {
            do
            {
                if (tokens[i].Type == CssTokenType.Whitespace &&
                    (tokens[i].Text == "\n" || tokens[i].Text == "\r"))
                {
                    break;
                }
                else
                {

                }
                i++;
            } while (i < tokens.Count);
        }

        private static void ReadInlineCommentText(ref CssNode currentNode, List<CssToken> tokens, ref int i)
        {
            do
            {
                if (tokens[i].Type == CssTokenType.Identifier &&
                    (tokens[i].Text == "*" && tokens[i + 1].Text == "/"))
                {
                    i++;
                    break;
                }
                else
                {

                }
                i++;
            } while (i < tokens.Count);
        }

        private static CssToken Peek(List<CssToken> tokens, int currentIndex, CssTokenType type = CssTokenType.Unknown)
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

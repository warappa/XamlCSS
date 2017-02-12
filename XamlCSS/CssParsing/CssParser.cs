using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XamlCSS.CssParsing
{
    public class CssParser
    {
        private static string defaultCssNamespace;
        private static ICssFileProvider cssFileProvider;

        public static void Initialize(string defaultCssNamespace, ICssFileProvider cssFileProvider)
        {
            CssParser.defaultCssNamespace = defaultCssNamespace;
            CssParser.cssFileProvider = cssFileProvider;
        }

        internal static CssNode GetAst(string cssDocument)
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

                n = new CssNode(CssNodeType.StyleDeclarationBlock, currentNode, currentToken.Text);
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
            var content = cssFileProvider?.LoadFrom(currentNode.Text);

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
                else // selector
                {
                    currentNode.TextBuilder.Append(currentToken.Text);
                }
            }
            else if (currentNode.Type == CssNodeType.SelectorFragment)
            {
                currentNode.TextBuilder.Append(currentToken.Text);
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

            if (currentNode.Type == CssNodeType.ImportDeclaration)
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
            }

            return currentNode;
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

        private static string GetVariableValue(CssNode variableReferenceAst, Dictionary<string, string> parameterValues)
        {
            var variableName = variableReferenceAst.Text;

            if (parameterValues != null &&
                parameterValues.ContainsKey(variableName))
            {
                return parameterValues[variableName];
            }

            var current = variableReferenceAst.Parent;
            while (current != null)
            {
                var foundDeclaration = current.Children
                    .LastOrDefault(x =>
                        x.Type == CssNodeType.VariableDeclaration &&
                        x.Children.Any(y => y.Type == CssNodeType.VariableName && y.Text == variableName));

                if (foundDeclaration == null)
                {
                    foundDeclaration = current.Children
                        .LastOrDefault(x =>
                            x.Type == CssNodeType.MixinParameter &&
                            x.Text == variableName
                            );
                }
                if (foundDeclaration != null)
                {
                    return foundDeclaration.Children.First(y => y.Type == CssNodeType.VariableValue).Text;
                }
                current = current.Parent;
            }

            throw new InvalidOperationException($"Variable {variableName} not found!");
        }

        private static List<TriggerAction> GetActionDeclarationsFromBlock(CssNode astStyleDeclarationBlock, Dictionary<string, string> parameterValues)
        {
            return astStyleDeclarationBlock.Children
                .Where(x => x.Type == CssNodeType.StyleDeclaration)
                .Select(x =>
                {
                    var keyAst = x.Children
                             .Single(y => y.Type == CssNodeType.Key);
                    var valueAst = x.Children
                             .Single(y => y.Type == CssNodeType.Value);
                    return new TriggerAction
                    {
                        Action = keyAst.Text,
                        Parameters = valueAst.Text != "" ?
                                 valueAst.Text.Trim() :
                                 valueAst.Children
                                     .Select(y => y.Type == CssNodeType.VariableReference ? GetVariableValue(y, parameterValues) : y.Text)
                                     .Aggregate("", (a, b) => a + (a != "" ? " " : "") + b).Trim()
                    };
                })
                .ToList();
        }

        private static List<StyleDeclaration> GetStyleDeclarationsFromBlock(CssNode astStyleDeclarationBlock, Dictionary<string, string> parameterValues)
        {
            var mixinIncludes = GetMixinIncludes(astStyleDeclarationBlock);

            return mixinIncludes
                .Concat(astStyleDeclarationBlock.Children
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
                                     .Select(y => y.Type == CssNodeType.VariableReference ? GetVariableValue(y, parameterValues) : y.Text)
                                     .Aggregate("", (a, b) => a + (a != "" ? " " : "") + b).Trim()
                    };
                }))
                .GroupBy(x => x.Property, x => x.Value)
                .Select(x => new StyleDeclaration
                {
                    Property = x.Key,
                    Value = x.Last()
                })
                .ToList();
        }

        private static void GetStyleRules(StyleSheet styleSheet, CssNode astRule)
        {
            var astStyleDeclarationBlock = astRule.Children
                   .Single(x => x.Type == CssNodeType.StyleDeclarationBlock);

            var styleDeclarations = GetStyleDeclarationsFromBlock(astStyleDeclarationBlock, null);

            var propertyTriggers = GetPropertyTriggers(astStyleDeclarationBlock);
            var dataTriggers = GetDataTriggers(astStyleDeclarationBlock);
            var eventTriggers = GetEventTriggers(astStyleDeclarationBlock);

            var triggers = propertyTriggers.Concat(dataTriggers).Concat(eventTriggers).ToList();

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
                                                 .Select(y => y.Type == CssNodeType.VariableReference ? GetVariableValue(y, null) : y.Text)
                                                 .Aggregate("", (a, b) => a + (a != "" ? " " : "") + b).Trim(),
                                    StyleDeclaraionBlock = new StyleDeclarationBlock(GetStyleDeclarationsFromBlock(astTriggerStyleDeclarationBlock, null))
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
                                                 .Select(y => y.Type == CssNodeType.VariableReference ? GetVariableValue(y, null) : y.Text)
                                                 .Aggregate("", (a, b) => a + (a != "" ? " " : "") + b).Trim(),
                                    StyleDeclarationBlock = new StyleDeclarationBlock(GetStyleDeclarationsFromBlock(astTriggerStyleDeclarationBlock, null))
                                };
                            })
                            .ToList<ITrigger>();
        }

        private static List<ITrigger> GetEventTriggers(CssNode astStyleDeclarationBlock)
        {
            return astStyleDeclarationBlock.Children
                            .Where(x => x.Type == CssNodeType.EventTrigger)
                            .Select(x =>
                            {
                                var eventAst = x.Children
                                         .Single(y => y.Type == CssNodeType.EventTriggerEvent);

                                var astTriggerActionDeclarationBlock = x.Children
                                         .Single(y => y.Type == CssNodeType.StyleDeclarationBlock);

                                return new EventTrigger
                                {
                                    Event = eventAst.Text.Trim(),
                                    Actions = new List<TriggerAction>(GetActionDeclarationsFromBlock(astTriggerActionDeclarationBlock, null))
                                };
                            })
                            .ToList<ITrigger>();
        }

        private static List<StyleDeclaration> GetMixinIncludes(CssNode astStyleDeclarationBlock)
        {
            return astStyleDeclarationBlock.Children
                            .Where(x => x.Type == CssNodeType.MixinInclude)
                            .SelectMany(x =>
                            {
                                var name = x.Text;

                                var astMixinParameters = x.Children
                                         .SingleOrDefault(y => y.Type == CssNodeType.MixinIncludeParameters)
                                         ?.Children
                                         .Select(y => y.Text)
                                         .ToList() ?? new List<string>();

                                return GetMixinStyleDefinitions(astStyleDeclarationBlock, name, astMixinParameters);
                            })
                            .ToList();
        }

        private static List<StyleDeclaration> GetMixinStyleDefinitions(CssNode astStyleDeclarationBlock, string name, List<string> parameterValues)
        {
            var declaration = GetMixinDeclaration(astStyleDeclarationBlock, name);

            if (declaration == null)
                return new List<StyleDeclaration>();

            Dictionary<string, string> parameterDict = new Dictionary<string, string>();
            var parameterAsts = declaration.Children.First(x => x.Type == CssNodeType.MixinParameters).Children;
            for (var i = 0; i < parameterAsts.Count; i++)
            {
                var parameterAst = parameterAsts[i];
                if (i < parameterValues.Count)
                {
                    parameterDict.Add(parameterAst.Text, parameterValues[i]);
                }
                else
                {
                    var defaultValueAst = parameterAst.Children.FirstOrDefault(x => x.Type == CssNodeType.MixinParameterDefaultValue);
                    if (defaultValueAst == null)
                    {
                        throw new InvalidOperationException($"Parameter missing for parameter '{parameterAst.Text}' which has no default value!");
                    }

                    parameterDict.Add(parameterAst.Text, defaultValueAst.Text);
                }
            }

            return GetStyleDeclarationsFromBlock(declaration.Children.First(x => x.Type == CssNodeType.StyleDeclarationBlock), parameterDict);
        }

        private static CssNode GetMixinDeclaration(CssNode astStyleDeclarationBlock, string name)
        {
            var current = astStyleDeclarationBlock.Parent;

            while (current != null)
            {
                var mixinDeclaration = current
                    .Children
                    .LastOrDefault(x =>
                        x.Type == CssNodeType.MixinDeclaration &&
                        x.Text == name);
                if (mixinDeclaration != null)
                {
                    return mixinDeclaration;
                }
                current = current.Parent;
            }
            return null;
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
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace XamlCSS.CssParsing
{
    public class AstGenerator
    {
        private List<LineInfo> errors;
        private List<LineInfo> warnings;

        private List<CssToken> tokens;
        private int currentIndex;
        private CssNode currentNode;
        private CssToken currentToken => tokens[currentIndex];
        private CssToken nextToken => tokens[currentIndex + 1];
        private CssToken previousToken => currentIndex > 0 ? tokens[currentIndex - 1] : default(CssToken);
        private CssNode n;

        private void ReadKeyframes()
        {
            throw new NotSupportedException();
        }

        private void Error(string message, IEnumerable<CssToken> tokens)
        {
            errors.Add(new LineInfo($"ERROR ({tokens.First().Line}:{tokens.First().Column} - {tokens.Last().Line}:{tokens.Last().Column}): {message}", tokens));
        }

        private void Warning(string message, CssToken token)
        {
            warnings.Add(new LineInfo($"WARNING ({tokens.First().Line}:{tokens.First().Column} - {tokens.Last().Line}:{tokens.Last().Column}): {message}", tokens));
        }

        private void SkipToEndOfBlock()
        {
            var innerOpenedBlocks = 0;
            while (currentIndex < tokens.Count &&
                (currentToken.Type != CssTokenType.BraceClose ||
                innerOpenedBlocks > 0))
            {
                if (currentToken.Type == CssTokenType.DoubleQuotes)
                {
                    ReadDoubleQuoteText(false);
                }
                else if (currentToken.Type == CssTokenType.SingleQuotes)
                {
                    ReadSingleQuoteText(false);
                }
                else if (currentToken.Type == CssTokenType.Slash &&
                    nextToken.Type == CssTokenType.Slash)
                {
                    SkipLineCommentText();
                }
                else if (currentToken.Type == CssTokenType.Slash &&
                    nextToken.Type == CssTokenType.Asterisk)
                {
                    SkipInlineCommentText();
                }
                else if (currentToken.Type == CssTokenType.BraceOpen)
                {
                    innerOpenedBlocks++;
                }

                currentIndex++;
            }

            currentIndex++;
        }

        private void ReadImport()
        {
            var oldCurrentNode = currentNode;
            var startToken = currentToken;

            try
            {
                SkipWhitespace();

                switch (currentToken.Type)
                {
                    case CssTokenType.DoubleQuotes:
                        SkipExpected(startToken, CssTokenType.DoubleQuotes);
                        ReadDoubleQuoteText(false);
                        SkipExpected(startToken, CssTokenType.Semicolon);

                        AddImportedStyle(currentNode);
                        break;
                    case CssTokenType.SingleQuotes:
                        SkipExpected(startToken, CssTokenType.SingleQuotes);
                        ReadSingleQuoteText(false);
                        SkipExpected(startToken, CssTokenType.Semicolon);

                        AddImportedStyle(currentNode);
                        break;
                    default:
                        throw new AstGenerationException($"ReadImport: unexpected token '{currentToken.Text}'", GetTokens(startToken, currentToken));
                        break;
                }
            }
            catch (AstGenerationException e)
            {
                Error(e.Message, e.Tokens);

                SkipUntilLineEnd();

                currentNode = oldCurrentNode;
            }
        }

        private List<CssToken> GetTokens(CssToken startToken, CssToken endToken)
        {
            return GetTokens(tokens.IndexOf(startToken), tokens.IndexOf(endToken));
        }

        private List<CssToken> GetTokens(int startIndex, int endIndex)
        {
            if (startIndex < 0 ||
                startIndex > endIndex)
            {
                throw new ArgumentException($"startIndex invalid: {startIndex} ({startIndex}, {endIndex}, {tokens.Count})");
            }
            if (endIndex < 0 ||
                endIndex >= tokens.Count)
            {
                throw new ArgumentException($"endIndex invalid: {endIndex} ({startIndex}, {endIndex}, {tokens.Count})");
            }

            var list = new List<CssToken>();

            for (var i = startIndex; i <= endIndex; i++)
            {
                list.Add(tokens[i]);
            }

            return list;
        }

        private string GetStringFromTokens(CssToken startToken, CssToken endToken)
        {
            return GetStringFromTokens(tokens.IndexOf(startToken), tokens.IndexOf(endToken));
        }

        private string GetStringFromTokens(int startIndex, int endIndex)
        {
            if (startIndex < 0 ||
                startIndex > endIndex)
            {
                throw new ArgumentException($"startIndex invalid: {startIndex} ({startIndex}, {endIndex}, {tokens.Count})");
            }
            if (endIndex < 0 ||
                endIndex >= tokens.Count)
            {
                throw new ArgumentException($"endIndex invalid: {endIndex} ({startIndex}, {endIndex}, {tokens.Count})");
            }

            var stringBuilder = new StringBuilder();
            for (var i = startIndex; i < endIndex; i++)
            {
                stringBuilder.Append(tokens[i].Text);
            }

            return stringBuilder.ToString();
        }

        private void GoToParent()
        {
            currentNode = currentNode.Parent;
        }

        private void AddOnParentAndSetCurrent(CssNode node)
        {
            node.Parent = currentNode.Parent;

            currentNode.Parent.AddChild(node);
            currentNode = node;
        }

        private void AddOnParentAndSetCurrent(CssNodeType type)
        {
            AddOnParentAndSetCurrent(new CssNode(type));
        }

        private void AddAndSetCurrent(CssNode node)
        {
            node.Parent = currentNode;

            currentNode.AddChild(node);
            currentNode = node;
        }
        private void AddAndSetCurrent(CssNodeType type)
        {
            AddAndSetCurrent(new CssNode(type));
        }

        private void ReadNamespaceDeclaration()
        {
            // current node is NamespaceDeclaration

            var oldCurrentNode = currentNode;
            var startToken = currentToken;

            try
            {

                SkipWhitespace(false);

                AddAndSetCurrent(CssNodeType.NamespaceKeyword);

                SkipExpected(startToken, CssTokenType.At);

                ReadIdentifier(); // namespace keyword

                SkipWhitespace(false);

                ExpectToken(startToken, CssTokenType.DoubleQuotes, CssTokenType.SingleQuotes, CssTokenType.Identifier);

                AddOnParentAndSetCurrent(CssNodeType.NamespaceAlias);

                if (currentToken.Type != CssTokenType.DoubleQuotes &&
                    currentToken.Type != CssTokenType.SingleQuotes)
                {
                    ReadIdentifier(); // namespace alias

                    SkipWhitespace(false);
                }

                ExpectToken(startToken, CssTokenType.DoubleQuotes, CssTokenType.SingleQuotes);

                AddOnParentAndSetCurrent(CssNodeType.NamespaceValue);

                switch (currentToken.Type)
                {
                    case CssTokenType.DoubleQuotes:
                        SkipExpected(startToken, CssTokenType.DoubleQuotes);

                        ReadDoubleQuoteText(false);
                        SkipExpected(startToken, CssTokenType.Semicolon);

                        GoToParent();
                        break;
                    case CssTokenType.SingleQuotes:
                        SkipExpected(startToken, CssTokenType.SingleQuotes);

                        ReadSingleQuoteText(false);
                        SkipExpected(startToken, CssTokenType.Semicolon);
                        GoToParent();
                        break;
                    default:
                        throw new AstGenerationException($"ReadNamespaceDeclaration: unexpected token '{currentToken.Text}'", GetTokens(startToken, currentToken));

                        break;
                }
            }
            catch (AstGenerationException e)
            {
                Error(e.Message, e.Tokens);

                SkipUntilLineEnd();

                oldCurrentNode.Parent.RemoveChild(oldCurrentNode);

                currentNode = oldCurrentNode;
            }
        }

        private void SkipExpected(CssToken startToken, CssTokenType type)
        {
            if (currentIndex >= tokens.Count)
            {
                throw new AstGenerationException($"Expected token-type '{type}' but end of style was reached!", GetTokens(startToken, tokens[tokens.Count - 1]));
            }

            if (currentToken.Type != type)
            {
                throw new AstGenerationException($"Expected token-type '{type}' but current token was '{currentToken.Type}'!", GetTokens(startToken, currentToken));
            }

            currentIndex++;
        }

        private void ReadIdentifier()
        {
            ExpectToken(currentToken, CssTokenType.Identifier);

            currentNode.TextBuilder.Append(currentToken.Text);
            currentIndex++;
        }

        private void SkipWhitespace(bool skipLineEnding = true)
        {
            while (currentIndex < tokens.Count &&
                (currentToken.Type == CssTokenType.Whitespace ||
                (currentToken.Type == CssTokenType.Slash &&
                       nextToken.Type == CssTokenType.Asterisk) ||
                       (currentToken.Type == CssTokenType.Slash &&
                    nextToken.Type == CssTokenType.Slash)))
            {
                if (currentToken.Type == CssTokenType.Slash &&
                       nextToken.Type == CssTokenType.Asterisk)
                {
                    SkipInlineCommentText();
                }
                else if (currentToken.Type == CssTokenType.Slash &&
                    nextToken.Type == CssTokenType.Slash)
                {
                    SkipLineCommentText();
                }
                else if (!skipLineEnding &&
                    (currentToken.Text[0] == '\n' ||
                    currentToken.Text[0] == '\r'))
                {
                    break;
                }
                else
                {
                    currentIndex++;
                }
            }
        }

        private void SkipUntilLineEnd(params CssTokenType[] orTypes)
        {
            while (currentIndex < tokens.Count &&
                currentToken.Text[0] != '\n' &&
                !orTypes.Contains(currentToken.Type))
            {
                currentIndex++;
            }
        }

        private void ReadDocument()
        {
            var oldCurrentNode = currentNode;

            try
            {
                SkipWhitespace();

                while (currentIndex < tokens.Count)
                {
                    var startToken = currentToken;

                    switch (currentToken.Type)
                    {
                        case CssTokenType.Slash:
                            if (nextToken.Type == CssTokenType.Slash)
                            {
                                SkipLineCommentText();
                            }
                            else if (nextToken.Type == CssTokenType.Asterisk)
                            {
                                SkipInlineCommentText();
                            }
                            else
                            {
                                SkipUntilLineEnd();
                            }
                            break;
                        case CssTokenType.At:
                            var identifier = nextToken;// Peek(tokens, currentIndex, CssTokenType.Identifier);

                            if (identifier.Text == "keyframes")
                            {
                                AddAndSetCurrent(CssNodeType.KeyframesDeclaration);

                                ReadKeyframes();

                                GoToParent();
                            }
                            else if (identifier.Text == "import")
                            {
                                AddAndSetCurrent(CssNodeType.ImportDeclaration);

                                SkipExpected(startToken, CssTokenType.At);
                                SkipExpected(startToken, CssTokenType.Identifier);

                                ReadImport();

                                GoToParent();
                            }
                            else if (identifier.Text == "namespace")
                            {
                                AddAndSetCurrent(CssNodeType.NamespaceDeclaration);

                                ReadNamespaceDeclaration();

                                GoToParent();
                            }
                            else if (identifier.Text == "mixin")
                            {
                                AddAndSetCurrent(CssNodeType.MixinDeclaration);

                                SkipExpected(startToken, CssTokenType.At);
                                SkipExpected(startToken, CssTokenType.Identifier);

                                ReadMixin();

                                GoToParent();
                            }
                            else
                            {
                                Error($"ReadDocument: unexpected token '{identifier.Text}'", GetTokens(startToken, identifier));
                            }
                            break;
                        case CssTokenType.Dollar:
                            AddAndSetCurrent(CssNodeType.VariableDeclaration);

                            ReadVariable();

                            GoToParent();
                            break;
                        case CssTokenType.Identifier:
                        case CssTokenType.Dot:
                        case CssTokenType.Hash:
                        case CssTokenType.SquareBracketOpen:
                        case CssTokenType.Asterisk:
                        case CssTokenType.Tilde:
                        case CssTokenType.Circumflex:
                        case CssTokenType.Colon:
                        case CssTokenType.Underscore:
                            AddAndSetCurrent(CssNodeType.StyleRule);

                            ReadStyleRule();

                            GoToParent();
                            break;
                        default:
                            throw new AstGenerationException(currentToken.Type.ToString(), currentToken);
                    }

                    SkipWhitespace();
                }
            }
            catch (AstGenerationException e)
            {
                Error(e.Message, e.Tokens);

                currentNode = oldCurrentNode;
            }
        }

        private void ExpectNode(params CssNodeType[] types)
        {
            if (!types.Contains(currentNode.Type))
            {
                throw new AstGenerationException($"Expected node type {string.Join(" or ", types.Select(x => $"'{x}'"))} but got '{currentNode.Type}'!", currentToken);
            }
        }

        private void ExpectToken(CssToken startToken, params CssTokenType[] types)
        {
            if (!types.Contains(currentToken.Type))
            {
                throw new AstGenerationException($"Expected token type {string.Join(" or ", types.Select(x => $"'{x}'"))} but got '{currentToken.Type}'!", GetTokens(startToken, currentToken));
            }
        }

        private void ReadMixin()
        {
            var oldCurrentNode = currentNode;
            var startToken = currentToken;
            try
            {
                SkipWhitespace();

                ReadUntil(CssTokenType.ParenthesisOpen);
                TrimCurrentNode();

                SkipExpected(startToken, CssTokenType.ParenthesisOpen);

                AddAndSetCurrent(CssNodeType.MixinParameters);

                while (currentIndex < tokens.Count &&
                    currentToken.Type != CssTokenType.ParenthesisClose)
                {
                    var parameterToken = currentToken;

                    SkipWhitespace();

                    ExpectNode(CssNodeType.MixinParameters);

                    AddAndSetCurrent(CssNodeType.MixinParameter);

                    ReadUntil(CssTokenType.ParenthesisClose, CssTokenType.Comma, CssTokenType.Colon);

                    if (currentToken.Type == CssTokenType.Colon)
                    {
                        SkipExpected(parameterToken, CssTokenType.Colon);
                        SkipWhitespace();

                        AddAndSetCurrent(CssNodeType.MixinParameterDefaultValue);

                        if (currentToken.Type == CssTokenType.DoubleQuotes)
                        {
                            SkipExpected(parameterToken, CssTokenType.DoubleQuotes);
                            ReadDoubleQuoteText(false);
                            ReadUntil(CssTokenType.ParenthesisClose, CssTokenType.Comma);
                        }
                        else if (currentToken.Type == CssTokenType.SingleQuotes)
                        {
                            SkipExpected(parameterToken, CssTokenType.SingleQuotes);
                            ReadSingleQuoteText(false);
                            ReadUntil(CssTokenType.ParenthesisClose, CssTokenType.Comma);
                        }
                        else
                        {
                            ReadUntil(CssTokenType.ParenthesisClose, CssTokenType.Comma);
                        }

                        TrimCurrentNode();

                        GoToParent();
                    }

                    SkipWhitespace();

                    if (currentToken.Type != CssTokenType.ParenthesisClose)
                    {
                        currentIndex++;
                    }

                    GoToParent();
                }

                SkipExpected(startToken, CssTokenType.ParenthesisClose);

                GoToParent();

                SkipWhitespace();

                //SkipExpected(CssTokenType.BraceOpen);

                AddAndSetCurrent(CssNodeType.StyleDeclarationBlock);

                ReadStyleDeclarationBlock();

                GoToParent();

                SkipWhitespace();
            }
            catch (AstGenerationException e)
            {
                Error(e.Message, e.Tokens);

                SkipToEndOfBlock();

                currentNode = oldCurrentNode;
            }
        }

        private void ReadVariable()
        {
            var oldCurrentNode = currentNode;
            var startToken = currentToken;
            try
            {
                SkipWhitespace();

                AddAndSetCurrent(CssNodeType.VariableName);

                ReadUntil(CssTokenType.Colon);
                TrimCurrentNode();

                SkipExpected(startToken, CssTokenType.Colon);

                AddOnParentAndSetCurrent(CssNodeType.VariableValue);

                SkipWhitespace();

                if (currentToken.Type == CssTokenType.DoubleQuotes)
                {
                    SkipExpected(startToken, CssTokenType.DoubleQuotes);
                    ReadDoubleQuoteText(false);
                    ReadUntil(CssTokenType.Semicolon);
                }
                else if (currentToken.Type == CssTokenType.SingleQuotes)
                {
                    SkipExpected(startToken, CssTokenType.SingleQuotes);
                    ReadSingleQuoteText(false);
                    ReadUntil(CssTokenType.Semicolon);
                }
                else
                {
                    ReadUntil(CssTokenType.Semicolon);
                }

                TrimCurrentNode();
                SkipExpected(startToken, CssTokenType.Semicolon);

                GoToParent();
            }
            catch (AstGenerationException e)
            {
                Error(e.Message, e.Tokens);

                SkipUntilLineEnd();

                currentNode = oldCurrentNode;
            }
        }

        private void ReadStyleRule()
        {
            var old = currentNode;

            try
            {
                AddAndSetCurrent(CssNodeType.Selectors);

                ReadSelectors();

                AddAndSetCurrent(CssNodeType.StyleDeclarationBlock);

                ReadStyleDeclarationBlock();
                GoToParent();
            }
            catch (AstGenerationException e)
            {
                Error(e.Message, e.Tokens);

                SkipToEndOfBlock();

                currentNode = old;
            }
        }

        private void ReadStyleDeclarationBlock()
        {
            var old = currentNode;
            var startToken = currentToken;

            try
            {

                SkipWhitespace();

                SkipExpected(startToken, CssTokenType.BraceOpen);

                SkipWhitespace();

                while (currentIndex < tokens.Count &&
                    currentToken.Type != CssTokenType.BraceClose)
                {
                    var styleDeclarationStartToken = currentToken;

                    SkipWhitespace(false);

                    if (currentToken.Type == CssTokenType.Dollar)
                    {
                        AddAndSetCurrent(CssNodeType.VariableDeclaration);

                        ReadVariable();

                        GoToParent();
                    }
                    else if (currentToken.Type == CssTokenType.At)
                    {
                        var identifier = nextToken.Text;

                        if (identifier == "include")
                        {
                            SkipExpected(styleDeclarationStartToken, CssTokenType.At);
                            SkipExpected(styleDeclarationStartToken, CssTokenType.Identifier);

                            AddAndSetCurrent(CssNodeType.MixinInclude);

                            ReadMixinInclude();

                            GoToParent();
                        }
                        else if (identifier == "extend")
                        {
                            SkipExpected(styleDeclarationStartToken, CssTokenType.At);
                            SkipExpected(styleDeclarationStartToken, CssTokenType.Identifier);

                            AddAndSetCurrent(CssNodeType.Extend);

                            ReadExtend();

                            GoToParent();
                        }
                        else if (identifier == "Property")
                        {
                            SkipExpected(styleDeclarationStartToken, CssTokenType.At);
                            SkipExpected(styleDeclarationStartToken, CssTokenType.Identifier);

                            AddAndSetCurrent(CssNodeType.PropertyTrigger);

                            ReadPropertyTrigger();

                            GoToParent();
                        }
                        else if (identifier == "Data")
                        {
                            SkipExpected(styleDeclarationStartToken, CssTokenType.At);
                            SkipExpected(styleDeclarationStartToken, CssTokenType.Identifier);

                            AddAndSetCurrent(CssNodeType.DataTrigger);

                            ReadDataTrigger();

                            GoToParent();
                        }
                        else if (identifier == "Event")
                        {
                            SkipExpected(styleDeclarationStartToken, CssTokenType.At);
                            SkipExpected(styleDeclarationStartToken, CssTokenType.Identifier);

                            AddAndSetCurrent(CssNodeType.EventTrigger);

                            ReadEventTrigger();

                            GoToParent();
                        }
                        else if (identifier == "Enter")
                        {
                            SkipExpected(styleDeclarationStartToken, CssTokenType.At);
                            SkipExpected(styleDeclarationStartToken, CssTokenType.Identifier);

                            SkipWhitespace();

                            SkipExpected(styleDeclarationStartToken, CssTokenType.Colon);

                            AddAndSetCurrent(CssNodeType.EnterAction);

                            ReadEnterOrExitAction();

                            GoToParent();
                        }
                        else if (identifier == "Exit")
                        {
                            SkipExpected(styleDeclarationStartToken, CssTokenType.At);
                            SkipExpected(styleDeclarationStartToken, CssTokenType.Identifier);

                            SkipWhitespace();

                            SkipExpected(styleDeclarationStartToken, CssTokenType.Colon);

                            AddAndSetCurrent(CssNodeType.ExitAction);

                            ReadEnterOrExitAction();

                            GoToParent();
                        }
                        else
                        {
                            throw new AstGenerationException($"ReadStyleDeclarationBlock: '@{identifier}' not supported!", GetTokens(styleDeclarationStartToken, currentToken));
                        }
                    }
                    else if (
                       currentNode.Parent.Type == CssNodeType.StyleRule &&
                       (
                           FirstTokenTypeOf(tokens, currentIndex, new[]
                           {
                           CssTokenType.Semicolon,
                           CssTokenType.BraceOpen,
                           CssTokenType.BraceClose,
                           CssTokenType.DoubleQuotes,
                           CssTokenType.SingleQuotes,
                           CssTokenType.At
                           }) == CssTokenType.BraceOpen))
                    {
                        AddAndSetCurrent(CssNodeType.StyleRule);

                        ReadStyleRule();

                        GoToParent();
                    }
                    else
                    {
                        ExpectNode(CssNodeType.StyleDeclarationBlock);

                        AddAndSetCurrent(CssNodeType.StyleDeclaration);

                        var styleDeclarationNode = currentNode;

                        AddAndSetCurrent(CssNodeType.Key);

                        var keyNode = currentNode;

                        ReadUntil(CssTokenType.Colon, CssTokenType.BraceClose, CssTokenType.Whitespace, CssTokenType.At);
                        try
                        {
                            SkipExpected(styleDeclarationStartToken, CssTokenType.Colon);

                            TrimCurrentNode();

                            AddOnParentAndSetCurrent(CssNodeType.Value);

                            SkipWhitespace(false);

                            var wasQuoted = currentToken.Type == CssTokenType.DoubleQuotes ||
                                currentToken.Type == CssTokenType.SingleQuotes;

                            if (currentToken.Type == CssTokenType.DoubleQuotes)
                            {
                                SkipExpected(styleDeclarationStartToken, CssTokenType.DoubleQuotes);
                                ReadDoubleQuoteText(false);
                            }
                            else if (currentToken.Type == CssTokenType.SingleQuotes)
                            {
                                SkipExpected(styleDeclarationStartToken, CssTokenType.SingleQuotes);
                                ReadSingleQuoteText(false);
                            }

                            ReadUntil(CssTokenType.Semicolon, CssTokenType.At, CssTokenType.BraceClose, CssTokenType.BraceOpen);

                            TrimCurrentNode();
                            if (currentNode.Text == "" &&
                                !wasQuoted)
                            {
                                throw new AstGenerationException($"No value for key '{keyNode.Text}' provided!", currentToken);
                            }

                            SkipExpected(styleDeclarationStartToken, CssTokenType.Semicolon);

                            if (currentNode.TextBuilder.Length > 0 &&
                                currentNode.Text[0] == '$')
                            {
                                var variable = currentNode.Text;
                                currentNode.TextBuilder.Clear();
                                AddAndSetCurrent(CssNodeType.VariableReference);
                                currentNode.TextBuilder.Append(variable);
                                GoToParent();
                            }

                            SkipWhitespace();
                        }
                        catch (AstGenerationException e)
                        {
                            Error(e.Message, GetTokens(styleDeclarationStartToken, ReachedEnd ? tokens[tokens.Count - 1] : currentToken));

                            SkipUntilLineEnd(CssTokenType.Semicolon, CssTokenType.BraceClose, CssTokenType.At);

                            SkipIfFound(CssTokenType.Semicolon);

                            styleDeclarationNode.Parent.RemoveChild(styleDeclarationNode);
                        }

                        GoToParent();
                        GoToParent();
                    }

                    SkipWhitespace();
                }

                SkipExpected(startToken, CssTokenType.BraceClose);

                SkipWhitespace();
            }
            catch (AstGenerationException e)
            {
                Error(e.Message, e.Tokens);

                SkipToEndOfBlock();

                currentNode = old;
            }
        }

        private bool ReachedEnd => currentIndex >= tokens.Count;

        private void ReadEnterOrExitAction()
        {
            var old = currentNode;
            try
            {
                SkipWhitespace();

                AddAndSetCurrent(CssNodeType.ActionDeclarationBlock);

                ReadActionDeclarationBlock();

                GoToParent();
            }
            catch (AstGenerationException e)
            {
                Error(e.Message, e.Tokens);

                SkipToEndOfBlock();

                currentNode = old;
            }
        }

        private void ReadActionDeclarationBlock()
        {
            var old = currentNode;
            var startToken = currentToken;

            try
            {
                SkipWhitespace();
                SkipExpected(startToken, CssTokenType.BraceOpen);
                SkipWhitespace();

                while (currentIndex < tokens.Count &&
                    currentToken.Type != CssTokenType.BraceClose)
                {
                    SkipWhitespace();

                    AddAndSetCurrent(CssNodeType.ActionDeclaration);
                    AddAndSetCurrent(CssNodeType.Key);

                    ReadUntil(CssTokenType.Colon);
                    SkipExpected(startToken, CssTokenType.Colon);
                    TrimCurrentNode();

                    AddOnParentAndSetCurrent(CssNodeType.ActionParameterBlock);

                    ReadActionParameterBlock();

                    GoToParent();

                    SkipWhitespace();

                    GoToParent();
                }

                SkipExpected(startToken, CssTokenType.BraceClose);
            }
            catch (AstGenerationException e)
            {
                Error(e.Message, e.Tokens);

                SkipToEndOfBlock();

                currentNode = old;
            }
        }

        private void ReadActionParameterBlock()
        {
            var old = currentNode;
            var startToken = currentToken;

            try
            {
                SkipWhitespace();
                SkipExpected(startToken, CssTokenType.BraceOpen);
                SkipWhitespace();

                while (currentIndex < tokens.Count &&
                    currentToken.Type != CssTokenType.BraceClose)
                {
                    var actionParameterToken = currentToken;

                    SkipWhitespace();

                    AddAndSetCurrent(CssNodeType.ActionParameter);
                    AddAndSetCurrent(CssNodeType.Key);

                    ReadUntil(CssTokenType.Colon);
                    SkipExpected(actionParameterToken, CssTokenType.Colon);
                    TrimCurrentNode();

                    AddOnParentAndSetCurrent(CssNodeType.Value);

                    SkipWhitespace();

                    if (currentToken.Type == CssTokenType.DoubleQuotes)
                    {
                        SkipExpected(actionParameterToken, CssTokenType.DoubleQuotes);
                        ReadDoubleQuoteText(false);
                        ReadUntil(CssTokenType.Semicolon);
                    }
                    else if (currentToken.Type == CssTokenType.SingleQuotes)
                    {
                        SkipExpected(actionParameterToken, CssTokenType.SingleQuotes);
                        ReadSingleQuoteText(false);
                        ReadUntil(CssTokenType.Semicolon);
                    }
                    else
                    {
                        ReadUntil(CssTokenType.Semicolon);
                    }

                    SkipExpected(actionParameterToken, CssTokenType.Semicolon);

                    SkipWhitespace();

                    GoToParent();
                    GoToParent();
                }

                SkipExpected(startToken, CssTokenType.BraceClose);
            }
            catch (AstGenerationException e)
            {
                Error(e.Message, e.Tokens);

                SkipToEndOfBlock();

                currentNode = old;
            }
        }


        private void ReadPropertyTrigger()
        {
            var old = currentNode;
            var startToken = currentToken;

            try
            {
                SkipWhitespace();

                AddAndSetCurrent(CssNodeType.PropertyTriggerProperty);
                ReadUntil(CssTokenType.Whitespace);
                SkipExpected(startToken, CssTokenType.Whitespace);
                TrimCurrentNode();

                AddOnParentAndSetCurrent(CssNodeType.PropertyTriggerValue);

                if (currentToken.Type == CssTokenType.DoubleQuotes)
                {
                    SkipExpected(startToken, CssTokenType.DoubleQuotes);
                    ReadDoubleQuoteText(false);
                    ReadUntil(CssTokenType.Whitespace);
                }
                else if (currentToken.Type == CssTokenType.SingleQuotes)
                {
                    SkipExpected(startToken, CssTokenType.SingleQuotes);
                    ReadSingleQuoteText(false);
                    ReadUntil(CssTokenType.Whitespace);
                }
                else
                {
                    ReadUntil(CssTokenType.Whitespace);
                }

                SkipExpected(startToken, CssTokenType.Whitespace);
                TrimCurrentNode();

                AddOnParentAndSetCurrent(CssNodeType.StyleDeclarationBlock);

                ReadStyleDeclarationBlock();

                GoToParent();
            }
            catch (AstGenerationException e)
            {
                Error(e.Message, e.Tokens);

                SkipToEndOfBlock();

                currentNode = old;
            }
        }

        private void ReadDataTrigger()
        {
            var old = currentNode;
            var startToken = currentToken;

            try
            {
                SkipWhitespace();

                AddAndSetCurrent(CssNodeType.DataTriggerBinding);
                ReadUntil(CssTokenType.Whitespace);
                SkipExpected(startToken, CssTokenType.Whitespace);
                TrimCurrentNode();

                AddOnParentAndSetCurrent(CssNodeType.DataTriggerValue);

                if (currentToken.Type == CssTokenType.DoubleQuotes)
                {
                    SkipExpected(startToken, CssTokenType.DoubleQuotes);
                    ReadDoubleQuoteText(false);
                    ReadUntil(CssTokenType.Whitespace);
                }
                else if (currentToken.Type == CssTokenType.SingleQuotes)
                {
                    SkipExpected(startToken, CssTokenType.SingleQuotes);
                    ReadSingleQuoteText(false);
                    ReadUntil(CssTokenType.Whitespace);
                }
                else
                {
                    ReadUntil(CssTokenType.Whitespace);
                }

                SkipExpected(startToken, CssTokenType.Whitespace);
                TrimCurrentNode();

                AddOnParentAndSetCurrent(CssNodeType.StyleDeclarationBlock);

                ReadStyleDeclarationBlock();

                GoToParent();
            }
            catch (AstGenerationException e)
            {
                Error(e.Message, e.Tokens);

                SkipToEndOfBlock();

                currentNode = old;
            }
        }

        private void ReadEventTrigger()
        {
            var old = currentNode;
            var startToken = currentToken;

            try
            {
                SkipWhitespace();

                AddAndSetCurrent(CssNodeType.EventTriggerEvent);
                ReadUntil(CssTokenType.Whitespace);
                SkipExpected(startToken, CssTokenType.Whitespace);
                TrimCurrentNode();

                AddOnParentAndSetCurrent(CssNodeType.ActionDeclarationBlock);

                ReadActionDeclarationBlock();

                GoToParent();
            }
            catch (AstGenerationException e)
            {
                Error(e.Message, e.Tokens);

                SkipToEndOfBlock();

                currentNode = old;
            }
        }

        private void ReadMixinInclude()
        {
            var old = currentNode;
            var startToken = currentToken;

            try
            {
                SkipWhitespace();

                ReadUntil(CssTokenType.ParenthesisOpen, CssTokenType.Semicolon);
                TrimCurrentNode();

                AddAndSetCurrent(CssNodeType.MixinIncludeParameters);

                if (currentToken.Type == CssTokenType.ParenthesisOpen)
                {
                    SkipExpected(startToken, CssTokenType.ParenthesisOpen);

                    while (currentIndex < tokens.Count &&
                        currentToken.Type != CssTokenType.ParenthesisClose)
                    {
                        var inclueParameterToken = currentToken;

                        SkipWhitespace();

                        AddAndSetCurrent(CssNodeType.MixinIncludeParameter);

                        ReadUntil(CssTokenType.ParenthesisClose, CssTokenType.Comma);
                        TrimCurrentNode();

                        if (currentToken.Type != CssTokenType.ParenthesisClose)
                        {
                            SkipExpected(inclueParameterToken, CssTokenType.Comma);
                        }

                        SkipWhitespace();

                        GoToParent();
                    }

                    SkipExpected(startToken, CssTokenType.ParenthesisClose);
                }

                SkipExpected(startToken, CssTokenType.Semicolon);

                GoToParent();
            }
            catch (AstGenerationException e)
            {
                Error(e.Message, e.Tokens);

                SkipToEndOfBlock();

                currentNode = old;
            }
        }

        private void ReadExtend()
        {
            var old = currentNode;
            var startToken = currentToken;

            try
            {
                SkipWhitespace();

                ReadUntil(CssTokenType.Semicolon);
                SkipExpected(currentToken, CssTokenType.Semicolon);
                SkipWhitespace();

                TrimCurrentNode();
            }
            catch (AstGenerationException e)
            {
                Error(e.Message, e.Tokens);

                SkipToEndOfBlock();

                currentNode = old;
            }
        }

        private void ReadSelectors()
        {
            var old = currentNode;
            var startToken = currentToken;
            try
            {

                while (currentIndex < tokens.Count &&
                    currentToken.Type != CssTokenType.BraceOpen)
                {
                    SkipWhitespace();

                    if (currentNode.Type == CssNodeType.Selectors)
                    {
                        AddAndSetCurrent(CssNodeType.Selector);
                    }
                    else
                    {
                        AddOnParentAndSetCurrent(CssNodeType.Selector);
                    }

                    while (currentIndex < tokens.Count &&
                        currentToken.Type != CssTokenType.BraceOpen &&
                        currentToken.Type != CssTokenType.Comma)
                    {
                        if (currentToken.Type == CssTokenType.Ampersand &&
                            currentNode.Parent?.Parent?.Parent?.Type == CssNodeType.Document)
                        {
                            Error($"Ampersand found but no parent rule!", GetTokens(startToken, currentToken));
                        }

                        if (currentNode.Type == CssNodeType.Selector)
                        {
                            AddAndSetCurrent(CssNodeType.SelectorFragment);
                        }
                        else
                        {
                            AddOnParentAndSetCurrent(CssNodeType.SelectorFragment);
                        }

                        ReadUntil(CssTokenType.BraceOpen, CssTokenType.Comma, CssTokenType.Whitespace);

                        TrimCurrentNode();

                        SkipWhitespace();

                        GoToParent();
                    }

                    SkipIfFound(CssTokenType.Comma);
                }

                SkipWhitespace();

                GoToParent();
                GoToParent();
            }
            catch (AstGenerationException e)
            {
                Error(e.Message, e.Tokens);

                SkipToEndOfBlock();

                currentNode = old;
            }
        }

        private void SkipIfFound(CssTokenType type)
        {
            if (currentIndex >= tokens.Count)
            {
                return;
            }

            if (currentToken.Type == type)
            {
                currentIndex++;
            }
        }

        private void ReadUntil(params CssTokenType[] types)
        {
            while (currentIndex < tokens.Count &&
                !types.Contains(currentToken.Type))
            {
                if (currentToken.Type == CssTokenType.Slash &&
                    nextToken.Type == CssTokenType.Asterisk)
                {
                    SkipInlineCommentText();
                }
                else if (currentToken.Type == CssTokenType.Slash &&
                    nextToken.Type == CssTokenType.Slash)
                {
                    SkipLineCommentText();
                }
                else
                {
                    currentNode.TextBuilder.Append(currentToken.Text);
                    currentIndex++;
                }
            }
        }

        public GeneratorResult GetAst(string cssDocument)
        {
            return GetAst(Tokenizer.Tokenize(cssDocument));
        }

        public GeneratorResult GetAst(List<CssToken> tokens)
        {
            errors = new List<LineInfo>();
            warnings = new List<LineInfo>();

            currentNode = new CssNode(CssNodeType.Document, null, "");

            currentIndex = 0;

            this.tokens = tokens;

            ReadDocument();

            return new GeneratorResult
            {
                Root = currentNode,
                Errors = errors,
                Warnings = warnings
            };
        }

        private void TrimCurrentNode()
        {
            var trimmed = currentNode.Text.Trim();
            currentNode.TextBuilder.Clear();
            currentNode.TextBuilder.Append(trimmed);
        }

        private void AddImportedStyle(CssNode currentNode)
        {
            var content = CssParser.cssFileProvider?.LoadFrom(currentNode.Text);

            if (content != null)
            {
                var result = new AstGenerator().GetAst(content);
                var ast = result.Root;

                var document = currentNode.Parent;

                document.AddChildren(ast.Children);
            }
            else
            {
                Error($"Cannot load '{currentNode.Text}'!", GetTokens(currentToken, currentToken));
            }
        }

        private void ReadSingleQuoteText(bool goToParent = true, bool supportMultipleLines = false)
        {
            var startToken = currentToken;
            do
            {
                if (!supportMultipleLines &&
                    tokens[currentIndex].Type == CssTokenType.Whitespace &&
                    tokens[currentIndex].Text == "\n")
                {
                    throw new AstGenerationException("Quoted text doesn't support multiple lines!", GetTokens(startToken, currentToken));
                }

                if (tokens[currentIndex].Type == CssTokenType.Backslash)
                {
                    currentIndex++;
                    currentNode.TextBuilder.Append(tokens[currentIndex].Text);
                }
                else if (tokens[currentIndex].Type == CssTokenType.SingleQuotes)
                {
                    if (goToParent)
                    {
                        currentNode = currentNode.Parent;
                    }
                    currentIndex++;
                    break;
                }
                else
                {
                    currentNode.TextBuilder.Append(tokens[currentIndex].Text);
                }
                currentIndex++;
            } while (currentIndex < tokens.Count);
        }

        private void ReadDoubleQuoteText(bool goToParent = true, bool supportMultipleLines = false)
        {
            var startToken = currentToken;
            do
            {
                if (!supportMultipleLines &&
                    tokens[currentIndex].Type == CssTokenType.Whitespace &&
                    tokens[currentIndex].Text == "\n")
                {
                    throw new AstGenerationException("Quoted text doesn't support multiple lines!", GetTokens(startToken, currentToken));
                }

                if (tokens[currentIndex].Type == CssTokenType.Backslash)
                {
                    currentIndex++;
                    currentNode.TextBuilder.Append(tokens[currentIndex].Text);
                }
                else if (tokens[currentIndex].Type == CssTokenType.DoubleQuotes)
                {
                    if (goToParent)
                    {
                        currentNode = currentNode.Parent;
                    }
                    currentIndex++;
                    break;
                }
                else
                {
                    currentNode.TextBuilder.Append(ResolveEscapedUnicodeCharacters(tokens[currentIndex]));
                }
                currentIndex++;
            } while (currentIndex < tokens.Count);
        }

        private static string ResolveEscapedUnicodeCharacters(CssToken token)
        {
            if (token.EscapedUnicodeCharacterCount == 0)
            {
                return token.Text;
            }

            var foundCharacters = 0;
            var text = token.Text;
            var textLength = text.Length;
            var resolvedText = new StringBuilder();

            for (var i = 0; i < textLength; i++)
            {
                if (text[i] == '\\' &&
                    foundCharacters < token.EscapedUnicodeCharacterCount)
                {
                    var unicode = new StringBuilder();
                    var current = (char)0;

                    i++;
                    while (i < textLength)
                    {
                        current = text[i];
                        if (current == '\\' ||
                        !(
                            (current >= 'a' && current <= 'f') ||
                            (current >= 'A' && current <= 'F') ||
                            (current >= '0' && current <= '9')
                        ))
                        {
                            break;
                        }

                        unicode.Append(text[i]);

                        i++;
                        if (unicode.Length == 6)
                        {
                            break;
                        }
                    }

                    if (i < textLength &&
                        text[i] == ' ')
                    {
                        // swallow space
                        i++;
                    }

                    i--;

                    if (uint.TryParse(unicode.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint parsedIntValue))
                    {
                        var parsedValue = (char)parsedIntValue;

                        resolvedText.Append(parsedValue);
                        foundCharacters++;
                    }
                    else
                    {
                        resolvedText.Append(unicode);
                    }
                }
                else
                {
                    resolvedText.Append(text[i]);
                }
            }

            return resolvedText.ToString();
        }

        private void SkipLineCommentText()
        {
            do
            {
                if (currentToken.Text[0] == '\n' || currentToken.Text[0] == '\r')
                {
                    break;
                }

                currentIndex++;
            } while (currentIndex < tokens.Count);
        }

        private void SkipInlineCommentText()
        {
            do
            {
                if (currentToken.Type == CssTokenType.Asterisk && nextToken.Type == CssTokenType.Slash)
                {
                    currentIndex++;
                    currentIndex++;
                    break;
                }

                currentIndex++;
            } while (currentIndex < tokens.Count);
        }

        private static CssToken Peek(List<CssToken> tokens, int currentIndex, CssTokenType type = CssTokenType.Unknown)
        {
            currentIndex++;
            if (type == CssTokenType.Unknown)
            {
                if (currentIndex >= tokens.Count)
                {
                    return default(CssToken);
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

            return default(CssToken);
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

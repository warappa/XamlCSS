using System.Collections.Generic;
using System.Linq;

namespace XamlCSS.CssParsing
{
    public class Tokenizer
    {
        public static IEnumerable<CssToken> Tokenize(string cssDocument)
        {
            var line = 1;
            var column = 1;

            var theRawTokens = new List<RawToken>();
            var currentRawToken = new RawToken()
            {
                Line = line,
                Column = column
            };
            theRawTokens.Add(currentRawToken);
            foreach (var character in cssDocument)
            {
                if (char.IsLetterOrDigit(character))
                {
                    if (currentRawToken.IsLetterOrDigit == false)
                    {
                        currentRawToken = new RawToken
                        {
                            Line = line,
                            Column = column
                        };

                        theRawTokens.Add(currentRawToken);
                    }

                    currentRawToken.Value.Append(character);
                }
                else
                {
                    currentRawToken = new RawToken
                    {
                        Line = line,
                        Column = column
                    };

                    theRawTokens.Add(currentRawToken);

                    currentRawToken.Value.Append(character);
                }

                column++;

                if (character == '\n')
                {
                    column = 1;
                    line++;
                }
            }

            theRawTokens = theRawTokens
                .Where(x => x.Value.Length > 0)
                .ToList();
            
            var strsIndex = 0;

            var tokens = new List<CssToken>();

            RawToken c;
            while (strsIndex < theRawTokens.Count)
            {
                c = theRawTokens[strsIndex++];
                CssToken t;
                var firstChar = c.Value.ToString()[0];

                if (firstChar == '@')
                {
                    t = new CssToken(CssTokenType.At, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == '{')
                {
                    t = new CssToken(CssTokenType.BraceOpen, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == '}')
                {
                    t = new CssToken(CssTokenType.BraceClose, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == ';')
                {
                    t = new CssToken(CssTokenType.Semicolon, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == ',')
                {
                    t = new CssToken(CssTokenType.Comma, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == ':')
                {
                    t = new CssToken(CssTokenType.Colon, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == '.')
                {
                    t = new CssToken(CssTokenType.Dot, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == '<')
                {
                    t = new CssToken(CssTokenType.AngleBraketOpen, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == '>')
                {
                    t = new CssToken(CssTokenType.AngleBraketClose, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == '|')
                {
                    t = new CssToken(CssTokenType.Pipe, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == '\"')
                {
                    t = new CssToken(CssTokenType.DoubleQuotes, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == '\'')
                {
                    t = new CssToken(CssTokenType.SingleQuotes, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == '(')
                {
                    t = new CssToken(CssTokenType.ParenthesisOpen, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == ')')
                {
                    t = new CssToken(CssTokenType.ParenthesisClose, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == '#')
                {
                    t = new CssToken(CssTokenType.Hash, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == '\\')
                {
                    t = new CssToken(CssTokenType.Backslash, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == '/')
                {
                    t = new CssToken(CssTokenType.Slash, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == ' ')
                {
                    t = new CssToken(CssTokenType.Whitespace, c.Value.ToString(), c.Line, c.Column);
                }
                else if (
                   firstChar == '\t' ||
                   firstChar == '\r' ||
                   firstChar == '\n'
                   )
                {
                    t = new CssToken(CssTokenType.Whitespace, c.Value.ToString(), c.Line, c.Column);
                }
                else if (firstChar == '$')
                {
                    t = new CssToken(CssTokenType.Dollar, c.Value.ToString(), c.Line, c.Column);
                }
                else
                {
                    t = new CssToken(CssTokenType.Identifier, c.Value.ToString(), c.Line, c.Column);
                }

                tokens.Add(t);
            }

            return tokens;
        }
    }
}

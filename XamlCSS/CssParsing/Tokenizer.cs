using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XamlCSS.CssParsing
{
    public class Tokenizer
    {
        private static readonly CssTokenType[] tokenTypeMap = new CssTokenType[256];

        static Tokenizer()
        {
            for (var i = 0; i < 256; i++)
            {
                tokenTypeMap[i] = ReturnTokenType((char)i);
            }
        }

        public static List<CssToken> Tokenize(string cssDocument)
        {
            if (string.IsNullOrEmpty(cssDocument))
            {
                return new List<CssToken>();
            }

            var value = new StringBuilder();
            var line = 1;
            var column = 1;

            var theRawTokens = new List<CssToken>(cssDocument.Length);

            var currentRawToken = new CssToken(CssTokenType.Unknown, "", line, column);
            currentRawToken.Type = tokenTypeMap[cssDocument[0]];

            theRawTokens.Add(currentRawToken);

            var docLength = cssDocument.Length;
            for (var i = 0; i < docLength; i++)
            {
                var character = cssDocument[i];
                if (char.IsLetterOrDigit(character))
                {
                    if (currentRawToken.IsLetterOrDigit(value) == false)
                    {
                        currentRawToken.Text = value.ToString();
                        value.Clear();

                        currentRawToken = new CssToken(CssTokenType.Unknown, "", line, column);
                        currentRawToken.Type = tokenTypeMap[character];

                        theRawTokens.Add(currentRawToken);
                    }

                    value.Append(character);
                }
                else
                {
                    if (value.Length > 0)
                    {
                        currentRawToken.Text = value.ToString();
                        value.Clear();

                        currentRawToken = new CssToken(CssTokenType.Unknown, "", line, column);
                        currentRawToken.Type = tokenTypeMap[character];

                        theRawTokens.Add(currentRawToken);
                    }

                    value.Append(character);
                }

                column++;

                if (character == '\n')
                {
                    column = 1;
                    line++;
                }
            }

            currentRawToken.Text = value.ToString();

            return theRawTokens;
        }

        private static CssTokenType ReturnTokenType(char firstChar)
        {
            if (char.IsLetterOrDigit(firstChar))
            {
                return CssTokenType.Identifier;
            }
            else if (
               firstChar == ' ' ||
               firstChar == '\t' ||
               firstChar == '\r' ||
               firstChar == '\n'
               )
            {
                return CssTokenType.Whitespace;
            }
            else if (firstChar == '{')
            {
                return CssTokenType.BraceOpen;
            }
            else if (firstChar == '}')
            {
                return CssTokenType.BraceClose;
            }
            else if (firstChar == ';')
            {
                return CssTokenType.Semicolon;
            }
            else if (firstChar == ',')
            {
                return CssTokenType.Comma;
            }
            else if (firstChar == ':')
            {
                return CssTokenType.Colon;
            }
            else if (firstChar == '\"')
            {
                return CssTokenType.DoubleQuotes;
            }
            else if (firstChar == '\'')
            {
                return CssTokenType.SingleQuotes;
            }
            else if (firstChar == '(')
            {
                return CssTokenType.ParenthesisOpen;
            }
            else if (firstChar == ')')
            {
                return CssTokenType.ParenthesisClose;
            }
            else if (firstChar == '.')
            {
                return CssTokenType.Dot;
            }
            else if (firstChar == '|')
            {
                return CssTokenType.Pipe;
            }
            else if (firstChar == '@')
            {
                return CssTokenType.At;
            }
            else if (firstChar == '<')
            {
                return CssTokenType.AngleBraketOpen;
            }
            else if (firstChar == '>')
            {
                return CssTokenType.AngleBraketClose;
            }
            else if (firstChar == '#')
            {
                return CssTokenType.Hash;
            }
            else if (firstChar == '\\')
            {
                return CssTokenType.Backslash;
            }
            else if (firstChar == '/')
            {
                return CssTokenType.Slash;
            }
            else if (firstChar == '$')
            {
                return CssTokenType.Dollar;
            }
            else if (firstChar == '*')
            {
                return CssTokenType.Asterisk;
            }
            else if (firstChar == '!')
            {
                return CssTokenType.ExclamationMark;
            }
            else if (firstChar == '?')
            {
                return CssTokenType.QuestionMark;
            }
            else if (firstChar == '-')
            {
                return CssTokenType.Minus;
            }
            else if (firstChar == '+')
            {
                return CssTokenType.Plus;
            }
            else if (firstChar == '%')
            {
                return CssTokenType.Percentage;
            }
            else if (firstChar == '[')
            {
                return CssTokenType.SquareBracketOpen;
            }
            else if (firstChar == ']')
            {
                return CssTokenType.SquareBracketClose;
            }
            else if (firstChar == '=')
            {
                return CssTokenType.Assign;
            }
            else if (firstChar == '&')
            {
                return CssTokenType.Ampersand;
            }
            else if (firstChar == '^')
            {
                return CssTokenType.Circumflex;
            }
            else if (firstChar == '_')
            {
                return CssTokenType.Underscore;
            }
            else if (firstChar == '~')
            {
                return CssTokenType.Tilde;
            }
            else
            {
                return CssTokenType.Identifier;
            }
        }
    }
}

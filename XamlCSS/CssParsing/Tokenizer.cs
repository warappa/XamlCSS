using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace XamlCSS.CssParsing
{
    public class Tokenizer
    {
        private static readonly IDictionary<char, CssTokenType> tokenTypeMap = new Dictionary<char, CssTokenType>();

        static Tokenizer()
        {
            for (var i = 0; i < 256; i++)
            {
                tokenTypeMap[(char)i] = ReturnTokenType((char)i);
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

            var rawTokens = new List<CssToken>(cssDocument.Length);

            var currentRawToken = new CssToken(CssTokenType.Unknown, "", line, column);
            currentRawToken.Type = tokenTypeMap[cssDocument[0]];

            rawTokens.Add(currentRawToken);

            var docLength = cssDocument.Length;
            for (var i = 0; i < docLength; i++)
            {
                var character = cssDocument[i];
                if (char.IsLetterOrDigit(character))
                {
                    if (currentRawToken.IsLetterOrDigit(value) == false)
                    {
                        var valueString = value.ToString();
                        if (valueString == "\\")
                        {
                            var oldI = i;
                            var escapedUnicodeCharacter = ReadEscapedUnicodeCharacter(cssDocument, ref i);
                            if (escapedUnicodeCharacter != null)
                            {
                                i++;

                                valueString = "\\" + cssDocument.Substring(oldI, i - oldI);
                                value.Clear();


                                column += i - oldI;

                                currentRawToken.Type = CssTokenType.Identifier;
                                currentRawToken.EscapedUnicodeCharacterCount++;

                                character = cssDocument[i];
                            }
                            else
                            {
                                i = oldI;
                            }
                        }

                        currentRawToken.Text = valueString;
                        value.Clear();

                        currentRawToken = new CssToken(CssTokenType.Unknown, "", line, column);
                        if (!tokenTypeMap.TryGetValue(character, out CssTokenType type))
                        {
                            tokenTypeMap[character] = ReturnTokenType(character);
                        }

                        currentRawToken.Type = tokenTypeMap[character];

                        rawTokens.Add(currentRawToken);
                    }

                    value.Append(character);
                }
                else
                {
                    // new token

                    // old token in cache?
                    if (value.Length > 0)
                    {
                        // old token
                        currentRawToken.Text = value.ToString();
                        value.Clear();

                        // new token for next round
                        currentRawToken = new CssToken(CssTokenType.Unknown, "", line, column);
                        if (!tokenTypeMap.TryGetValue(character, out CssTokenType type))
                        {
                            tokenTypeMap[character] = ReturnTokenType(character);
                        }
                        currentRawToken.Type = tokenTypeMap[character];

                        rawTokens.Add(currentRawToken);
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

            FixUnicodeEscapedIdentifiers(rawTokens);

            return rawTokens;
        }

        private static void FixUnicodeEscapedIdentifiers(List<CssToken> tokens)
        {
            CssToken previousToken = default(CssToken);
            var count = tokens.Count;

            for (var j = 0; j < count; j++)
            {
                var token = tokens[j];
                var currentTokenType = token.Type;
                if (previousToken != null &&
                    previousToken.Type == CssTokenType.Identifier &&
                    currentTokenType == CssTokenType.Identifier)
                {
                    previousToken.Text += token.Text;
                    previousToken.EscapedUnicodeCharacterCount += token.EscapedUnicodeCharacterCount;
                    tokens.RemoveAt(j);
                    count--;
                    j--;
                }
                else
                {
                    previousToken = token;
                }
            }
        }

        private static char? ReadEscapedUnicodeCharacter(string cssDocument, ref int i)
        {
            var stringB = new StringBuilder();
            var current = (char)0;
            var count = 0;
            var documentLength = cssDocument.Length;
            while (i < documentLength)
            {
                current = cssDocument[i];
                if (current == '\\' ||
                !(
                    (current >= 'a' && current <= 'f') ||
                    (current >= 'A' && current <= 'F') ||
                    (current >= '0' && current <= '9')
                ))
                {
                    break;
                }
                stringB.Append(cssDocument[i]);

                i++;
                count++;

                if (count == 6)
                {
                    // 6 hexadecimal digits is maximum
                    break;
                }
            }

            if (i < documentLength &&
                cssDocument[i] == ' ')
            {
                // swallow space
                i++;
            }

            i--;

            if (uint.TryParse(stringB.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint parsedIntValue))
            {
                return (char)parsedIntValue;
            }

            return null;
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

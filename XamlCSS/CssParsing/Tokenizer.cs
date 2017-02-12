using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace XamlCSS.CssParsing
{
    public class Tokenizer
    {
        public static IEnumerable<CssToken> Tokenize(string cssDocument)
        {
            var rawTokens = cssDocument.Split(new[] { ' ', '\t' })
                .SelectMany(x => new[] { " ", x })
                .ToList();

            rawTokens = rawTokens
                .Select(x => x == "" ? " " : x)
                .ToList();

            var newRawTokens = new List<string>(rawTokens.Count);

            var previousWasWhitespace = false;
            var previousWhitespaceCharacter = "\0";
            for (var i = 0; i < rawTokens.Count; i++)
            {
                var currentToken = rawTokens[i];
                var isWhitespace = string.IsNullOrWhiteSpace(currentToken);
                if (isWhitespace)
                {
                    if (previousWhitespaceCharacter != currentToken)
                    {
                        newRawTokens.Add(currentToken);
                    }

                    previousWhitespaceCharacter = currentToken;
                }
                else
                {
                    newRawTokens.Add(currentToken);
                    previousWhitespaceCharacter = "\0";
                }

                previousWasWhitespace = isWhitespace;
            }

            rawTokens = newRawTokens.ToList();
            
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
                .SplitThem('/')
                .SplitThem('$')
                .SplitThem('\r')
                .SplitThem('\n')
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
                else if (c == "/")
                {
                    t = new CssToken(CssTokenType.Slash, c);
                }
                else if (
                    c == " "
                    )
                {
                    t = new CssToken(CssTokenType.Whitespace, c);
                }
                else if (
                   c == "\t" ||
                   c == "\r" ||
                   c == "\n"
                   )
                {
                    t = new CssToken(CssTokenType.Whitespace, c);
                }
                else if (c == "$")
                {
                    t = new CssToken(CssTokenType.Dollar, c);
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

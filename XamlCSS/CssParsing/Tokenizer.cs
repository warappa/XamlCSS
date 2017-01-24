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
                .SplitThem('$')
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

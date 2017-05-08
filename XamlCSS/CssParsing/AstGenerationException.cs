using System;
using System.Collections.Generic;
using System.Linq;

namespace XamlCSS.CssParsing
{
    public class AstGenerationException : Exception
    {
        public AstGenerationException(string message, CssToken token)
            : base(message)
        {
            Tokens = new[] { token };
            FromLine = token.Line;
            FromColumn = token.Column;
            ToLine = token.Line;
            ToColumn = token.Column + token.Text.Length;
        }

        public AstGenerationException(string message, IEnumerable<CssToken> tokens)
            : base(message)
        {
            Tokens = tokens;
            FromLine = tokens.First().Line;
            FromColumn = tokens.First().Column;
            ToLine = tokens.Last().Line;
            ToColumn = tokens.Last().Column;
        }

        public IEnumerable<CssToken> Tokens { get; private set; }
        public int FromLine { get; private set; }
        public int FromColumn { get; private set; }
        public int ToLine { get; private set; }
        public int ToColumn { get; private set; }
        public string Text => Tokens.Select(x => x.Text).Aggregate((a, b) => a + b);
    }
}

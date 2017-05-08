using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XamlCSS.CssParsing
{
    public class LineInfo
    {
        public LineInfo(string message, IEnumerable<CssToken> tokens)
        {
            Message = message;
            Tokens = tokens;
            FromLine = tokens.First().Line;
            FromColumn = tokens.First().Column;
            ToLine = tokens.Last().Line;
            ToColumn = tokens.Last().Column;
        }

        public int FromLine { get; set; }
        public int FromColumn { get; set; }
        public int ToLine { get; set; }
        public int ToColumn { get; set; }
        public string Message { get; set; }
        public IEnumerable<CssToken> Tokens { get; set; }
        public string Text => Tokens.Select(x => x.Text).Aggregate((a, b) => a + b);
    }
}

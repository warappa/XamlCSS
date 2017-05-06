using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XamlCSS.CssParsing
{
    public class LineInfo
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Message { get; set; }
        public CssToken Token { get; set; }
    }
    public class GeneratorResult
    {
        public CssNode Root { get; set; }

        public List<LineInfo> Errors { get; set; }
        public List<LineInfo> Warnings { get; set; }
    }
}

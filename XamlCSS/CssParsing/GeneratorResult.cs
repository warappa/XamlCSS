using System.Collections.Generic;

namespace XamlCSS.CssParsing
{
    public class GeneratorResult
    {
        public CssNode Root { get; set; }

        public List<LineInfo> Errors { get; set; }
        public List<LineInfo> Warnings { get; set; }
    }
}

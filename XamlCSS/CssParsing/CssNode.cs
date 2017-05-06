using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace XamlCSS.CssParsing
{
    [DebuggerDisplay(@"{Type} ""{Text}""")]
    public class CssNode
    {
        public CssNode(CssNodeType type, CssNode parent, string text)
        {
            Type = type;
            Parent = parent;
            TextBuilder = new StringBuilder(text);
        }

        public CssNode(CssNodeType type)
        {
            Type = type;
            TextBuilder = new StringBuilder();
        }

        public CssNodeType Type { get; set; }
        public StringBuilder TextBuilder { get; set; }
        private string text = null;

        public string Text => text != null && text.Length == TextBuilder.Length ?
            text :
            (text = TextBuilder.ToString());
        public CssNode Parent { get; set; }

        public List<CssNode> Children { get; set; } = new List<CssNode>();
    }
}

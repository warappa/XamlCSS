using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace XamlCSS.CssParsing
{
    [DebuggerDisplay(@"{Type} ""{Text}"" {FlatChildren}")]
    public class CssNode
    {
        public CssNode(CssNodeType type, CssNode parent, string text)
        {
            Type = type;
            Parent = parent;
            TextBuilder = new StringBuilder(text);
        }

        public CssNodeType Type { get; set; }
        public StringBuilder TextBuilder { get; set; }
        public string Text => TextBuilder.ToString();
        public CssNode Parent { get; set; }

        public List<CssNode> Children { get; set; } = new List<CssNode>();
        
        private string FlatChildren
        {
            get
            {
                return string.Join(" ", Children.Select(x => "(" + x.Type + ")" + x.Text));
            }
        }

        private IEnumerable<string> AllChildrenText(IEnumerable<CssNode> nodes, int level = 0)
        {
            return nodes
                .SelectMany(x =>
                    new[] { new String(' ', level * 5), x.Text }
                        .Concat(AllChildrenText(x.Children, level + 1)))
                .ToList();
        }
    }
}

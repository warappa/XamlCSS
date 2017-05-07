using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        private List<CssNode> children = new List<CssNode>();
        public IEnumerable<CssNode> Children { get => children; }

        private Dictionary<string, CssNode> cachedVariables;
        internal CssNode GetVariableDeclaration(string variableName)
        {
            if (cachedVariables == null)
            {
                cachedVariables = new Dictionary<string, CssNode>();

                var variableDeclarations = Children
                        .Where(x => x.Type == CssNodeType.VariableDeclaration)
                            .ToList();

                foreach (var declaration in variableDeclarations)
                {
                    cachedVariables[declaration.Children.First(y => y.Type == CssNodeType.VariableName).Text] = declaration;
                }
            }

            CssNode foundVariableDeclaration;

            if (cachedVariables.TryGetValue(variableName, out foundVariableDeclaration))
            {
                return foundVariableDeclaration;
            }

            return null;
        }

        internal void AddChildren(IEnumerable<CssNode> children)
        {
            foreach (var child in children)
            {
                AddChild(child);
            }
        }

        internal void AddChild(CssNode child)
        {
            children.Add(child);

            if (child.Type == CssNodeType.VariableDeclaration)
            {
                cachedVariables = null;
            }
        }

        internal void RemoveChild(CssNode child)
        {
            children.Remove(child);

            if (child.Type == CssNodeType.VariableDeclaration)
            {
                cachedVariables = null;
            }
        }
    }
}

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace XamlCSS
{
    [DebuggerDisplay("{SelectorString}")]
    public class StyleRule
    {
        private string selectorString = null;
        private List<Selector> selectors = new List<Selector>();

        public string SelectorString=>
                    selectorString ?? (selectorString = string.Join(",", Selectors.Select(x => x.Value)));
        
        public List<Selector> Selectors
        {
            get => selectors;
            set
            {
                selectors = value;
                selectorString = null;
            }
        }

        public StyleDeclarationBlock DeclarationBlock { get; set; } = new StyleDeclarationBlock();
        public SelectorType SelectorType { get; set; } = SelectorType.LogicalTree;
    }
}

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace XamlCSS
{
    [DebuggerDisplay("{SelectorString}")]
    public class StyleRule
    {
        private string selectorString = null;
        public string SelectorString
        {
            get
            {
                    return selectorString = string.Join(",", Selectors.Select(x => x.Value));
            }
        }

        private List<Selector> selectors = new List<Selector>();
        public List<Selector> Selectors
        {
            get
            {
                return selectors;
            }
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

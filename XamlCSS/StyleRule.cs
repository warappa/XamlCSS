using System.Diagnostics;
using System.Linq;

namespace XamlCSS
{
    [DebuggerDisplay("{SelectorString}")]
    public class StyleRule
    {
        private string selectorString = null;
        private SelectorCollection selectors = new SelectorCollection();

        public string SelectorString=>
                    selectorString ?? (selectorString = string.Join(",", Selectors.Select(x => x.Value)));
        
        public SelectorCollection Selectors
        {
            get => selectors;
            set
            {
                selectors = value;
                selectorString = null;
            }
        }

        public StyleDeclarationBlock DeclarationBlock { get; set; } = new StyleDeclarationBlock();
    }
}

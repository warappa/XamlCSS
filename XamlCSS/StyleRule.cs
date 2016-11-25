using System.Collections.Generic;

namespace XamlCSS
{
    public class StyleRule
	{
		public string SelectorString { get; set; }
        public List<Selector> Selectors { get; set; } = new List<Selector>();
		public StyleDeclarationBlock DeclarationBlock { get; set; } = new StyleDeclarationBlock();
		public SelectorType SelectorType { get; set; } = SelectorType.LogicalTree;
	}
}

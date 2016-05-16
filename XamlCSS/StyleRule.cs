using System.Collections.Generic;

namespace XamlCSS
{
	public class StyleRule
	{
		public string Selector { get; set; }
		public StyleDeclarationBlock DeclarationBlock { get; set; } = new StyleDeclarationBlock();
		public SelectorType SelectorType { get; set; } = SelectorType.LogicalTree;
	}
}

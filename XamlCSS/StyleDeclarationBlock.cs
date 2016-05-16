using System.Collections.Generic;

namespace XamlCSS
{
	public class StyleDeclarationBlock : List<StyleDeclaration>
	{
		public StyleDeclarationBlock()
		{

		}
		public StyleDeclarationBlock(IEnumerable<StyleDeclaration> collection) : base(collection)
		{
		}
	}
}

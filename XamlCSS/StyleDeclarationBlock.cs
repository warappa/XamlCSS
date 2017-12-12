using System.Collections.Generic;
using System.Linq;

namespace XamlCSS
{
    public class StyleDeclarationBlock : List<StyleDeclaration>
    {
        public StyleDeclarationBlock()
        {
            Triggers = new List<ITrigger>();
        }
        public StyleDeclarationBlock(IEnumerable<StyleDeclaration> collection, IEnumerable<ITrigger> triggers = null)
            : base(collection)
        {
            Triggers = triggers?.ToList() ?? new List<ITrigger>();
        }

        public List<ITrigger> Triggers { get; set; }
    }
}

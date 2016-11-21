using System.Collections.Generic;
using System.Linq;

namespace XamlCSS
{
    public class Selector
    {
        protected string val;
        public string Value
        {
            get
            {
                return val;
            }
            set
            {
                val = value;

                IdSpecificity = value.ToCharArray().Count(x => x == '#');
                ClassSpecificity = value.ToCharArray().Count(x => x == '.');
                var a = value.Split(' ').Count(x => !x.StartsWith(".") && !x.StartsWith("#"));
                SimpleSpecificity = a;
            }
        }
        public int SimpleSpecificity { get; set; }
        public int ClassSpecificity { get; set; }
        public int IdSpecificity { get; set; }

        public string Specificity
        {
            get
            {
                if (IdSpecificity > 0)
                {
                    return $"{IdSpecificity},{ClassSpecificity},{SimpleSpecificity}";
                }
                else if (ClassSpecificity > 0)
                {
                    return $"{ClassSpecificity},{SimpleSpecificity}";
                }
                else
                {
                    return $"{SimpleSpecificity}";
                }
            }
        }
    }

    public class StyleRule
	{
		public string SelectorString { get; set; }
        public List<Selector> Selectors { get; set; } = new List<Selector>();
		public StyleDeclarationBlock DeclarationBlock { get; set; } = new StyleDeclarationBlock();
		public SelectorType SelectorType { get; set; } = SelectorType.LogicalTree;
	}
}

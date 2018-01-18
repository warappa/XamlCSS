using System;
using System.Linq;

namespace XamlCSS
{
    public class SpecificityCalculator
    {
        public static SpecificityResult Calculate(string expression)
        {
            var charArray = expression.ToCharArray();

            var idSpecificity = charArray.Count(x => x == '#');
            var classSpecificity = charArray.Count(x => x == '.' || x == ':' || x == '[');

            var simpleSpecifitySplit = expression.Split(new[] { ' ', '>', '*' }, StringSplitOptions.RemoveEmptyEntries);
            var simpleSpecificity = simpleSpecifitySplit.Count(x => !x.StartsWith(".", StringComparison.Ordinal) && !x.StartsWith("#", StringComparison.Ordinal));

            return new SpecificityResult(idSpecificity, classSpecificity, simpleSpecificity);
        }

        public class SpecificityResult
        {
            public SpecificityResult(int idSpecificity, int classSpecificity, int simpleSpecificity)
            {
                IdSpecificity = idSpecificity;
                ClassSpecificity = classSpecificity;
                SimpleSpecificity = simpleSpecificity;
            }

            public int SimpleSpecificity { get; private set; }
            public int ClassSpecificity { get; private set; }
            public int IdSpecificity { get; private set; }
        }
    }
}
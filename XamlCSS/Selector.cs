using System.Diagnostics;
using System.Linq;

namespace XamlCSS
{
    [DebuggerDisplay("{Value}")]
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
}
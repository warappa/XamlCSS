using System;
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

                var charArray = value.ToCharArray();

                IdSpecificity = charArray.Count(x => x == '#');
                ClassSpecificity = charArray.Count(x => x == '.' || x == ':' || x == '[');

                var simpleSpecifitySplit = value.Split(new[] { ' ', '>', '*' }, StringSplitOptions.RemoveEmptyEntries);
                SimpleSpecificity = simpleSpecifitySplit.Count(x => !x.StartsWith(".") && !x.StartsWith("#"));
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

        public static bool operator <(Selector e1, Selector e2)
        {
            if (e1.IdSpecificity < e2.IdSpecificity)
                return true;
            if (e1.ClassSpecificity < e2.ClassSpecificity)
                return true;
            if (e1.SimpleSpecificity < e2.SimpleSpecificity)
                return true;

            return false;
        }

        public static bool operator >(Selector e1, Selector e2)
        {
            if (e1.IdSpecificity > e2.IdSpecificity)
                return true;
            if (e1.ClassSpecificity > e2.ClassSpecificity)
                return true;
            if (e1.SimpleSpecificity > e2.SimpleSpecificity)
                return true;

            return false;
        }
    }
}
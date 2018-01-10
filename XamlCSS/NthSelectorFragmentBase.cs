using System.Text.RegularExpressions;
using XamlCSS.CssParsing;

namespace XamlCSS
{
    public abstract class NthSelectorFragmentBase : SelectorFragment
    {
        private static Regex nthRegex = new Regex(@"((?<factor>[0-9]+)?(?<n>n))?(?<distance>([\+\-]?[0-9]+))?");

        protected int factor;
        protected int distance;

        public NthSelectorFragmentBase(CssNodeType type, string text)
            : base(type, text)
        {
            Text = GetParameterExpression(text);

            GetFactorAndDistance(Text, out factor, out distance);
        }

        protected abstract string GetParameterExpression(string expression);

        protected bool CalcIsNth(int factor, int distance, ref int thisPosition)
        {
            thisPosition = thisPosition - distance;

            if (thisPosition < 0)
            {
                return false;
            }

            return (factor != 0 ? thisPosition % factor : thisPosition) == 0;
        }

        protected void GetFactorAndDistance(string expression, out int factor, out int distance)
        {
            factor = 0;
            distance = 0;

            if (string.IsNullOrWhiteSpace(expression))
            {
                return;
            }

            if (expression == "even")
            {
                factor = 2;
            }
            else if (expression == "odd")
            {
                factor = 2;
                distance = 1;
            }
            else
            {
                var matchResult = nthRegex.Match(expression);

                int.TryParse(matchResult.Groups["factor"]?.Value ?? "", out factor);
                int.TryParse(matchResult.Groups["distance"]?.Value ?? "", out distance);

                if (factor == 0 &&
                    matchResult.Groups["n"].Success)
                {
                    factor = 1;
                }
            }
        }
    }
}
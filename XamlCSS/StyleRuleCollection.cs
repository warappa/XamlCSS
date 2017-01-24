using System.Collections.Generic;

namespace XamlCSS
{
    public class StyleRuleCollection : List<StyleRule>
    {
        public StyleRuleCollection() { }
        public StyleRuleCollection(IEnumerable<StyleRule> rules)
            : base(rules)
        {

        }
    }
}

using System.Collections.Generic;
using System.Linq;
using XamlCSS.CssParsing;

namespace XamlCSS
{
    public class MergedStyleSheet : SingleStyleSheet
    {
        public MergedStyleSheet()
        {
        }

        public MergedStyleSheet(SingleStyleSheet styleSheet)
        {
            this.Namespaces = styleSheet.Namespaces;
            this.Rules = styleSheet.Rules;
        }

        protected List<SingleStyleSheet> styleSheets = new List<SingleStyleSheet>();
        virtual public List<SingleStyleSheet> StyleSheets
        {
            get
            {
                return styleSheets;
            }
            set
            {
                styleSheets = value;

                combinedRules = null;
                combinedNamespaces = null;
            }
        }

        protected List<CssNamespace> combinedNamespaces;
        protected StyleRuleCollection combinedRules;


        override public List<CssNamespace> Namespaces
        {
            get
            {
                return combinedNamespaces ?? (combinedNamespaces = GetCombinedNamespaces());
            }
            set
            {
                namespaces = value;
                combinedNamespaces = null;
            }
        }

        private List<CssNamespace> GetCombinedNamespaces()
        {
            if (StyleSheets.Count == 0)
            {
                return namespaces;
            }

            return StyleSheets
                                .Select(x => x.Namespaces)
                                .Aggregate((a, b) => a.Concat(b).ToList())
                                .Concat(namespaces)
                                .GroupBy(x => x.Alias)
                                .Select(x => x.Last())
                                .ToList();
        }

        override public StyleRuleCollection Rules
        {
            get
            {
                return combinedRules ?? (combinedRules = new StyleRuleCollection(GetCombinedStyleRules()));
            }
            set
            {
                rules = value;
                combinedRules = null;
            }
        }

        protected List<StyleRule> GetCombinedStyleRules()
        {
            if (StyleSheets.Count == 0)
            {
                return rules;
            }

            return StyleSheets
                    .Select(x => x.Rules.ToList())
                    .Aggregate((a, b) => a.Concat(b).ToList())
                    .Concat(rules)
                    .GroupBy(x => x.SelectorString)
                    .Select(x => new StyleRule
                    {
                        SelectorString = x.Key,
                        Selectors = x.First().Selectors,
                        SelectorType = x.First().SelectorType,
                        DeclarationBlock = new StyleDeclarationBlock(GetMergedStyleDeclarations(x.ToList()))
                    })
                    .ToList();
        }

        protected List<StyleDeclaration> GetMergedStyleDeclarations(List<StyleRule> styleRules)
        {
            return styleRules
                .SelectMany(x => x.DeclarationBlock.ToList())
                .GroupBy(x => x.Property)
                .Select(x => x.Last())
                .ToList();
        }
    }
}

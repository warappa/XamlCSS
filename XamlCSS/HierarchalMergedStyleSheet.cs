using System.Collections.Generic;
using System.Linq;

namespace XamlCSS
{
    public class HierarchalMergedStyleSheet : MergedStyleSheet
    {
        protected List<StyleSheet> GetParentStyleSheets(object from)
        {
            List<StyleSheet> styleSheets = new List<StyleSheet>();

            var current = GetParent(from);
            while (current != null)
            {
                var styleSheet = GetStyleSheet(current);
                if (styleSheet != null)
                {
                    styleSheets.Add(styleSheet);
                }
                current = GetParent(current);
            }

            return styleSheets;
        }

        override public List<StyleSheet> StyleSheets
        {
            get
            {
                return styleSheets ?? (styleSheets = GetParentStyleSheets(AttachedTo).Reverse<StyleSheet>().ToList());
            }
            set
            {
                styleSheets = value;

                combinedRules = null;
                combinedNamespaces = null;
            }
        }

        override public object AttachedTo
        {
            get
            {
                return base.AttachedTo;
            }
            set
            {
                base.AttachedTo = value;

                styleSheets = null;
                combinedRules = null;
                combinedNamespaces = null;
            }
        }
    }
}

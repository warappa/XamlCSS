using System.Collections.Generic;
using System.Linq;

namespace XamlCSS
{
    public class StyleSheet : MergedStyleSheet
    {
        protected List<SingleStyleSheet> GetParentStyleSheets(object from)
        {
            List<SingleStyleSheet> styleSheets = new List<SingleStyleSheet>();

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

        override public List<SingleStyleSheet> StyleSheets
        {
            get
            {
                return styleSheets ?? (styleSheets = GetParentStyleSheets(AttachedTo).Reverse<SingleStyleSheet>().ToList());
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

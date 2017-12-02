using System.Collections.Generic;

namespace XamlCSS
{
    public class StyleSheetCollection : List<StyleSheet>
    {
        public StyleSheetCollection()
        {
        }

        public StyleSheetCollection(IEnumerable<StyleSheet> collection) : base(collection)
        {
        }

        public StyleSheetCollection(int capacity) : base(capacity)
        {
        }
    }
}

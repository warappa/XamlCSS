using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XamlCSS
{
    public class StyleSheetCollection : ObservableCollection<StyleSheet>
    {
        public StyleSheetCollection()
        {
        }

        public StyleSheetCollection(IEnumerable<StyleSheet> collection) : base(collection)
        {
        }
    }
}

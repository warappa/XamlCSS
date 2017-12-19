using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XamlCSS
{
    public class SelectorCollection : ObservableCollection<Selector>
    {
        public SelectorCollection()
        {
        }

        public SelectorCollection(IEnumerable<Selector> collection) : base(collection)
        {
        }
    }
}

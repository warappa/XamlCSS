using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XamlCSS
{
    public class SelectorCollection : ObservableCollection<ISelector>
    {
        public SelectorCollection()
        {
        }

        public SelectorCollection(IEnumerable<ISelector> collection) : base(collection)
        {
        }
    }
}

using System.Collections.Generic;

namespace XamlCSS
{
    public class SelectorCollection : List<Selector>
    {
        public SelectorCollection()
        {
        }

        public SelectorCollection(IEnumerable<Selector> collection) : base(collection)
        {
        }

        public SelectorCollection(int capacity) : base(capacity)
        {
        }
    }
}

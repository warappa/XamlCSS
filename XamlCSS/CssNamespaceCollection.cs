using System.Collections.Generic;
using XamlCSS.CssParsing;

namespace XamlCSS
{
    public class CssNamespaceCollection : List<CssNamespace>
    {
        public CssNamespaceCollection()
        {
        }

        public CssNamespaceCollection(IEnumerable<CssNamespace> collection) : base(collection)
        {
        }

        public CssNamespaceCollection(int capacity) : base(capacity)
        {
        }
    }
}

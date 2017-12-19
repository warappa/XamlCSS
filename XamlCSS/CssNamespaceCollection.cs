using System.Collections.Generic;
using System.Collections.ObjectModel;
using XamlCSS.CssParsing;

namespace XamlCSS
{
    public class CssNamespaceCollection : ObservableCollection<CssNamespace>
    {
        public CssNamespaceCollection()
        {
        }

        public CssNamespaceCollection(IEnumerable<CssNamespace> collection) : base(collection)
        {
        }
    }
}

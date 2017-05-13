using System.Collections.Generic;
using XamlCSS.CssParsing;

namespace XamlCSS
{
    public interface IMarkupExtensionParser
    {
        object ProvideValue(string expression, object targetObject, IEnumerable<CssNamespace> namespaces);
    }
}

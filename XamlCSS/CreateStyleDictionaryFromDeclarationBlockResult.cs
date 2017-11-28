using System.Collections.Generic;

namespace XamlCSS
{
    public class CreateStyleDictionaryFromDeclarationBlockResult<TDependencyProperty>
        where TDependencyProperty : class
    {
        public Dictionary<TDependencyProperty, object> PropertyStyleValues { get; private set; } = new Dictionary<TDependencyProperty, object>();
        public List<string> Errors { get; private set; } = new List<string>();
    }
}

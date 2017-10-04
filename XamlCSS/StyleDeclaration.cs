using System.Diagnostics;

namespace XamlCSS
{
    [DebuggerDisplay("{Property}: {Value}")]
    public class StyleDeclaration
    {
        public string Property { get; set; }
        public string Value { get; set; }
    }
}

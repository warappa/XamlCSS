using System;
using System.Diagnostics;

namespace XamlCSS
{
    [DebuggerDisplay("{MatchedType.Name} {Rule.SelectorString}")]
    class StyleMatchInfo
    {
        public StyleRule Rule { get; set; }
        public Type MatchedType { get; set; }
    }
}

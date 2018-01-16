using System;
using System.Collections.Generic;

namespace XamlCSS
{
    public class StyleUpdateInfo
    {
        public StyleSheet OldStyleSheet { get; set; }
        public List<string> OldMatchedSelectors { get; set; }
        public StyleSheet CurrentStyleSheet { get; set; }
        public List<string> CurrentMatchedSelectors { get; set; } = new List<string>();
        public Type MatchedType { get; internal set; }
        public SelectorType DoMatchCheck { get; internal set; }
    }
}

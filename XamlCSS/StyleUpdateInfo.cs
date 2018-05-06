using System;
using System.Collections.Generic;
using XamlCSS.Utils;

namespace XamlCSS
{
    public class StyleUpdateInfo
    {
        public StyleSheet OldStyleSheet { get; set; }
        public LinkedHashSet<ISelector> OldMatchedSelectors { get; set; }
        public StyleSheet CurrentStyleSheet { get; set; }
        public LinkedHashSet<ISelector> CurrentMatchedSelectors { get; set; } = new LinkedHashSet<ISelector>();
        public LinkedHashSet<string> CurrentMatchedResourceKeys { get; set; } = new LinkedHashSet<string>();
        public Type MatchedType { get; internal set; }
        public SelectorType DoMatchCheck { get; internal set; }
    }
}

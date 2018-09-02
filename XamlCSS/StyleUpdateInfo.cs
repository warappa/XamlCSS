using System;
using System.Collections.Generic;
using XamlCSS.Utils;

namespace XamlCSS
{
    public class StyleUpdateInfo
    {
        public StyleSheet OldStyleSheet { get; set; }
        public IList<ISelector> OldMatchedSelectors { get; set; } = new List<ISelector>();
        public StyleSheet CurrentStyleSheet { get; set; }
        public IList<ISelector> CurrentMatchedSelectors { get; set; } = new List<ISelector>(); // new LinkedHashSet<ISelector>();
        public IList<string> CurrentMatchedResourceKeys { get; set; } = new List<string>(); // new LinkedHashSet<string>();
        public Type MatchedType { get; internal set; }
        public SelectorType DoMatchCheck { get; internal set; }
        public object InitialStyle { get; internal set; }
    }
}

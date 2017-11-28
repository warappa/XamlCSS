using System;
using System.Diagnostics;

namespace XamlCSS
{
    public partial class BaseCss<TDependencyObject, TUIElement, TStyle, TDependencyProperty>
        where TDependencyObject : class
        where TUIElement : class, TDependencyObject
        where TStyle : class
        where TDependencyProperty : class
    {
        [DebuggerDisplay("{MatchedType.Name} {Rule.SelectorString}")]
        public class StyleMatchInfo
        {
            public StyleRule Rule { get; set; }
            public Type MatchedType { get; set; }
        }
    }
}

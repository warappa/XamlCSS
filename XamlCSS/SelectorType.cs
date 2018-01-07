using System;

namespace XamlCSS
{
    [Flags]
    public enum SelectorType
    {
        None = 0,
        VisualTree = 1,
        LogicalTree = 2
    }
}

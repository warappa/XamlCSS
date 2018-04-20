using System.Collections.Generic;

namespace XamlCSS.Tests.Dom
{
    public class UIElement
    {
        public string Id { get; set; }
        public string Class { get; set; } = "";
        public List<UIElement> Children { get; set; } = new List<UIElement>();
    }
}
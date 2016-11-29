using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace XamlCSS.XamarinForms
{
    public class StyleService : INativeStyleService<Style, BindableObject, BindableProperty>
    {
        protected const string StyleSheetStyleKey = "StyleSheetStyle";

        public Style CreateFrom(IDictionary<BindableProperty, object> dict, Type forType)
        {
            Style style = null;
            if (forType != null)
                style = new Style(forType);
            else
                style = new Style(typeof(Element));

            foreach (var i in dict)
            {
                style.Setters.Add(new Setter() { Property = i.Key, Value = i.Value });
            }

            return style;
        }

        public IDictionary<BindableProperty, object> GetStyleAsDictionary(Style style)
        {
            if (style == null)
            {
                return null;
            }

            return style.Setters.OfType<Setter>().ToDictionary(x => x.Property, x => x.Value);
        }

        public void SetStyle(BindableObject visualElement, Style s)
        {
            if (visualElement is VisualElement)
                (visualElement as VisualElement).Style = s;
        }

        public string GetStyleResourceKey(Type type, string selector)
        {
            return $"{StyleSheetStyleKey}_${type.FullName}_{selector}";
        }

        public string BaseStyleResourceKey { get { return StyleSheetStyleKey; } }
    }
}

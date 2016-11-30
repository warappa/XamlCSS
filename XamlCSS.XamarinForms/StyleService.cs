using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace XamlCSS.XamarinForms
{
    public class StyleService : StyleServiceBase<Style, BindableObject, BindableProperty>
    {
        protected override void AddSetter(Style style, BindableProperty property, object value)
        {
            style.Setters.Add(new Setter { Property = property, Value = value });
        }

        protected override Style CreateStyle(Type forType)
        {
            Style style;

            if (forType != null)
            {
                style = new Style(forType);
            }
            else
            {
                style = new Style(typeof(Element));
            }

            return style;
        }

        public override IDictionary<BindableProperty, object> GetStyleAsDictionary(Style style)
        {
            if (style == null)
            {
                return null;
            }

            return style.Setters.ToDictionary(x => x.Property, x => x.Value);
        }

        public override void SetStyle(BindableObject visualElement, Style style)
        {
            if (visualElement is VisualElement)
            {
                (visualElement as VisualElement).Style = style;
            }
        }
    }
}

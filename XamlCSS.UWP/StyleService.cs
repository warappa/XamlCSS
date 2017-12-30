using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;

namespace XamlCSS.UWP
{
    public class StyleService : StyleServiceBase<Style, DependencyObject, DependencyProperty>
    {
        private IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyService;

        public StyleService(IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyService)
        {
            this.dependencyService = dependencyService;
        }

        public override Style CreateFrom(IDictionary<DependencyProperty, object> dict, IEnumerable<DependencyObject> triggers, Type forType)
        {
            var style = base.CreateFrom(dict, triggers, forType);

            RemoveInvalidSetterValues(style);

            return style;
        }

        private static void RemoveInvalidSetterValues(Style style)
        {
            var setters = style.Setters
                .Cast<Setter>()
                .ToList();

            for (var i = 0; i < setters.Count; i++)
            {
                try
                {
                    var test = setters[i].Value;
                }
                catch(Exception exc)
                {
                    setters.RemoveAt(i);
                    style.Setters.RemoveAt(i);
                    i--;
                }
            }
        }

        protected override void AddTrigger(Style style, DependencyObject trigger)
        {
            throw new Exception("Triggers are not supported on UWP!");
        }

        public override IEnumerable<DependencyObject> GetTriggersAsList(Style style)
        {
            return new List<DependencyObject>();
        }

        public override DependencyObject CreateTrigger(StyleSheet styleSheet, ITrigger trigger, Type targetType, DependencyObject styleResourceReferenceHolder)
        {
            throw new Exception("Triggers are not supported on UWP!");
        }
        protected override void AddSetter(Style style, DependencyProperty property, object value)
        {
            style.Setters.Add(new Setter(property, value));
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
                style = new Style(typeof(FrameworkElement));
            }

            return style;
        }

        public override IDictionary<DependencyProperty, object> GetStyleAsDictionary(Style style)
        {
            if (style == null)
            {
                return null;
            }
            return style.Setters.OfType<Setter>().ToDictionary(x => x.Property, x => x.Value);
        }

        public override void SetStyle(DependencyObject visualElement, Style style)
        {
            if (visualElement is FrameworkElement)
            {
                (visualElement as FrameworkElement).Style = style;
            }
        }

        public override Style GetStyle(DependencyObject visualElement)
        {
            if (visualElement is FrameworkElement)
            {
                return (visualElement as FrameworkElement).Style;
            }

            return null;
        }
    }
}

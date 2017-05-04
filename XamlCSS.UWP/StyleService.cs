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

        protected override void AddTrigger(Style style, DependencyObject trigger)
        {
            throw new Exception("Triggers are not supported on UWP!");
        }

        public override IEnumerable<DependencyObject> GetTriggersAsList(Style style)
        {
            // throw new Exception("Triggers are not supported on UWP!");
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
    }
}

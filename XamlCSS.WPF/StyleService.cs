using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace XamlCSS.WPF
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
            style.Triggers.Add((System.Windows.TriggerBase)trigger);
        }

        public override IEnumerable<DependencyObject> GetTriggersAsList(Style style)
        {
            return style.Triggers;
        }

        public override DependencyObject CreateTrigger(ITrigger trigger, Type targetType)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            if (trigger is Trigger)
            {
                var propertyTrigger = trigger as Trigger;
                var nativeTrigger = new System.Windows.Trigger();

                nativeTrigger.Property = dependencyService.GetBindableProperty(targetType, propertyTrigger.Property);

                if (nativeTrigger.Property == null)
                {
                    throw new NullReferenceException($"Property '{propertyTrigger.Property}' may not be null!");
                }

                nativeTrigger.Value = dependencyService.GetBindablePropertyValue(targetType, nativeTrigger.Property, propertyTrigger.Value);

                foreach (var i in propertyTrigger.StyleDeclaraionBlock)
                {
                    var property = dependencyService.GetBindableProperty(targetType, i.Property);
                    var value = dependencyService.GetBindablePropertyValue(targetType, property, i.Value);

                    nativeTrigger.Setters.Add(new Setter { Property = property, Value = value });
                }

                return nativeTrigger;
            }

            throw new NotSupportedException($"Trigger '{trigger.GetType().FullName}' is not supported!");
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
                style = new Style();
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
            else if (visualElement is FrameworkContentElement)
            {
                (visualElement as FrameworkContentElement).Style = style;
            }
        }
    }
}

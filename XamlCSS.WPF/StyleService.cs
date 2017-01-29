using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace XamlCSS.WPF
{
    public class StyleService : StyleServiceBase<Style, DependencyObject, DependencyProperty>
    {
        private IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyService;
        private IMarkupExtensionParser markupExtensionParser;

        public StyleService(IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyService,
            IMarkupExtensionParser markupExtensionParser)
        {
            this.dependencyService = dependencyService;
            this.markupExtensionParser = markupExtensionParser;
        }

        protected override void AddTrigger(Style style, DependencyObject trigger)
        {
            style.Triggers.Add((System.Windows.TriggerBase)trigger);
        }

        public override IEnumerable<DependencyObject> GetTriggersAsList(Style style)
        {
            return style.Triggers;
        }

        private static object GetBasicValue(DataTrigger dataTrigger, object valueExpression)
        {
            if (Int32.TryParse(dataTrigger.Value, out int intValue))
            {
                valueExpression = intValue;
            }
            else if (Double.TryParse(dataTrigger.Value, out double doubleValue))
            {
                valueExpression = doubleValue;
            }
            else if (bool.TryParse(dataTrigger.Value, out bool boolValue))
            {
                valueExpression = boolValue;
            }

            return valueExpression;
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
            else if (trigger is DataTrigger)
            {
                var dataTrigger = trigger as DataTrigger;
                var nativeTrigger = new System.Windows.DataTrigger();

                var expression = "{Binding " + dataTrigger.Binding + "}";

                var binding = (System.Windows.Data.BindingBase)markupExtensionParser.ProvideValue(expression, null);
                nativeTrigger.Binding = binding;

                object valueExpression = dataTrigger.Value;
                valueExpression = GetBasicValue(dataTrigger, valueExpression);
                nativeTrigger.Value = valueExpression;

                foreach (var i in dataTrigger.StyleDeclaraionBlock)
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

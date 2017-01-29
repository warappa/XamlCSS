using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamlCSS.XamarinForms
{
    public class StyleService : StyleServiceBase<Style, BindableObject, BindableProperty>
    {
        private IDependencyPropertyService<BindableObject, BindableObject, Style, BindableProperty> dependencyService;
        private IMarkupExtensionParser markupExtensionParser;

        public StyleService(IDependencyPropertyService<BindableObject, BindableObject, Style, BindableProperty> dependencyService,
            IMarkupExtensionParser markupExtensionParser)
        {
            this.dependencyService = dependencyService;
            this.markupExtensionParser = markupExtensionParser;
        }
        protected override void AddSetter(Style style, BindableProperty property, object value)
        {
            style.Setters.Add(new Setter { Property = property, Value = value });
        }

        protected override void AddTrigger(Style style, BindableObject trigger)
        {
            style.Triggers.Add((Xamarin.Forms.TriggerBase)trigger);
        }

        public override IEnumerable<BindableObject> GetTriggersAsList(Style style)
        {
            return style.Triggers;
        }

        public override BindableObject CreateTrigger(ITrigger trigger, Type targetType)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            if (trigger is Trigger)
            {
                var propertyTrigger = trigger as Trigger;
                var nativeTrigger = new Xamarin.Forms.Trigger(targetType);
                nativeTrigger.Property = dependencyService.GetBindableProperty(targetType, propertyTrigger.Property);
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
                var nativeTrigger = new Xamarin.Forms.DataTrigger(targetType);

                var expression = "{Binding " + dataTrigger.Binding + "}";

                var binding = (Binding)markupExtensionParser.ProvideValue(expression, null);
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

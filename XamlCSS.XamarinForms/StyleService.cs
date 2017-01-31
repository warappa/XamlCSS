using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Xaml.Internals;
using XamlCSS.ComponentModel;

namespace XamlCSS.XamarinForms
{
    public class StyleService : StyleServiceBase<Style, BindableObject, BindableProperty>
    {
        private IDependencyPropertyService<BindableObject, BindableObject, Style, BindableProperty> dependencyService;
        private IMarkupExtensionParser markupExtensionParser;
        private CssTypeHelper<BindableObject, BindableObject, BindableProperty, Style> typeNameResolver;
        private XamarinTypeConverterProvider typeConverterProvider;

        public StyleService(IDependencyPropertyService<BindableObject, BindableObject, Style, BindableProperty> dependencyService,
            IMarkupExtensionParser markupExtensionParser)
        {
            this.dependencyService = dependencyService;
            this.markupExtensionParser = markupExtensionParser;
            this.typeNameResolver = new CssTypeHelper<BindableObject, BindableObject, BindableProperty, Style>(markupExtensionParser, dependencyService);
            this.typeConverterProvider = new XamarinTypeConverterProvider();
        }
        protected override void AddSetter(Style style, BindableProperty property, object value)
        {
            style.Setters.Add(new Setter { Property = property, Value = value });
        }

        protected override void AddTrigger(Style style, BindableObject trigger)
        {
            style.Triggers.Add((TriggerBase)trigger);
        }

        public override IEnumerable<BindableObject> GetTriggersAsList(Style style)
        {
            return style.Triggers;
        }

        public override BindableObject CreateTrigger(StyleSheet styleSheet, ITrigger trigger, Type targetType, BindableObject styleResourceReferenceHolder)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            if (trigger is Trigger)
            {
                var propertyTrigger = trigger as Trigger;
                var nativeTrigger = new Xamarin.Forms.Trigger(targetType);

                var bindableProperty = dependencyService.GetBindableProperty(targetType, propertyTrigger.Property);
                if (bindableProperty == null)
                {
                    throw new NullReferenceException($"Property '{propertyTrigger.Property}' may not be null (targetType '{targetType.Name}')!");
                }

                nativeTrigger.Property = bindableProperty;
                nativeTrigger.Value = dependencyService.GetBindablePropertyValue(targetType, nativeTrigger.Property, propertyTrigger.Value);

                foreach (var styleDeclaration in propertyTrigger.StyleDeclaraionBlock)
                {
                    var property = typeNameResolver.GetDependencyProperty(styleSheet.Namespaces, targetType, styleDeclaration.Property);
                    if (property == null)
                    {
                        continue;
                    }
                    var value = typeNameResolver.GetPropertyValue(targetType, styleResourceReferenceHolder, styleDeclaration.Value, property);

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

                nativeTrigger.Value = GetBasicValue(dataTrigger);

                foreach (var styleDeclaration in dataTrigger.StyleDeclarationBlock)
                {
                    var property = typeNameResolver.GetDependencyProperty(styleSheet.Namespaces, targetType, styleDeclaration.Property);
                    if (property == null)
                    {
                        continue;
                    }
                    var value = typeNameResolver.GetPropertyValue(targetType, styleResourceReferenceHolder, styleDeclaration.Value, property);

                    nativeTrigger.Setters.Add(new Setter { Property = property, Value = value });
                }

                return nativeTrigger;
            }
            else if (trigger is EventTrigger)
            {
                var eventTrigger = trigger as EventTrigger;
                var nativeTrigger = new Xamarin.Forms.EventTrigger();

                nativeTrigger.Event = eventTrigger.Event;

                foreach (var action in eventTrigger.Actions)
                {
                    var actionTypeName = typeNameResolver.ResolveFullTypeName(styleSheet.Namespaces, action.Action);
                    var actionType = Type.GetType(actionTypeName);
                    var triggerAction = (Xamarin.Forms.TriggerAction)Activator.CreateInstance(actionType);

                    var parameters = action.Parameters.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList();

                    foreach (var parameter in parameters)
                    {
                        var parameterName = parameter.Split(' ')[0];

                        object val = null;
                        var parameterValueExpression = parameter.Substring(parameterName.Length + 1).Trim();
                        BindableProperty depProp;
                        var type = typeNameResolver.GetClrPropertyType(styleSheet.Namespaces, triggerAction, parameterName);

                        if (typeNameResolver.IsMarkupExtension(parameterValueExpression))
                        {
                            val = typeNameResolver.GetMarkupExtensionValue(styleResourceReferenceHolder, parameterValueExpression);
                        }
                        else if ((depProp = typeNameResolver.GetDependencyProperty(styleSheet.Namespaces, actionType, parameterName)) != null)
                        {
                            val = typeNameResolver.GetPropertyValue(actionType, styleResourceReferenceHolder, parameterValueExpression, depProp);
                        }

                        if (val is string valueString)
                        {
                            val = typeConverterProvider.GetConverter(type)?.ConvertFromInvariantString(valueString) ?? val;
                        }

                        triggerAction.GetType().GetRuntimeProperty(parameterName).SetValue(triggerAction, val);
                    }

                    nativeTrigger.Actions.Add(triggerAction);
                }

                return nativeTrigger;
            }

            throw new NotSupportedException($"Trigger '{trigger.GetType().FullName}' is not supported!");
        }

        private static object GetBasicValue(DataTrigger dataTrigger)
        {
            object valueExpression = dataTrigger.Value;

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

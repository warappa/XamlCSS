using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace XamlCSS.WPF
{
    public class StyleService : StyleServiceBase<Style, DependencyObject, DependencyProperty>
    {
        private IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyService;
        private IMarkupExtensionParser markupExtensionParser;
        private CssTypeHelper<DependencyObject, DependencyObject, DependencyProperty, Style> typeNameResolver;

        public StyleService(IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyService,
            IMarkupExtensionParser markupExtensionParser)
        {
            this.dependencyService = dependencyService;
            this.markupExtensionParser = markupExtensionParser;
            this.typeNameResolver = new CssTypeHelper<DependencyObject, DependencyObject, DependencyProperty, Style>(markupExtensionParser, dependencyService);
        }

        protected override void AddTrigger(Style style, DependencyObject trigger)
        {
            style.Triggers.Add((System.Windows.TriggerBase)trigger);
        }

        public override IEnumerable<DependencyObject> GetTriggersAsList(Style style)
        {
            return style.Triggers.Select(x => (DependencyObject)XamlReader.Parse(XamlWriter.Save(x)));
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

        public override DependencyObject CreateTrigger(StyleSheet styleSheet, ITrigger trigger, Type targetType, DependencyObject styleResourceReferenceHolder)
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

                foreach (var styleDeclaration in propertyTrigger.StyleDeclaraionBlock)
                {
                    var property = typeNameResolver.GetDependencyProperty(styleSheet.Namespaces, targetType, styleDeclaration.Property);
                    var value = typeNameResolver.GetPropertyValue(targetType, null, styleDeclaration.Value, property);

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

                foreach (var styleDeclaration in dataTrigger.StyleDeclarationBlock)
                {
                    var property = typeNameResolver.GetDependencyProperty(styleSheet.Namespaces, targetType, styleDeclaration.Property);
                    var value = typeNameResolver.GetPropertyValue(targetType, null, styleDeclaration.Value, property);

                    nativeTrigger.Setters.Add(new Setter { Property = property, Value = value });
                }

                return nativeTrigger;
            }
            else if (trigger is EventTrigger)
            {
                var eventTrigger = trigger as EventTrigger;
                var nativeTrigger = new System.Windows.EventTrigger();

                var fieldInfo = targetType.GetField(eventTrigger.Event + "Event", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);

                nativeTrigger.RoutedEvent = (RoutedEvent)fieldInfo.GetValue(null);

                foreach (var action in eventTrigger.Actions)
                {
                    var actionTypeName = typeNameResolver.ResolveFullTypeName(styleSheet.Namespaces, action.Action);
                    var actionType = Type.GetType(actionTypeName);
                    var triggerAction = (System.Windows.TriggerAction)Activator.CreateInstance(actionType);

                    var parameters = action.Parameters.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var parameter in parameters)
                    {
                        var parameterName = parameter.Split(' ')[0];
                        var depProp = typeNameResolver.GetDependencyProperty(styleSheet.Namespaces, actionType, parameterName);
                        var val = typeNameResolver.GetPropertyValue(actionType, styleResourceReferenceHolder, parameter.Substring(parameterName.Length + 1), depProp);

                        if (val is DynamicResourceExtension)
                        {
                            var dyn = val as DynamicResourceExtension;
                            val = dyn.ProvideValue((IServiceProvider)typeof(System.Windows.Application).GetProperty("ServiceProvider").GetValue(Application.Current));

                        }
                        else if (val is StaticResourceExtension)
                        {
                            var dyn = val as StaticResourceExtension;
                            val = dyn.ProvideValue((IServiceProvider)typeof(System.Windows.Application).GetProperty("ServiceProvider").GetValue(Application.Current));

                        }
                        triggerAction.SetValue(depProp, val);
                    }

                    nativeTrigger.Actions.Add(triggerAction);
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

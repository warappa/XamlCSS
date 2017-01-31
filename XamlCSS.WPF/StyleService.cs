using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
            style.Triggers.Add((TriggerBase)trigger);
        }

        public override IEnumerable<DependencyObject> GetTriggersAsList(Style style)
        {
            return style.Triggers.Select(x => (DependencyObject)XamlReader.Parse(XamlWriter.Save(x)));
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

        public override DependencyObject CreateTrigger(StyleSheet styleSheet, ITrigger trigger, Type targetType, DependencyObject styleResourceReferenceHolder)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            if (trigger is Trigger)
            {
                var propertyTrigger = trigger as Trigger;
                var nativeTrigger = new System.Windows.Trigger();

                var dependencyProperty = dependencyService.GetBindableProperty(targetType, propertyTrigger.Property);
                if (dependencyProperty == null)
                {
                    throw new NullReferenceException($"Property '{propertyTrigger.Property}' may not be null (targetType '{targetType.Name}')!");
                }

                nativeTrigger.Property = dependencyProperty;
                nativeTrigger.Value = dependencyService.GetBindablePropertyValue(targetType, nativeTrigger.Property, propertyTrigger.Value);

                foreach (var styleDeclaration in propertyTrigger.StyleDeclaraionBlock)
                {
                    var property = typeNameResolver.GetDependencyProperty(styleSheet.Namespaces, targetType, styleDeclaration.Property);
                    if (property == null)
                    {
                        continue;
                    }
                    var value = typeNameResolver.GetPropertyValue(targetType, styleResourceReferenceHolder, styleDeclaration.Value, property);

                    if (value is string valueString)
                    {
                        value = TypeDescriptor.GetConverter(property.PropertyType)?.ConvertFromInvariantString(valueString) ?? value;
                    }

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

                nativeTrigger.Value = GetBasicValue(dataTrigger);

                foreach (var styleDeclaration in dataTrigger.StyleDeclarationBlock)
                {
                    var property = typeNameResolver.GetDependencyProperty(styleSheet.Namespaces, targetType, styleDeclaration.Property);
                    if (property == null)
                    {
                        continue;
                    }

                    var value = typeNameResolver.GetPropertyValue(targetType, styleResourceReferenceHolder, styleDeclaration.Value, property);

                    if (value is string valueString)
                    {
                        value = TypeDescriptor.GetConverter(property.PropertyType)?.ConvertFromInvariantString(valueString) ?? value;
                    }

                    nativeTrigger.Setters.Add(new Setter { Property = property, Value = value });
                }

                return nativeTrigger;
            }
            else if (trigger is EventTrigger)
            {
                var eventTrigger = trigger as EventTrigger;
                var nativeTrigger = new System.Windows.EventTrigger();

                var fieldInfo = targetType.GetField(eventTrigger.Event + "Event", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                nativeTrigger.RoutedEvent = (RoutedEvent)fieldInfo.GetValue(null);

                foreach (var action in eventTrigger.Actions)
                {
                    var actionTypeName = typeNameResolver.ResolveFullTypeName(styleSheet.Namespaces, action.Action);
                    var actionType = Type.GetType(actionTypeName);
                    var triggerAction = (System.Windows.TriggerAction)Activator.CreateInstance(actionType);

                    var parameters = action.Parameters.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList();

                    foreach (var parameter in parameters)
                    {
                        var parameterName = parameter.Split(' ')[0];
                        object value = null;
                        var parameterValueExpression = parameter.Substring(parameterName.Length + 1).Trim();
                        DependencyProperty depProp;
                        var type = typeNameResolver.GetClrPropertyType(styleSheet.Namespaces, triggerAction, parameterName);

                        if (typeNameResolver.IsMarkupExtension(parameterValueExpression))
                        {
                            value = typeNameResolver.GetMarkupExtensionValue(styleResourceReferenceHolder, parameterValueExpression);
                        }
                        else if ((depProp = typeNameResolver.GetDependencyProperty(styleSheet.Namespaces, actionType, parameterName)) != null)
                        {
                            value = typeNameResolver.GetPropertyValue(actionType, styleResourceReferenceHolder, parameterValueExpression, depProp);

                            if (value is DynamicResourceExtension)
                            {
                                var dyn = value as DynamicResourceExtension;
                                var serviceProvider = (IServiceProvider)typeof(Application).GetProperty("ServiceProvider", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Application.Current);
                                value = dyn.ProvideValue(serviceProvider);
                            }
                            else if (value is StaticResourceExtension)
                            {
                                var dyn = value as StaticResourceExtension;
                                var serviceProvider = (IServiceProvider)typeof(Application).GetProperty("ServiceProvider", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Application.Current);
                                value = dyn.ProvideValue(serviceProvider);
                            }
                        }
                        else
                        {
                            value = parameterValueExpression;
                        }

                        if (value is string valueString)
                        {
                            value = TypeDescriptor.GetConverter(type)?.ConvertFromInvariantString(valueString) ?? value;
                        }

                        triggerAction.GetType().GetRuntimeProperty(parameterName).SetValue(triggerAction, value);
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

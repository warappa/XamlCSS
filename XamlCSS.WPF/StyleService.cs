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

                foreach (var styleDeclaration in propertyTrigger.StyleDeclarationBlock)
                {
                    var property = typeNameResolver.GetDependencyProperty(styleSheet.Namespaces, targetType, styleDeclaration.Property);
                    if (property == null)
                    {
                        continue;
                    }
                    try
                    {
                        var value = typeNameResolver.GetPropertyValue(targetType, styleResourceReferenceHolder, styleDeclaration.Value, property);

                        if (value is string valueString)
                        {
                            value = TypeDescriptor.GetConverter(property.PropertyType)?.ConvertFromInvariantString(valueString) ?? value;
                        }

                        nativeTrigger.Setters.Add(new Setter { Property = property, Value = value });
                    }
                    catch (Exception e)
                    {
                        styleSheet.Errors.Add($@"property trigger ""{propertyTrigger.Property} {propertyTrigger.Value} - {styleDeclaration.Property}: {styleDeclaration.Value}"": {e.Message}");
                    }
                }

                foreach (var action in propertyTrigger.EnterActions)
                {
                    try
                    {
                        var nativeTriggerAction = CreateTriggerAction(styleSheet, styleResourceReferenceHolder, action);

                        nativeTrigger.EnterActions.Add(nativeTriggerAction);
                    }
                    catch (Exception e)
                    {
                        styleSheet.Errors.Add($@"ERROR in property trigger ""{propertyTrigger.Property} {propertyTrigger.Value}"" enter action: {e.Message}");
                    }
                }

                foreach (var action in propertyTrigger.ExitActions)
                {
                    try
                    {
                        var nativeTriggerAction = CreateTriggerAction(styleSheet, styleResourceReferenceHolder, action);

                        nativeTrigger.ExitActions.Add(nativeTriggerAction);
                    }
                    catch (Exception e)
                    {
                        styleSheet.Errors.Add($@"ERROR in property trigger ""{propertyTrigger.Property} {propertyTrigger.Value}"" exit action: {e.Message}");
                    }
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
                    try
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
                    catch (Exception e)
                    {
                        styleSheet.Errors.Add($@"ERROR in property trigger ""{dataTrigger.Binding} {dataTrigger.Value} - {styleDeclaration.Property}: {styleDeclaration.Value}"": {e.Message}");
                    }
                }

                foreach (var action in dataTrigger.EnterActions)
                {
                    try
                    {
                        var nativeTriggerAction = CreateTriggerAction(styleSheet, styleResourceReferenceHolder, action);

                        nativeTrigger.EnterActions.Add(nativeTriggerAction);
                    }
                    catch (Exception e)
                    {
                        styleSheet.Errors.Add($@"ERROR in data trigger ""{dataTrigger.Binding} {dataTrigger.Value} - {action}"" enter action: {e.Message}");
                    }
                }

                foreach (var action in dataTrigger.ExitActions)
                {
                    try
                    {
                        var nativeTriggerAction = CreateTriggerAction(styleSheet, styleResourceReferenceHolder, action);

                        nativeTrigger.ExitActions.Add(nativeTriggerAction);
                    }
                    catch (Exception e)
                    {
                        styleSheet.Errors.Add($@"ERROR in data trigger ""{dataTrigger.Binding} {dataTrigger.Value} - {action}"" exit action: {e.Message}");
                    }
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
                    try
                    {
                        var nativeTriggerAction = CreateTriggerAction(styleSheet, styleResourceReferenceHolder, action);

                        nativeTrigger.Actions.Add(nativeTriggerAction);
                    }
                    catch (Exception e)
                    {
                        styleSheet.Errors.Add($@"ERROR in event trigger ""{eventTrigger.Event} {action.Action}"": {e.Message}");
                    }
                }

                return nativeTrigger;
            }

            throw new NotSupportedException($"Trigger '{trigger.GetType().FullName}' is not supported!");
        }

        private System.Windows.TriggerAction CreateTriggerAction(StyleSheet styleSheet, DependencyObject styleResourceReferenceHolder, TriggerAction action)
        {
            var actionTypeName = typeNameResolver.ResolveFullTypeName(styleSheet.Namespaces, action.Action);
            var actionType = Type.GetType(actionTypeName);
            var nativeTriggerAction = (System.Windows.TriggerAction)Activator.CreateInstance(actionType);

            foreach (var parameter in action.Parameters)
            {
                var parameterName = parameter.Property;
                object value = null;
                var parameterValueExpression = parameter.Value.Trim();
                DependencyProperty depProp;
                var type = typeNameResolver.GetClrPropertyType(styleSheet.Namespaces, nativeTriggerAction, parameterName);

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

                nativeTriggerAction.GetType().GetRuntimeProperty(parameterName).SetValue(nativeTriggerAction, value);
            }

            return nativeTriggerAction;
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Xml;
using XamlCSS.Utils;

namespace XamlCSS.WPF
{
    public class StyleService : StyleServiceBase<Style, DependencyObject, DependencyProperty>
    {
        private IDependencyPropertyService<DependencyObject, Style, DependencyProperty> dependencyService;
        private IMarkupExtensionParser markupExtensionParser;
        private CssTypeHelper<DependencyObject, DependencyProperty, Style> typeNameResolver;

        public StyleService(IDependencyPropertyService<DependencyObject, Style, DependencyProperty> dependencyService,
            IMarkupExtensionParser markupExtensionParser)
        {
            this.dependencyService = dependencyService;
            this.markupExtensionParser = markupExtensionParser;
            this.typeNameResolver = new CssTypeHelper<DependencyObject, DependencyProperty, Style>(markupExtensionParser, dependencyService);
        }

        protected override void AddTrigger(Style style, DependencyObject trigger)
        {
            style.Triggers.Add((TriggerBase)trigger);
        }

        private T Clone<T>(T obj)
        {
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, new XmlWriterSettings
            {
                Indent = false,
                ConformanceLevel = ConformanceLevel.Fragment,
                OmitXmlDeclaration = true,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
            }))
            {
                var mgr = new XamlDesignerSerializationManager(writer);
                mgr.XamlWriterMode = XamlWriterMode.Expression;

                XamlWriter.Save(obj, mgr);
                using (var stringReader = new StringReader(sb.ToString()))
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    return (T)XamlReader.Load(xmlReader);
                }
            }
        }

        public override IEnumerable<DependencyObject> GetTriggersAsList(Style style)
        {
            return style.Triggers.ToList();//.Select(x => (DependencyObject)XamlReader.Parse(XamlWriter.Save(x))).ToList();
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
                return "PropertyTrigger".Measure(() =>
                {
                    var propertyTrigger = trigger as Trigger;
                    var nativeTrigger = new System.Windows.Trigger();

                    var dependencyProperty = dependencyService.GetDependencyProperty(targetType, propertyTrigger.Property);
                    if (dependencyProperty == null)
                    {
                        throw new NullReferenceException($"Property '{propertyTrigger.Property}' may not be null (targetType '{targetType.Name}')!");
                    }

                    nativeTrigger.Property = dependencyProperty;
                    nativeTrigger.Value = dependencyService.GetDependencyPropertyValue(targetType, nativeTrigger.Property.Name, nativeTrigger.Property, propertyTrigger.Value);
                    "StyleDeclarationBlock".Measure(() =>
                    {
                        foreach (var styleDeclaration in propertyTrigger.StyleDeclarationBlock)
                        {
                            var propertyInfo = typeNameResolver.GetDependencyPropertyInfo(styleSheet.Namespaces, targetType, styleDeclaration.Property);
                            if (propertyInfo == null)
                            {
                                continue;
                            }
                            try
                            {
                                var value = typeNameResolver.GetPropertyValue(propertyInfo.DeclaringType, styleResourceReferenceHolder, propertyInfo.Name, styleDeclaration.Value, propertyInfo.Property, styleSheet.Namespaces);
                                if (value == null)
                                {

                                }
                                nativeTrigger.Setters.Add(new Setter { Property = propertyInfo.Property, Value = value });
                            }
                            catch (Exception e)
                            {
                                styleSheet.Errors.Add($@"ERROR in property trigger ""{propertyTrigger.Property} {propertyTrigger.Value} - {styleDeclaration.Property}: {styleDeclaration.Value}"": {e.Message}");
                            }
                        }
                    });

                    $"EnterActions ({propertyTrigger.EnterActions.Count})".Measure(() =>
                    {
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
                    });
                    $"ExitActions ({propertyTrigger.ExitActions.Count})".Measure(() =>
                    {
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
                    });

                    return nativeTrigger;
                });
            }
            else if (trigger is DataTrigger)
            {
                return "DataTrigger".Measure(() =>
                {
                    var dataTrigger = trigger as DataTrigger;
                    var nativeTrigger = new System.Windows.DataTrigger();

                    string expression = null;
                    if (typeNameResolver.IsMarkupExtension(dataTrigger.Binding))
                    {
                        expression = typeNameResolver.CreateMarkupExtensionExpression(dataTrigger.Binding);
                    }
                    else
                    {
                        expression = "{Binding " + dataTrigger.Binding + "}";
                    }

                    var binding = (System.Windows.Data.BindingBase)markupExtensionParser.ProvideValue(expression, null, styleSheet.Namespaces);
                    nativeTrigger.Binding = binding;

                    nativeTrigger.Value = GetBasicValue(dataTrigger);

                    foreach (var styleDeclaration in dataTrigger.StyleDeclarationBlock)
                    {
                        try
                        {
                            var propertyInfo = typeNameResolver.GetDependencyPropertyInfo(styleSheet.Namespaces, targetType, styleDeclaration.Property);
                            if (propertyInfo == null)
                            {
                                continue;
                            }

                            var value = typeNameResolver.GetPropertyValue(propertyInfo.DeclaringType, styleResourceReferenceHolder, propertyInfo.Name, styleDeclaration.Value, propertyInfo.Property, styleSheet.Namespaces);
                            if (value == null)
                            {

                            }
                            nativeTrigger.Setters.Add(new Setter { Property = propertyInfo.Property, Value = value });
                        }
                        catch (Exception e)
                        {
                            styleSheet.Errors.Add($@"ERROR in data trigger ""{dataTrigger.Binding} {dataTrigger.Value} - {styleDeclaration.Property}: {styleDeclaration.Value}"": {e.Message}");
                        }
                    }

                    foreach (var action in dataTrigger.EnterActions)
                    {
                        try
                        {
                            var info = $"{action.Action} {string.Join(", ", action.Parameters.Select(x => x.Property + ": " + x.Value))}";
                            $"measure {info}".Measure(() =>
                            {
                                $"CreateTriggerAction outer {info}".Measure(() =>
                                {
                                    System.Windows.TriggerAction nativeTriggerAction = null;

                                    $"CreateTriggerAction {info}".Measure(() => nativeTriggerAction = CreateTriggerAction(styleSheet, styleResourceReferenceHolder, action));

                                    $"Add to EnterActions {info}".Measure(() => nativeTrigger.EnterActions.Add(nativeTriggerAction));
                                });
                            });
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
                });
            }
            else if (trigger is EventTrigger)
            {
                return "EventTrigger".Measure(() =>
                {
                    var eventTrigger = trigger as EventTrigger;
                    var nativeTrigger = new System.Windows.EventTrigger();

                    nativeTrigger.RoutedEvent = (RoutedEvent)TypeHelpers.GetFieldValue(targetType, eventTrigger.Event + "Event");
                    "Actions".Measure(() =>
                    {
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
                    });

                    return nativeTrigger;
                });
            }

            throw new NotSupportedException($"Trigger '{trigger.GetType().FullName}' is not supported!");
        }
        private IServiceProvider serviceProvider;

        private System.Windows.TriggerAction CreateTriggerAction(StyleSheet styleSheet, DependencyObject styleResourceReferenceHolder, TriggerAction action)
        {
            string actionTypeName = null;
            Type actionType = null;
            System.Windows.TriggerAction nativeTriggerAction = null;

            "get types etc".Measure(() =>
            {
                actionTypeName = typeNameResolver.ResolveFullTypeName(styleSheet.Namespaces, action.Action);
                actionType = Type.GetType(actionTypeName);
                nativeTriggerAction = (System.Windows.TriggerAction)Activator.CreateInstance(actionType);
            });

            "foreach (var parameter in action.Parameters)".Measure(() =>
            {
                foreach (var parameter in action.Parameters)
                {
                    string parameterName = null;
                    object value = null;
                    string parameterValueExpression = null;
                    DependencyPropertyInfo<DependencyProperty> propertyInfo;
                    Type type = null;
                    "Parameter init".Measure(() =>
                    {
                        parameterName = parameter.Property;
                        parameterValueExpression = parameter.Value.Trim();
                        type = typeNameResolver.GetClrPropertyType(styleSheet.Namespaces, nativeTriggerAction, parameterName);
                    });
                    "Value conversions".Measure(() =>
                    {
                        if (typeNameResolver.IsMarkupExtension(parameterValueExpression))
                        {
                            $"GetMarkupExtensionValue {parameterValueExpression}".Measure(() => value = typeNameResolver.GetMarkupExtensionValue(styleResourceReferenceHolder, parameterValueExpression, styleSheet.Namespaces));
                        }
                        else if ((propertyInfo = typeNameResolver.GetDependencyPropertyInfo(styleSheet.Namespaces, actionType, parameterName)) != null)
                        {
                            "GetPropertyValue".Measure(() =>
                            {
                                value = typeNameResolver.GetPropertyValue(propertyInfo.DeclaringType, styleResourceReferenceHolder, propertyInfo.Name, parameterValueExpression, propertyInfo.Property, styleSheet.Namespaces);

                                if (value is DynamicResourceExtension)
                                {
                                    var dyn = value as DynamicResourceExtension;
                                    serviceProvider = serviceProvider ?? (serviceProvider = (IServiceProvider)typeof(Application).GetProperty("ServiceProvider", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Application.Current));
                                    value = dyn.ProvideValue(serviceProvider);
                                }
                                else if (value is StaticResourceExtension)
                                {
                                    var dyn = value as StaticResourceExtension;
                                    serviceProvider = serviceProvider ?? (serviceProvider = (IServiceProvider)typeof(Application).GetProperty("ServiceProvider", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Application.Current));
                                    value = dyn.ProvideValue(serviceProvider);
                                }
                            });
                        }
                        else
                        {
                            value = parameterValueExpression;
                        }

                        if (value is string valueString)
                        {
                            value = "GetConverter".Measure(() => TypeDescriptor.GetConverter(type)?.ConvertFromInvariantString(valueString) ?? value);
                        }

                        if (value == null)
                        {

                        }
                    });

                    $"Set Property Value {parameterName} = {value}".Measure(() => TypeHelpers.SetPropertyValue(nativeTriggerAction, parameterName, value));
                }
            });
            return nativeTriggerAction;

        }

        protected override void AddSetter(Style style, DependencyProperty property, object value)
        {
            if (value == null)
            {

            }
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

        public override Style GetStyle(DependencyObject visualElement)
        {
            if (visualElement is FrameworkElement)
            {
                return (visualElement as FrameworkElement).Style;
            }
            else if (visualElement is FrameworkContentElement)
            {
                return (visualElement as FrameworkContentElement).Style;
            }

            return null;
        }
    }
}

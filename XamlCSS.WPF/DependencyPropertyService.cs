using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using XamlCSS.Dom;
using XamlCSS.Utils;

namespace XamlCSS.WPF
{
    public class DependencyPropertyService : IDependencyPropertyService<DependencyObject, Style, DependencyProperty>
    {
        readonly ITypeDescriptorContext context = new TypeDescriptorContext(new Uri("pack://application:,,,/", UriKind.Absolute));

        public DependencyProperty GetDependencyProperty(DependencyObject frameworkElement, string propertyName)
        {
            return GetDependencyProperty(frameworkElement.GetType(), propertyName);
        }
        public DependencyProperty GetDependencyProperty(Type bindableObjectType, string propertyName)
        {
            DependencyProperty result;

            result = TypeHelpers.GetDependencyPropertyInfo<DependencyProperty>(bindableObjectType, propertyName)?.Property;

            return result;
        }

        public object GetDependencyPropertyValue(Type frameworkElementType, string propertyName, DependencyProperty property, object propertyValue)
        {
            return $"GetDependencyPropertyValue {propertyName} {propertyValue}".Measure(() =>
            {
                if (property != null &&
                    !(property.PropertyType.IsInstanceOfType(propertyValue)))
                {
                    var propertyType = property.PropertyType;

                    var converter = TypeDescriptor.GetConverter(propertyType);

                    if (converter != null)
                    {
                        if ((property.PropertyType == typeof(float) ||
                            property.PropertyType == typeof(double)) &&
                            (propertyValue as string)?.StartsWith(".", StringComparison.Ordinal) == true)
                        {
                            var stringValue = propertyValue as string;
                            propertyValue = "0" + (stringValue.Length > 1 ? stringValue : "");
                        }

                        propertyValue = converter.ConvertFrom(context, CultureInfo.InvariantCulture, propertyValue as string);

                    }
                    else if (propertyType == typeof(bool))
                    {
                        propertyValue = propertyValue.Equals("true");
                    }
                    else if (propertyType.IsEnum)
                    {
                        propertyValue = Enum.Parse(propertyType, propertyValue as string);
                    }
                    else
                    {
                        propertyValue = Convert.ChangeType(propertyValue, propertyType);
                    }
                }

                return propertyValue;
            });
        }

        protected object ReadSafe(DependencyObject obj, DependencyProperty property)
        {
            var val = obj.ReadLocalValue(property);
            if (val == DependencyProperty.UnsetValue)
            {
                return null;
            }

            if (val?.GetType().Name == "ResourceReferenceExpression")
            {
                var resourceKey = TypeHelpers.GetPropertyValue(val, "ResourceKey");

                val = MarkupExtensionParser.GetDynamicResourceValue(resourceKey, obj);
            }

            return val;
        }

        public string GetClass(DependencyObject obj)
        {
            return (string)ReadSafe(obj, Css.ClassProperty);
        }

        public Style GetInitialStyle(DependencyObject obj)
        {
            return (Style)ReadSafe(obj, Css.InitialStyleProperty);
        }

        public string GetName(DependencyObject obj)
        {
            return (obj as FrameworkElement)?.Name;
        }

        public StyleDeclarationBlock GetStyle(DependencyObject obj)
        {
            return (StyleDeclarationBlock)ReadSafe(obj, Css.StyleProperty);
        }

        public StyleSheet GetStyleSheet(DependencyObject obj)
        {
            return (StyleSheet)ReadSafe(obj, Css.StyleSheetProperty);
        }
        
        public IDomElement<DependencyObject> GetDomElement(DependencyObject obj)
        {
            return ReadSafe(obj, Css.DomElementProperty) as IDomElement<DependencyObject>;
        }
        
        public void SetClass(DependencyObject obj, string value)
        {
            obj.SetValue(Css.ClassProperty, value);
        }
        
        public void SetInitialStyle(DependencyObject obj, Style value)
        {
            obj.SetValue(Css.InitialStyleProperty, value);
        }
        
        public void SetName(DependencyObject obj, string value)
        {
            (obj as FrameworkElement).Name = value;
        }

        public void SetStyle(DependencyObject obj, StyleDeclarationBlock value)
        {
            obj.SetValue(Css.StyleProperty, value);
        }

        public void SetStyleSheet(DependencyObject obj, StyleSheet value)
        {
            obj.SetValue(Css.StyleSheetProperty, value);
        }
        
        public void SetDomElement(DependencyObject obj, IDomElement<DependencyObject> value)
        {
            Css.SetDomElement(obj, value);
        }

        public bool IsLoaded(DependencyObject obj)
        {
            if (obj is FrameworkElement)
            {
                return (obj as FrameworkElement).IsLoaded;
            }

            if (obj is FrameworkContentElement)
            {
                return (obj as FrameworkContentElement).IsLoaded;
            }

            return DesignerProperties.GetIsInDesignMode(obj);
        }

        public void RegisterLoadedOnce(DependencyObject element, Action<object> func)
        {
            var frameworkElement = element as FrameworkElement;
            if (frameworkElement != null)
            {
                RoutedEventHandler handler = null;
                handler = (s, e) =>
                {
                    frameworkElement.Loaded -= handler;
                    func(s);
                };

                frameworkElement.Loaded += handler;
            }
            else
            {
                var frameworkContentElement = element as FrameworkContentElement;
                if (frameworkContentElement != null)
                {
                    RoutedEventHandler handler = null;
                    handler = (s, e) =>
                    {
                        frameworkContentElement.Loaded -= handler;
                        func(s);
                    };

                    frameworkContentElement.Loaded += handler;
                }
            }
        }
        
        public object GetValue(DependencyObject obj, string propertyName)
        {
            if (obj == null)
            {
                return null;
            }

            var dp = TypeHelpers.GetDependencyPropertyInfo<DependencyProperty>(obj.GetType(), propertyName);
            return obj.GetValue(dp.Property);
        }

        public void SetValue(DependencyObject obj, string propertyName, object value)
        {
            if (obj == null)
            {
                return;
            }

            var dp = TypeHelpers.GetDependencyPropertyInfo<DependencyProperty>(obj.GetType(), propertyName);
            obj.SetValue(dp.Property, value);
        }
    }
}

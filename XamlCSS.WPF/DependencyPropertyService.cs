using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using XamlCSS.Dom;

namespace XamlCSS.WPF
{
    public class DependencyPropertyService : IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty>
    {
        readonly ITypeDescriptorContext context = new TypeDescriptorContext(new Uri("pack://application:,,,/", UriKind.Absolute));

        public DependencyProperty GetBindableProperty(DependencyObject frameworkElement, string propertyName)
        {
            return GetBindableProperty(frameworkElement.GetType(), propertyName);
        }
        public DependencyProperty GetBindableProperty(Type bindableObjectType, string propertyName)
        {
            string dpName = $"{propertyName}Property";
            var dpFields = TypeHelpers.DeclaredFields(bindableObjectType);
            var dpField = dpFields.FirstOrDefault(i => i.Name == dpName);

            if (dpField != null)
                return dpField.GetValue(null) as DependencyProperty;
            return null;
        }

        public object GetBindablePropertyValue(Type frameworkElementType, DependencyProperty property, object propertyValue)
        {
            if (property != null &&
                !(property.PropertyType.IsInstanceOfType(propertyValue)))
            {
                var propertyType = property.PropertyType;

                var converter = TypeDescriptor.GetConverter(propertyType);

                if (converter == null)
                {
                    converter = TypeDescriptor.GetConverter(propertyType);
                }

                if (converter != null)
                {
                    propertyValue = converter.ConvertFrom(context, CultureInfo.CurrentUICulture, propertyValue as string);
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
                var resourceKeyProperty = val.GetType().GetProperty("ResourceKey");

                val = GetDynamicResourceValue(resourceKeyProperty.GetValue(val), obj);
            }

            return val;
        }

        private static object GetDynamicResourceValue(object resourceKey, object element)
        {
            object val = null;
            while (element != null)
            {
                if (element is FrameworkElement)
                {
                    if (((FrameworkElement)element).Resources?.Contains(resourceKey) == true)
                    {
                        val = ((FrameworkElement)element).Resources[resourceKey];
                        break;
                    }

                    element = ((FrameworkElement)element).Parent;
                }
                else if (element is FrameworkContentElement)
                {
                    if (((FrameworkContentElement)element).Resources?.Contains(resourceKey) == true)
                    {
                        val = ((FrameworkContentElement)element).Resources[resourceKey];
                        break;
                    }
                    element = ((FrameworkContentElement)element).Parent;
                }
                else
                {
                    break;
                }
            }

            return val;
        }

        public string[] GetAppliedMatchingStyles(DependencyObject obj)
        {
            return (string[])ReadSafe(obj, Css.AppliedMatchingStylesProperty);
        }

        public string GetClass(DependencyObject obj)
        {
            return (string)ReadSafe(obj, Css.ClassProperty);
        }

        public bool? GetHadStyle(DependencyObject obj)
        {
            return (bool?)ReadSafe(obj, Css.HadStyleProperty);
        }

        public Style GetInitialStyle(DependencyObject obj)
        {
            return (Style)ReadSafe(obj, Css.InitialStyleProperty);
        }

        public string[] GetMatchingStyles(DependencyObject obj)
        {
            return (string[])ReadSafe(obj, Css.MatchingStylesProperty);
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

        public bool GetHandledCss(DependencyObject obj)
        {
            return ((bool?)ReadSafe(obj, Css.HandledCssProperty) ?? false);
        }

        public IDomElement<DependencyObject> GetDomElement(DependencyObject obj)
        {
            return ReadSafe(obj, Css.DomElementProperty) as IDomElement<DependencyObject>;
        }

        public void SetAppliedMatchingStyles(DependencyObject obj, string[] value)
        {
            obj.SetValue(Css.AppliedMatchingStylesProperty, value);
        }

        public void SetClass(DependencyObject obj, string value)
        {
            obj.SetValue(Css.ClassProperty, value);
        }

        public void SetHadStyle(DependencyObject obj, bool? value)
        {
            obj.SetValue(Css.HadStyleProperty, value);
        }

        public void SetInitialStyle(DependencyObject obj, Style value)
        {
            obj.SetValue(Css.InitialStyleProperty, value);
        }

        public void SetMatchingStyles(DependencyObject obj, string[] value)
        {
            obj.SetValue(Css.MatchingStylesProperty, value);
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

        public void SetHandledCss(DependencyObject obj, bool value)
        {
            obj.SetValue(Css.HandledCssProperty, value);
        }

        public void SetDomElement(DependencyObject obj, IDomElement<DependencyObject> value)
        {
            obj.SetValue(Css.DomElementProperty, value);
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
    }
}

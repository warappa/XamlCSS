using System;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using XamlCSS.ComponentModel;
using XamlCSS.Dom;
using XamlCSS.Utils;

namespace XamlCSS.XamarinForms
{
    public class DependencyPropertyService : IDependencyPropertyService<BindableObject, Style, BindableProperty>
    {
        private ITypeConverterProvider<TypeConverter> typeConverterProvider;

        public DependencyPropertyService()
        {
            this.typeConverterProvider = new XamarinTypeConverterProvider();
        }

        public BindableProperty GetDependencyProperty(BindableObject frameworkElement, string propertyName)
        {
            return GetDependencyProperty(frameworkElement.GetType(), propertyName);
        }
        public BindableProperty GetDependencyProperty(Type bindableObjectType, string propertyName)
        {
            string dpName = $"{propertyName}Property";
            return TypeHelpers.GetFieldValue(bindableObjectType, dpName) as BindableProperty;
        }
        
        public object GetClrValue(Type propertyType, string propertyValueString)
        {
            if (!(propertyType.GetTypeInfo()
                .IsAssignableFrom(propertyValueString.GetType().GetTypeInfo())))
            {
                TypeConverter converter = null;

                converter = typeConverterProvider.GetConverterFromProperty(propertyValueString, propertyType);

                if (converter == null)
                    converter = typeConverterProvider.GetConverter(propertyType);
                if (converter != null)
                    return converter.ConvertFromInvariantString(propertyValueString);

                else if (propertyType == typeof(bool))
                    return propertyValueString.Equals("true");
                else if (propertyType == typeof(Color))
                    return Color.FromHex(propertyValueString as string);
                else if (propertyType == typeof(LayoutOptions))
                    return propertyType.GetRuntimeFields().First(x => x.Name == propertyValueString as string).GetValue(null);
                else if (propertyType.GetTypeInfo().IsEnum)
                    return Enum.Parse(propertyType, propertyValueString as string);
                else
                    return Convert.ChangeType(propertyValueString, propertyType);
            }

            return propertyValueString;
        }

        public object GetDependencyPropertyValue(Type frameworkElementType, string propertyName, BindableProperty bindableProperty, object propertyValue)
        {
            if (!(bindableProperty.ReturnType.GetTypeInfo()
                .IsAssignableFrom(propertyValue.GetType().GetTypeInfo())))
            {
                Type propertyType = bindableProperty.ReturnType;
                TypeConverter converter = null;

                converter = typeConverterProvider.GetConverterFromProperty(bindableProperty.PropertyName, frameworkElementType);

                if (converter == null)
                    converter = typeConverterProvider.GetConverter(propertyType);
                if (converter != null)
                {
                    if ((propertyType == typeof(float) ||
                        propertyType == typeof(double)) &&
                        (propertyValue as string)?.StartsWith(".", StringComparison.Ordinal) == true)
                    {
                        var stringValue = propertyValue as string;
                        propertyValue = "0" + (stringValue.Length > 1 ? stringValue : "");
                    }

                    propertyValue = converter.ConvertFromInvariantString(propertyValue as string);
                }
                else if (propertyType == typeof(bool))
                    propertyValue = propertyValue.Equals("true");
                else if (propertyType == typeof(Color))
                    propertyValue = Color.FromHex(propertyValue as string);
                else if (propertyType == typeof(LayoutOptions))
                    propertyValue = propertyType.GetRuntimeFields().First(x => x.Name == propertyValue as string).GetValue(null);
                else if (propertyType.GetTypeInfo().IsEnum)
                    propertyValue = Enum.Parse(propertyType, propertyValue as string);
                else
                    propertyValue = Convert.ChangeType(propertyValue, propertyType);
            }

            return propertyValue;
        }
        
        public string GetClass(BindableObject obj)
        {
            return Css.GetClass(obj);
        }
        
        public Style GetInitialStyle(BindableObject obj)
        {
            return Css.GetInitialStyle(obj);
        }
        
        public string GetName(BindableObject obj)
        {
            return Css.GetId(obj);
        }

        public StyleDeclarationBlock GetStyle(BindableObject obj)
        {
            return Css.GetStyle(obj);
        }

        public StyleSheet GetStyleSheet(BindableObject obj)
        {
            return Css.GetStyleSheet(obj);
        }
        
        public void SetClass(BindableObject obj, string value)
        {
            Css.SetClass(obj, value);
        }
        
        public void SetInitialStyle(BindableObject obj, Style value)
        {
            Css.SetInitialStyle(obj, value);
        }
        
        public void SetName(BindableObject obj, string value)
        {
            Css.SetId(obj, value);
        }

        public void SetStyle(BindableObject obj, StyleDeclarationBlock value)
        {
            Css.SetStyle(obj, value);
        }

        public void SetStyleSheet(BindableObject obj, StyleSheet value)
        {
            Css.SetStyleSheet(obj, value);
        }
        
        public bool IsLoaded(BindableObject obj)
        {
            return (obj as Element)?.Parent != null;
        }

        public void RegisterLoadedOnce(BindableObject frameworkElement, Action<object> func)
        {
            EventHandler handler = null;
            handler = (s, e) =>
            {
                frameworkElement.BindingContextChanged -= handler;
                func(s);
            };
            frameworkElement.BindingContextChanged += handler;
        }

        public IDomElement<BindableObject, BindableProperty> GetDomElement(BindableObject obj)
        {
            return Css.GetDomElement(obj) as IDomElement<BindableObject, BindableProperty>;
        }

        public void SetDomElement(BindableObject obj, IDomElement<BindableObject, BindableProperty> value)
        {
            Css.SetDomElement(obj, value);
        }

        public object GetValue(BindableObject obj, string propertyName)
        {
            if (obj == null)
            {
                return null;
            }

            var dp = TypeHelpers.GetDependencyPropertyInfo<BindableProperty>(obj.GetType(), propertyName);
            return obj.GetValue(dp.Property);
        }

        public void SetValue(BindableObject obj, string propertyName, object value)
        {
            if (obj == null)
            {
                return;
            }

            var dp = TypeHelpers.GetDependencyPropertyInfo<BindableProperty>(obj.GetType(), propertyName);
            obj.SetValue(dp.Property, value);
        }
    }
}

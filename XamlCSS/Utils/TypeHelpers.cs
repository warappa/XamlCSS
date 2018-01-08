using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XamlCSS.Utils
{
    public static class TypeHelpers
    {
        private static bool dependencyPropertiesAreFields = true;

        public static void Initialze(bool dependencyPropertiesAreFields = true)
        {
            TypeHelpers.dependencyPropertiesAreFields = dependencyPropertiesAreFields;
        }

        private static IDictionary<Type, IDictionary<string, FieldInfo>> fieldInfos = new Dictionary<Type, IDictionary<string, FieldInfo>>();
        private static IDictionary<Type, IDictionary<string, PropertyInfo>> propertyInfos = new Dictionary<Type, IDictionary<string, PropertyInfo>>();
        private static IDictionary<Type, object> dependencyPropertyInfos = new Dictionary<Type, object>();
        private static IDictionary<Type, object> dependencyProperties = new Dictionary<Type, object>();
        private static Dictionary<Type, Dictionary<string, IPropertyAccessor>> cachedPropertyAccessors = new Dictionary<Type, Dictionary<string, IPropertyAccessor>>();

        private static IDictionary<string, FieldInfo> DeclaredFields(Type type)
        {
            IDictionary<string, FieldInfo> result;
            if (!fieldInfos.TryGetValue(type, out result))
            {
                var fields = new List<FieldInfo>();

                /*var typeInfo = type.GetTypeInfo();

                while (typeInfo != null)
                {
                    fields.AddRange(typeInfo.DeclaredFields);

                    typeInfo = typeInfo.BaseType?.GetTypeInfo();
                }

                fields = fields.GroupBy(x => x.Name)
                    .Select(x => x.First())
                    .ToList();
                */
                fieldInfos[type] = result = fields.ToDictionary(x => x.Name, x => x);
            }

            return result;
        }

        private static FieldInfo GetFieldInfo(Type type, string fieldName)
        {
            var declaredFields = DeclaredFields(type);
            if (!declaredFields.TryGetValue(fieldName, out FieldInfo fieldInfo))
            {
                var typeInfo = type.GetTypeInfo();

                while (typeInfo != null)
                {
                    fieldInfo = typeInfo.DeclaredFields.Where(x => x.Name == fieldName)
                        .FirstOrDefault();

                    if (fieldInfo != null)
                    {
                        break;
                    }

                    typeInfo = typeInfo.BaseType?.GetTypeInfo();
                }

                declaredFields[fieldName] = fieldInfo;
            }

            return fieldInfo;
        }

        private static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            var declaredProperties = DeclaredProperties(type);
            if (!declaredProperties.TryGetValue(propertyName, out PropertyInfo propertyInfo))
            {
                var typeInfo = type.GetTypeInfo();

                while (typeInfo != null)
                {
                    propertyInfo = typeInfo.DeclaredProperties.Where(x => x.Name == propertyName)
                        .FirstOrDefault();

                    if (propertyInfo != null)
                    {
                        break;
                    }

                    typeInfo = typeInfo.BaseType?.GetTypeInfo();
                }

                declaredProperties[propertyName] = propertyInfo;
            }

            return propertyInfo;
        }

        public static object GetFieldValue(object obj, string fieldName)
        {
            if (obj == null)
            {
                return null;
            }

            return obj.GetType().GetRuntimeField(fieldName)?.GetValue(obj);
        }

        public static object GetFieldValue(Type type, string fieldName)
        {
            if (type == null)
            {
                return null;
            }

            return type.GetRuntimeField(fieldName)?.GetValue(null);
        }

        private static IDictionary<string, PropertyInfo> DeclaredProperties(Type type)
        {
            IDictionary<string, PropertyInfo> result;
            if (!propertyInfos.TryGetValue(type, out result))
            {
                var properties = new Dictionary<string, PropertyInfo>();
                /*var typeInfo = type.GetTypeInfo();

                while (typeInfo != null)
                {
                    foreach (var property in typeInfo.DeclaredProperties)
                    {
                        properties[property.Name] = property;
                    }

                    typeInfo = typeInfo.BaseType?.GetTypeInfo();
                }*/

                propertyInfos[type] = properties;

                result = properties;
            }

            return result;
        }

        public static IPropertyAccessor GetPropertyAccessor(Type type, string propertyName)
        {
            return "GetPropertyAccessor".Measure(() =>
            {
                if (!cachedPropertyAccessors.TryGetValue(type, out Dictionary<string, IPropertyAccessor> propertyAccessors))
                {
                    cachedPropertyAccessors[type] = propertyAccessors = new Dictionary<string, IPropertyAccessor>();
                }

                if (!propertyAccessors.TryGetValue(propertyName, out IPropertyAccessor accessor))
                {
                    var propertyInfo = type.GetRuntimeProperty(propertyName);

                    propertyAccessors[propertyName] = accessor = propertyInfo == null ? null : PropertyInfoHelper.GetAccessor(propertyInfo);
                }

                return accessor;
            });
        }

        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null)
            {
                return null;
            }

            return GetPropertyAccessor(obj.GetType(), propertyName)?.GetValue(obj);
        }

        public static object GetPropertyValue(Type type, string propertyName)
        {
            if (type == null)
            {
                return null;
            }

            return GetPropertyAccessor(type, propertyName)?.GetValue(null);
        }

        internal static DependencyPropertyInfo<TDependencyProperty> GetDependencyPropertyInfo<TDependencyProperty>(Type type, string propertyName)
            where TDependencyProperty : class
        {
            Dictionary<string, DependencyPropertyInfo<TDependencyProperty>> propertyInfos;
            if (!dependencyPropertyInfos.TryGetValue(type, out object result))
            {
                result = dependencyPropertyInfos[type] = new Dictionary<string, DependencyPropertyInfo<TDependencyProperty>>();
            }

            propertyInfos = (Dictionary<string, DependencyPropertyInfo<TDependencyProperty>>)result;
            if (!propertyInfos.TryGetValue(propertyName, out DependencyPropertyInfo<TDependencyProperty> dependencyPropertyInfo))
            {
                if (dependencyPropertiesAreFields)
                {
                    var field = GetFieldInfo(type, $"{propertyName}Property");
                    if (field != null)
                    {
                        dependencyPropertyInfo = new DependencyPropertyInfo<TDependencyProperty>(field.GetValue(null) as TDependencyProperty, field.DeclaringType, field.Name);
                    }
                }
                else
                {
                    // UWP declares dependencyproperties as properties
                    var property = GetPropertyInfo(type, $"{propertyName}Property");
                    if (property != null)
                    {
                        dependencyPropertyInfo = new DependencyPropertyInfo<TDependencyProperty>(property.GetValue(null) as TDependencyProperty, property.DeclaringType, property.Name);
                    }
                }

                propertyInfos[propertyName] = dependencyPropertyInfo;
            }

            return dependencyPropertyInfo;
        }

        public static void SetPropertyValue(object obj, string propertyName, object value)
        {
            if (obj == null)
            {
                return;
            }

            GetPropertyAccessor(obj.GetType(), propertyName)?.SetValue(obj, value);
        }

        public static void SetPropertyValue(Type type, string propertyName, object value)
        {
            if (type == null)
            {
                return;
            }

            GetPropertyAccessor(type, propertyName)?.SetValue(null, value);
        }

        private static IDictionary<string, TDependencyProperty> DeclaredDependencyProperties<TDependencyProperty>(Type type)
            where TDependencyProperty : class
        {
            object result;
            if (!dependencyProperties.TryGetValue(type, out result))
            {
                DeclaredDependencyPropertyInfos<TDependencyProperty>(type);
                result = dependencyProperties[type];
            }

            return (IDictionary<string, TDependencyProperty>)result;
        }

        private static IDictionary<string, DependencyPropertyInfo<TDependencyProperty>> DeclaredDependencyPropertyInfos<TDependencyProperty>(Type type)
            where TDependencyProperty : class
        {
            object result;
            if (!dependencyPropertyInfos.TryGetValue(type, out result))
            {
                List<DependencyPropertyInfo<TDependencyProperty>> dps;

                if (dependencyPropertiesAreFields)
                {
                    dps = DeclaredFields(type).Values
                        .Where(x => x.FieldType == typeof(TDependencyProperty))
                        .Select(x => new DependencyPropertyInfo<TDependencyProperty>(x.GetValue(null) as TDependencyProperty, x.DeclaringType, x.Name))
                        .ToList();
                }
                else
                {
                    // UWP declares dependencyproperties as properties
                    dps = DeclaredProperties(type).Values
                        .Where(x => x.PropertyType == typeof(TDependencyProperty))
                        .Select(x => new DependencyPropertyInfo<TDependencyProperty>(x.GetValue(null) as TDependencyProperty, x.DeclaringType, x.Name))
                        .ToList();
                }

                var dict = dps.ToDictionary(x => x.ShortName, x => x);
                dependencyPropertyInfos[type] = dict;
                dependencyProperties[type] = dps.ToDictionary(x => x.ShortName, x => x.Property);

                result = dict;
            }

            return (IDictionary<string, DependencyPropertyInfo<TDependencyProperty>>)result;
        }

        internal static PropertyInfo DeclaredProperty(Type type, string propertyName)
        {
            var properties = DeclaredProperties(type);

            if (!properties.TryGetValue(propertyName, out PropertyInfo propertyInfo))
            {
                propertyInfo = properties[propertyName] = type.GetRuntimeProperty(propertyName);

            }

            return propertyInfo;
        }
    }

    /// <summary>
    /// From https://stackoverflow.com/questions/724143/how-do-i-create-a-delegate-for-a-net-property
    /// </summary>
    public interface IPropertyAccessor
    {
        PropertyInfo PropertyInfo { get; }
        object GetValue(object source);
        void SetValue(object source, object value);
    }

    /// <summary>
    /// From https://stackoverflow.com/questions/724143/how-do-i-create-a-delegate-for-a-net-property
    /// </summary>
    internal static class PropertyInfoHelper
    {
        private static Dictionary<PropertyInfo, IPropertyAccessor> _cache =
            new Dictionary<PropertyInfo, IPropertyAccessor>();

        public static IPropertyAccessor GetAccessor(PropertyInfo propertyInfo)
        {
            IPropertyAccessor result = null;
            if (!_cache.TryGetValue(propertyInfo, out result))
            {
                result = CreateAccessor(propertyInfo);
                _cache[propertyInfo] = result;
            }
            return result;
        }

        public static IPropertyAccessor CreateAccessor(PropertyInfo PropertyInfo)
        {
            var GenType = typeof(PropertyWrapper<,>)
                .MakeGenericType(PropertyInfo.DeclaringType, PropertyInfo.PropertyType);
            return (IPropertyAccessor)Activator.CreateInstance(GenType, PropertyInfo);
        }
    }

    /// <summary>
    /// From https://stackoverflow.com/questions/724143/how-do-i-create-a-delegate-for-a-net-property
    /// </summary>
    internal class PropertyWrapper<TObject, TValue> : IPropertyAccessor where TObject : class
    {
        private Func<TObject, TValue> Getter;
        private Action<TObject, TValue> Setter;

        public PropertyWrapper(PropertyInfo PropertyInfo)
        {
            this.PropertyInfo = PropertyInfo;

            MethodInfo GetterInfo = PropertyInfo.GetMethod;
            MethodInfo SetterInfo = PropertyInfo.SetMethod;

            Getter = (Func<TObject, TValue>)GetterInfo?.CreateDelegate
                    (typeof(Func<TObject, TValue>));
            Setter = (Action<TObject, TValue>)SetterInfo?.CreateDelegate
                    (typeof(Action<TObject, TValue>));
        }

        object IPropertyAccessor.GetValue(object source)
        {
            return Getter(source as TObject);
        }

        void IPropertyAccessor.SetValue(object source, object value)
        {
            Setter(source as TObject, (TValue)value);
        }

        public PropertyInfo PropertyInfo { get; private set; }
    }
}

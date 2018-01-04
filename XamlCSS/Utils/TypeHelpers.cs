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

        private static IDictionary<Type, IEnumerable<FieldInfo>> fieldInfos = new Dictionary<Type, IEnumerable<FieldInfo>>();
        private static IDictionary<Type, IEnumerable<PropertyInfo>> propertyInfos = new Dictionary<Type, IEnumerable<PropertyInfo>>();
        private static IDictionary<Type, object> dependencyPropertyInfos = new Dictionary<Type, object>();
        private static IDictionary<Type, object> dependencyPropertys = new Dictionary<Type, object>();

        public static IEnumerable<FieldInfo> DeclaredFields(Type type)
        {
            IEnumerable<FieldInfo> result;
            if (!fieldInfos.TryGetValue(type, out result))
            {
                var fields = new List<FieldInfo>();

                var typeInfo = type.GetTypeInfo();

                while (typeInfo != null)
                {
                    fields.AddRange(typeInfo.DeclaredFields);

                    typeInfo = typeInfo.BaseType?.GetTypeInfo();
                }

                fieldInfos[type] = fields;
                result = fields;
            }

            return result;
        }

        public static IEnumerable<PropertyInfo> DeclaredProperties(Type type)
        {
            IEnumerable<PropertyInfo> result;
            if (!propertyInfos.TryGetValue(type, out result))
            {
                var properties = new List<PropertyInfo>();
                var typeInfo = type.GetTypeInfo();

                while (typeInfo != null)
                {
                    properties.AddRange(typeInfo.DeclaredProperties);

                    typeInfo = typeInfo.BaseType?.GetTypeInfo();
                }

                propertyInfos[type] = properties;

                result = properties;
            }

            return result;
        }

        public static IDictionary<string, TDependencyProperty> DeclaredDependencyProperties<TDependencyProperty>(Type type)
            where TDependencyProperty : class
        {
            object result;
            if (!dependencyPropertys.TryGetValue(type, out result))
            {
                DeclaredDependencyPropertyInfos<TDependencyProperty>(type);
                result = dependencyPropertys[type];
            }

            return (IDictionary<string, TDependencyProperty>)result;
        }

        public static IDictionary<string, DependencyPropertyInfo<TDependencyProperty>> DeclaredDependencyPropertyInfos<TDependencyProperty>(Type type)
            where TDependencyProperty : class
        {
            object result;
            if (!dependencyPropertyInfos.TryGetValue(type, out result))
            {
                List<DependencyPropertyInfo<TDependencyProperty>> dps;

                if (dependencyPropertiesAreFields)
                {
                    dps = DeclaredFields(type)
                        .Where(x => x.FieldType == typeof(TDependencyProperty))
                        .Select(x => new DependencyPropertyInfo<TDependencyProperty>(x.GetValue(null) as TDependencyProperty, x.DeclaringType, x.Name))
                        .ToList();
                }
                else
                {
                    // UWP declares dependencyproperties as properties
                    dps = DeclaredProperties(type)
                        .Where(x => x.PropertyType == typeof(TDependencyProperty))
                        .Select(x => new DependencyPropertyInfo<TDependencyProperty>(x.GetValue(null) as TDependencyProperty, x.DeclaringType, x.Name))
                        .ToList();
                }
                
                var dict = dps.ToDictionary(x => x.ShortName, x => x);
                dependencyPropertyInfos[type] = dict;
                dependencyPropertys[type] = dps.ToDictionary(x => x.ShortName, x => x.Property);

                result = dict;
            }

            return (IDictionary<string, DependencyPropertyInfo<TDependencyProperty>>)result;
        }
    }
}

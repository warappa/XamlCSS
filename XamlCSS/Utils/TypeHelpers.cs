using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XamlCSS.Utils
{
    public static class TypeHelpers
    {
        private static IDictionary<Type, IEnumerable<FieldInfo>> fieldInfos = new Dictionary<Type, IEnumerable<FieldInfo>>();
        private static IDictionary<Type, IEnumerable<PropertyInfo>> propertyInfos = new Dictionary<Type, IEnumerable<PropertyInfo>>();
        private static IDictionary<Type, object> dependencyPropertyInfos = new Dictionary<Type, object>();

        public static IEnumerable<FieldInfo> DeclaredFields(Type type)
        {
            if (fieldInfos.ContainsKey(type))
            {
                return fieldInfos[type];
            }

            var originalType = type;
            var fields = new List<FieldInfo>();

            while (type != null)
            {
                fields.AddRange(type.GetRuntimeFields());

                var baseType = type.GetTypeInfo().BaseType;

                type = baseType;
            }

            fieldInfos[originalType] = fields;

            return fields;
        }

        public static IEnumerable<PropertyInfo> DeclaredProperties(Type type)
        {
            if (propertyInfos.ContainsKey(type))
            {
                return propertyInfos[type];
            }

            var originalType = type;
            var properties = new List<PropertyInfo>();

            while (type != null)
            {
                properties.AddRange(type.GetTypeInfo().DeclaredProperties);

                var baseType = type.GetTypeInfo().BaseType;

                type = baseType;
            }

            propertyInfos[originalType] = properties;

            return properties;
        }

        public static IEnumerable<DependencyPropertyInfo<TDependencyProperty>> DeclaredDependencyProperties<TDependencyProperty>(Type type)
        where TDependencyProperty : class
        {
            if (dependencyPropertyInfos.ContainsKey(type))
            {
                return (IEnumerable<DependencyPropertyInfo<TDependencyProperty>>)dependencyPropertyInfos[type];
            }

            var dps = DeclaredFields(type)
                .Where(x => x.FieldType == typeof(TDependencyProperty))
                .Select(x => new DependencyPropertyInfo<TDependencyProperty>(x.GetValue(null) as TDependencyProperty, x.DeclaringType, x.Name))
                .ToList();


            dependencyPropertyInfos[type] = dps;

            return dps;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace XamlCSS.Utils
{
    public static class TypeHelpers
    {
        private static bool dependencyPropertiesAreFields = true;
        private static IDictionary<string, List<string>> namespaceMapping = new Dictionary<string, List<string>>();

        public static void Initialize(IDictionary<string, List<string>> namespaceMapping, bool dependencyPropertiesAreFields = true)
        {
            TypeHelpers.namespaceMapping = namespaceMapping;
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
            Dictionary<string, IPropertyAccessor> propertyAccessors = null;
            IPropertyAccessor accessor = null;

            if (!cachedPropertyAccessors.TryGetValue(type, out propertyAccessors))
            {
                cachedPropertyAccessors[type] = propertyAccessors = new Dictionary<string, IPropertyAccessor>();
            }

            if (!propertyAccessors.TryGetValue(propertyName, out accessor))
            {
                var propertyInfo = type.GetRuntimeProperty(propertyName);

                propertyAccessors[propertyName] = accessor = propertyInfo == null ? null : PropertyInfoHelper.GetAccessor(propertyInfo);
            }

            return accessor;
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

        public static object GetClrPropertyValue(List<CssNamespace> namespaces, object obj, string propertyExpression)
        {
            var typeAndProperyName = ResolveFullTypeNameAndPropertyName(namespaces, propertyExpression, obj.GetType());

            var type = Type.GetType(typeAndProperyName.Item1);
            return TypeHelpers.GetPropertyValue(obj, typeAndProperyName.Item2);
        }

        public static Type GetClrPropertyType(IList<CssNamespace> namespaces, object obj, string propertyExpression)
        {
            var typeAndProperyName = ResolveFullTypeNameAndPropertyName(namespaces, propertyExpression, obj.GetType());

            var type = Type.GetType(typeAndProperyName.Item1);
            return TypeHelpers.DeclaredProperty(type, typeAndProperyName.Item2).PropertyType;
        }

        private static IDictionary<string, IDictionary<Type, Tuple<string, string>>> resolveFullTypeNameAndPropertyNameDictionary = new Dictionary<string, IDictionary<Type, Tuple<string, string>>>();


        public static Tuple<string, string> ResolveFullTypeNameAndPropertyName(IList<CssNamespace> namespaces, string cssPropertyExpression, Type matchedType)
        {
            Tuple<string, string> result = null;
            IDictionary<Type, Tuple<string, string>> map;
            string namespaceUri = null;

            if (resolveFullTypeNameAndPropertyNameDictionary.TryGetValue(cssPropertyExpression, out map))
            {
                foreach (var nspace in namespaces)
                {
                    if (map.TryGetValue(matchedType, out result))
                    {
                        return result;
                    }
                }

                //namespaceUri = matchedType.AssemblyQualifiedName.Replace($".{matchedType.Name}, ", ", ");
                //if (map.TryGetValue(matchedType, out result))
                //{
                //    return result;
                //}
            }
            else
            {
                map = resolveFullTypeNameAndPropertyNameDictionary[cssPropertyExpression] = new Dictionary<Type, Tuple<string, string>>();
            }

            string typename = null, propertyName = null;

            if (cssPropertyExpression.IndexOf('|') > -1)
            {
                var strs = cssPropertyExpression.Split('|', '.');
                var alias = strs[0];
                var shortTypename = strs[1];

                namespaceUri = namespaces
                    .First(x => x.Alias == alias)
                    .Namespace;

                var namespaceFragments = EnsureAssemblyQualifiedName(namespaceUri, shortTypename)
                    .Split(separator, 2);

                typename = $"{namespaceFragments[0]}.{shortTypename}, {string.Join(",", namespaceFragments.Skip(1))}";
                propertyName = strs[2];
            }
            else if (cssPropertyExpression.IndexOf('.') > -1)
            {
                var strs = cssPropertyExpression.Split('.');
                var shortTypename = strs[0];

                namespaceUri = namespaces
                    .First(x => x.Alias == "")
                    .Namespace;

                var namespaceFragments = EnsureAssemblyQualifiedName(namespaceUri, shortTypename)
                    .Split(separator, 2);

                typename = $"{namespaceFragments[0]}.{shortTypename}, {string.Join(",", namespaceFragments.Skip(1))}";
                propertyName = strs[1];
            }
            else
            {
                typename = matchedType.AssemblyQualifiedName;
                propertyName = cssPropertyExpression;

                namespaceUri = matchedType.AssemblyQualifiedName.Replace($".{matchedType.Name}, ", ", ");
            }

            result = new Tuple<string, string>(typename, propertyName);
            map[matchedType] = result;
            return result;
        }

        private static char[] separator = new[] { ',' };
        private static IDictionary<string, IDictionary<string, string>> resolvedNames = new Dictionary<string, IDictionary<string, string>>();
        public static string EnsureAssemblyQualifiedName(string namespaceUri, string shortTypename)
        {
            if (namespaceUri == null)
            {
                return null;
            }

            string testTypename;
            string[] namespaceFragments;

            IDictionary<string, string> t;

            if (resolvedNames.TryGetValue(namespaceUri, out t))
            {
                if (t.TryGetValue(shortTypename, out string assemblyQualifiedName))
                {
                    return assemblyQualifiedName;
                }
            }
            else
            {
                t = resolvedNames[namespaceUri] = new Dictionary<string, string>();
            }

            if (namespaceMapping.TryGetValue(namespaceUri, out List<string> mapped))
            {
                foreach (var fullQualifiedNamespaceName in mapped)
                {
                    namespaceFragments = fullQualifiedNamespaceName
                               .Split(separator, 2);
                    testTypename = $"{namespaceFragments[0]}.{shortTypename},{string.Join(",", namespaceFragments.Skip(1))}";
                    var parsedType = Type.GetType(testTypename, false);
                    if (parsedType != null)
                    {
                        t[shortTypename] = fullQualifiedNamespaceName;
                        return fullQualifiedNamespaceName;
                    }
                }
            }

            namespaceFragments = namespaceUri.Split(separator);
            testTypename = $"{namespaceFragments[0]}.{shortTypename},{string.Join(",", namespaceFragments.Skip(1))}";
            
            string resolvedName = null;
            var resolved = Type.GetType(testTypename, false);

            if (resolved == null)
            {
                var testNamespaceUri = namespaceMapping.Values
                    .SelectMany(x => x)
                    .FirstOrDefault(x => x.StartsWith(namespaceUri + ","));

                if (testNamespaceUri != null)
                {
                    namespaceFragments = testNamespaceUri.Split(separator);

                    testTypename = $"{namespaceFragments[0]}.{shortTypename},{string.Join(",", namespaceFragments.Skip(1))}";
                    if (testTypename != null)
                    {
                        resolved = Type.GetType(testTypename, false);
                    }
                }
            }

            if (resolved != null)
            {
                t[shortTypename] = resolvedName = resolved.AssemblyQualifiedName.Replace($".{shortTypename},", ","); // convert simple qualified name to full qualified name
                return resolvedName;
            }

            t[shortTypename] = namespaceUri;
            return namespaceUri;
        }

        public static string ResolveFullTypeName(IList<CssNamespace> namespaces, string cssTypeExpression)
        {
            string typename;

            if (cssTypeExpression.IndexOf('|') > -1)
            {
                var strs = cssTypeExpression.Split('|');
                var alias = strs[0];
                var shortTypename = strs[1];

                var namespaceUri = namespaces
                    .FirstOrDefault(x => x.Alias == alias)
                    ?.Namespace;

                namespaceUri = EnsureAssemblyQualifiedName(namespaceUri, shortTypename);

                var namespaceFragments = namespaceUri
                    .Split(separator, 2);

                if (namespaceFragments == null)
                {
                    throw new Exception($@"Namespace ""{alias}"" not found!");
                }

                typename = $"{namespaceFragments[0]}.{shortTypename},{string.Join(",", namespaceFragments.Skip(1))}";
            }
            else
            {
                var strs = cssTypeExpression.Split('.');
                var shortTypename = strs[0];

                var namespaceUri = namespaces
                    .First(x => x.Alias == "")
                    .Namespace;

                var namespaceFragments = EnsureAssemblyQualifiedName(namespaceUri, shortTypename)
                    .Split(separator, 2);

                typename = $"{namespaceFragments[0]}.{strs[0]},{string.Join(",", namespaceFragments.Skip(1))}";
            }

            return typename;
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
            var genType = typeof(PropertyWrapper<,>)
                .MakeGenericType(PropertyInfo.DeclaringType, PropertyInfo.PropertyType);
            return (IPropertyAccessor)Activator.CreateInstance(genType, PropertyInfo);
        }
    }

    /// <summary>
    /// From https://stackoverflow.com/questions/724143/how-do-i-create-a-delegate-for-a-net-property
    /// </summary>
    [XamlCSS.Linker.Preserve(AllMembers = true)]
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

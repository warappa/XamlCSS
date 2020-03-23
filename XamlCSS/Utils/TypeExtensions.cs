using System;
using System.Reflection;

namespace XamlCSS.Utils
{
    internal static class TypeExtensions
    {
        private const BindingFlags ResolveFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        public static FieldInfo GetFieldReliable(this Type type, string fieldName)
        {
            return type.GetField(fieldName, ResolveFlags);
        }
        public static PropertyInfo GetPropertyReliable(this Type type, string fieldName)
        {
            return type.GetProperty(fieldName, ResolveFlags);
        }
    }
}

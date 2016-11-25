using System;
using System.Collections.Generic;
using System.Reflection;

namespace XamlCSS
{
	public static class TypeHelpers
	{
		public static IEnumerable<FieldInfo> DeclaredFields(Type type)
		{
			var fields = new List<FieldInfo>();

			while (type != null)
			{
				fields.AddRange(type.GetRuntimeFields());
				
				var baseType = type.GetTypeInfo().BaseType;

				type = baseType;
			}

			return fields;
		}

		public static IEnumerable<PropertyInfo> DeclaredProperties(Type type)
		{
			var properties = new List<PropertyInfo>();

			while (type != null)
			{
				properties.AddRange(type.GetTypeInfo().DeclaredProperties);

				var baseType = type.GetTypeInfo().BaseType;

				type = baseType;
			}

			return properties;
		}
	}
}

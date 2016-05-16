using System;
using System.Collections.Generic;
using System.Reflection;

namespace XamlCSS
{
	public class TypeHelpers
	{
		public static FieldInfo[] DeclaredFields(Type type)
		{
			var fields = new List<FieldInfo>();

			while (type != null)
			{
				fields.AddRange(type.GetRuntimeFields());
				
				Type type2 = type.GetTypeInfo().BaseType;
				type = type2 != null ? type2 : null;
			}

			return fields.ToArray();
		}

		public static PropertyInfo[] DeclaredProperties(Type type)
		{
			var fields = new List<PropertyInfo>();

			while (type != null)
			{
				fields.AddRange(type.GetTypeInfo().DeclaredProperties);

				Type type2 = type.GetTypeInfo().BaseType;
				type = type2 != null ? type2 : null;
			}

			return fields.ToArray();
		}
	}
}

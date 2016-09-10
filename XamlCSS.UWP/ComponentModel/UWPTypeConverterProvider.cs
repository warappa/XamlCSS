using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.UI;

namespace XamlCSS.ComponentModel
{
	public class UWPTypeConverterProvider : ITypeConverterProvider<TypeConverter>
	{
		protected static IDictionary<Type, TypeConverter> converterMapping = new Dictionary<Type, TypeConverter>();

		public UWPTypeConverterProvider()
		{
			converterMapping = new Dictionary<Type, TypeConverter>();

			Register<bool, NumberTypeConverter<bool>>();
			Register<byte, NumberTypeConverter<byte>>();
			Register<int, NumberTypeConverter<int>>();
			Register<uint, NumberTypeConverter<uint>>();
			Register<long, NumberTypeConverter<long>>();
			Register<ulong, NumberTypeConverter<ulong>>();
			Register<float, NumberTypeConverter<float>>();
			Register<double, NumberTypeConverter<double>>();
			
			Register<Color, ColorTypeConverter>();
		}

		public void RegisterEnum<TEnum>()
		{
            if (!typeof(Enum).GetTypeInfo().IsAssignableFrom(typeof(TEnum).GetTypeInfo()))
            {
                throw new InvalidOperationException($"Type '{typeof(TEnum).FullName}' is not an enum!");
            }

			Register<TEnum, EnumTypeConverter<TEnum>>();
		}

		public void Register<TTargetType>(TypeConverter converter)
		{
			if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            var key = typeof(TTargetType);

            if (converterMapping.ContainsKey(key))
            {
                throw new InvalidOperationException($"Converter for type '{key.FullName}' is already registered!");
            }

            converterMapping[key] = converter;
		}

		public void Register<TTargetType, TTypeConverter>()
			where TTypeConverter : TypeConverter, new()
		{
			var key = typeof(TTargetType);

			if (converterMapping.ContainsKey(key))
            {
                throw new InvalidOperationException($"Converter for type '{key.FullName}' is already registered!");
            }

            converterMapping[key] = Activator.CreateInstance<TTypeConverter>();
		}

		public TypeConverter GetConverterFromProperty(string propertyName, Type type)
		{
			return null;
		}

		public TypeConverter GetConverter(Type targetDataType)
		{
			if (converterMapping.ContainsKey(targetDataType))
            {
                return converterMapping[targetDataType];
            }

            return null;
		}
	}
}

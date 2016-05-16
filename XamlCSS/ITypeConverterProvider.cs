using System;

namespace XamlCSS
{
	public interface ITypeConverterProvider<TTypeConverter>
	{
		void RegisterEnum<TEnum>();
		void Register<TTargetType>(TTypeConverter converter);
		void Register<TTargetType, TSpecificTypeConverter>()
			where TSpecificTypeConverter : TTypeConverter, new();
		TTypeConverter GetConverter(Type targetDataType);
		TTypeConverter GetConverterFromProperty(string propertyName, Type type);
	}
}

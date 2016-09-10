using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace XamlCSS.ComponentModel
{
	public class XamarinTypeConverterProvider: ITypeConverterProvider<TypeConverter>
	{
		protected static IDictionary<Type, TypeConverter> converterMapping = new Dictionary<Type, TypeConverter>();

		public XamarinTypeConverterProvider()
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

			Register<Font, FontTypeConverter>();
			Register<Binding, BindingTypeConverter>();
			Register<BoundsConstraint, BoundsTypeConverter>();
			Register<Color, ColorTypeConverter>();
			Register<Constraint, ConstraintTypeConverter>();
			Register<GridLength, GridLengthTypeConverter>();
			Register<Keyboard, KeyboardTypeConverter>();
			Register<Point, PointTypeConverter>();
			Register<Rectangle, RectangleTypeConverter>();
			Register<Thickness, ThicknessTypeConverter>();
			Register<Type, TypeTypeConverter>();
			Register<Uri, UriTypeConverter>();
			Register<UrlWebViewSource, WebViewSourceTypeConverter>();
			
			RegisterEnum<AbsoluteLayoutFlags>();
			RegisterEnum<Aspect>();
			RegisterEnum<BindingMode>();
			RegisterEnum<ConstraintType>();
			RegisterEnum<DependencyFetchTarget>();
			RegisterEnum<FontAttributes>();
			RegisterEnum<GestureState>();
			RegisterEnum<GestureStatus>();
			RegisterEnum<GridUnitType>();
			RegisterEnum<KeyboardFlags>();
			RegisterEnum<LayoutAlignment>();
			RegisterEnum<LineBreakMode>();
			RegisterEnum<ListViewCachingStrategy>();
			RegisterEnum<MasterBehavior>();
			RegisterEnum<MeasureFlags>();
			RegisterEnum<NamedSize>();
			RegisterEnum<ScrollOrientation>();
			RegisterEnum<ScrollToMode>();
			RegisterEnum<ScrollToPosition>();
			RegisterEnum<SeparatorVisibility>();
			RegisterEnum<StackOrientation>();
			RegisterEnum<TableIntent>();
			RegisterEnum<TargetIdiom>();
			RegisterEnum<TargetPlatform>();
			RegisterEnum<TextAlignment>();
			RegisterEnum<ToolbarItemOrder>();
			RegisterEnum<ViewState>();
			RegisterEnum<WebNavigationEvent>();
			RegisterEnum<WebNavigationResult>();

			Register<LayoutOptions, LayoutOptionsConverter>();
		}

		public void RegisterEnum<TEnum>()
		{
			if (typeof(Enum).GetTypeInfo().IsAssignableFrom(typeof(TEnum).GetTypeInfo()) == false)
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
			var dpProperties = TypeHelpers.DeclaredProperties(type);

			var property = dpProperties.Where(x => x.Name == propertyName).ToArray();

			return property
				.Where(x => x.GetCustomAttributes<TypeConverterAttribute>().Any())
				.Select(x => x.GetCustomAttribute<TypeConverterAttribute>())
				.Select(x => Type.GetType(x.ConverterTypeName))
				.Select(x => (TypeConverter)Activator.CreateInstance(x))
				.FirstOrDefault();
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

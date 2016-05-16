using System;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using XamlCSS.ComponentModel;

namespace XamlCSS.XamarinForms
{
	public class DependencyPropertyService : IDependencyPropertyService<BindableObject, VisualElement, Style, BindableProperty>
	{
		private ITypeConverterProvider<TypeConverter> typeConverterProvider;

		public DependencyPropertyService()
		{
			this.typeConverterProvider = new XamarinTypeConverterProvider();
		}

		public BindableProperty GetBindableProperty(BindableObject frameworkElement, string propertyName)
		{
			return GetBindableProperty(frameworkElement.GetType(), propertyName);
		}
		public BindableProperty GetBindableProperty(Type bindableObjectType, string propertyName)
		{
			string dpName = propertyName + "Property";
			var dpFields = TypeHelpers.DeclaredFields(bindableObjectType);
			var dpField = dpFields.FirstOrDefault(i => i.Name == dpName);

			if (dpField != null)
				return dpField.GetValue(null) as BindableProperty;
			return null;
		}
		
		public object GetBindablePropertyValue(BindableObject frameworkElement, BindableProperty bindableProperty, object propertyValue)
		{
			return GetBindablePropertyValue(frameworkElement.GetType(), bindableProperty, propertyValue);
		}

		public object GetBindablePropertyValue(Type frameworkElementType, BindableProperty bindableProperty, object propertyValue)
		{
			if (!(bindableProperty.ReturnType.GetTypeInfo()
				.IsAssignableFrom(propertyValue.GetType().GetTypeInfo())))
			{
				Type propertyType = bindableProperty.ReturnType;
				TypeConverter converter = null;
				
				converter = typeConverterProvider.GetConverterFromProperty(bindableProperty.PropertyName, frameworkElementType);

				if (converter == null)
					converter = typeConverterProvider.GetConverter(propertyType);
				if (converter != null)
					propertyValue = converter.ConvertFromInvariantString(propertyValue as string);

				else if (propertyType == typeof(bool))
					propertyValue = propertyValue.Equals("true");
				else if (propertyType == typeof(Color))
					propertyValue = Color.FromHex(propertyValue as string);
				else if (propertyType == typeof(LayoutOptions))
					propertyValue = propertyType.GetRuntimeFields().First(x => x.Name == propertyValue as string).GetValue(null);
				else if (propertyType.GetTypeInfo().IsEnum)
					propertyValue = Enum.Parse(propertyType, propertyValue as string);
				else
					propertyValue = Convert.ChangeType(propertyValue, propertyType);
			}

			return propertyValue;
		}

		public string[] GetAppliedMatchingStyles(BindableObject obj)
		{
			return obj.GetValue(Css.AppliedMatchingStylesProperty) as string[];
		}

		public string GetClass(BindableObject obj)
		{
			return obj.GetValue(Css.ClassProperty) as string;
		}

		public bool? GetHadStyle(BindableObject obj)
		{
			return obj.GetValue(Css.HadStyleProperty) as bool?;
		}

		public Style GetInitialStyle(BindableObject obj)
		{
			return obj.GetValue(Css.InitialStyleProperty) as Style;
		}

		public string[] GetMatchingStyles(BindableObject obj)
		{
			return obj.GetValue(Css.MatchingStylesProperty) as string[];
		}

		public string GetName(BindableObject obj)
		{
			return obj.GetValue(Css.IdProperty) as string;
		}

		public StyleDeclarationBlock GetStyle(BindableObject obj)
		{
			return obj.GetValue(Css.StyleProperty) as StyleDeclarationBlock;
		}

		public StyleSheet GetStyleSheet(BindableObject obj)
		{
			return obj.GetValue(Css.StyleSheetProperty) as StyleSheet;
		}

		public void SetAppliedMatchingStyles(BindableObject obj, string[] value)
		{
			obj.SetValue(Css.AppliedMatchingStylesProperty, value);
		}

		public void SetClass(BindableObject obj, string value)
		{
			obj.SetValue(Css.ClassProperty, value);
		}

		public void SetHadStyle(BindableObject obj, bool? value)
		{
			obj.SetValue(Css.HadStyleProperty, value);
		}

		public void SetInitialStyle(BindableObject obj, Style value)
		{
			obj.SetValue(Css.InitialStyleProperty, value);
		}

		public void SetMatchingStyles(BindableObject obj, string[] value)
		{
			obj.SetValue(Css.MatchingStylesProperty, value);
		}

		public void SetName(BindableObject obj, string value)
		{
			obj.SetValue(Css.IdProperty, value);
		}

		public void SetStyle(BindableObject obj, StyleDeclarationBlock value)
		{
			obj.SetValue(Css.StyleProperty, value);
		}

		public void SetStyleSheet(BindableObject obj, StyleSheet value)
		{
			obj.SetValue(Css.StyleSheetProperty, value);
		}

		public bool IsLoaded(VisualElement obj)
		{
			return obj.Parent != null;
		}

		public void RegisterLoadedOnce(VisualElement frameworkElement, Action<object> func)
		{
			EventHandler handler = null;
			handler = (s, e) =>
			{
				frameworkElement.BindingContextChanged -= handler;
				func(s);
			};
			frameworkElement.BindingContextChanged += handler;
		}
	}
}

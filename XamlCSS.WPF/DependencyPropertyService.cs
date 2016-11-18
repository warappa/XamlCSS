using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace XamlCSS.WPF
{
	public class DependencyPropertyService : IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty>
	{
		public DependencyProperty GetBindableProperty(DependencyObject frameworkElement, string propertyName)
		{
			return GetBindableProperty(frameworkElement.GetType(), propertyName);
		}
		public DependencyProperty GetBindableProperty(Type bindableObjectType, string propertyName)
		{
			string dpName = propertyName + "Property";
			var dpFields = TypeHelpers.DeclaredFields(bindableObjectType);
			var dpField = dpFields.FirstOrDefault(i => i.Name == dpName);

			if (dpField != null)
				return dpField.GetValue(null) as DependencyProperty;
			return null;
		}

		public object GetBindablePropertyValue(Type frameworkElementType, DependencyProperty property, object propertyValue)
		{
			if (property != null &&
				!(property.PropertyType.IsAssignableFrom(propertyValue.GetType())))
			{
				var propertyType = property.PropertyType;
				
				var converter = TypeDescriptor.GetConverter(propertyType);

				if (converter == null)
				{
					converter = TypeDescriptor.GetConverter(propertyType);
				}

				if (converter != null)
				{
					propertyValue = converter.ConvertFrom(propertyValue as string);
				}
				else if (propertyType == typeof(bool))
				{
					propertyValue = propertyValue.Equals("true");
				}
				else if (propertyType.IsEnum)
				{
					propertyValue = Enum.Parse(propertyType, propertyValue as string);
				}
				else
				{
					propertyValue = Convert.ChangeType(propertyValue, propertyType);
				}
			}

			return propertyValue;
		}

		protected object ReadSafe(DependencyObject obj, DependencyProperty property)
		{
			var val = obj.ReadLocalValue(property);
			if (val == DependencyProperty.UnsetValue)
			{
				return null;
			}

			return val;
		}

		public string[] GetAppliedMatchingStyles(DependencyObject obj)
		{
			return (string[])ReadSafe(obj, Css.AppliedMatchingStylesProperty);
		}

		public string GetClass(DependencyObject obj)
		{
			return (string)ReadSafe(obj, Css.ClassProperty);
		}

		public bool? GetHadStyle(DependencyObject obj)
		{
			return (bool?)ReadSafe(obj, Css.HadStyleProperty);
		}

		public Style GetInitialStyle(DependencyObject obj)
		{
			return (Style)ReadSafe(obj, Css.InitialStyleProperty);
		}

		public string[] GetMatchingStyles(DependencyObject obj)
		{
			return (string[])ReadSafe(obj, Css.MatchingStylesProperty);
		}

		public string GetName(DependencyObject obj)
		{
			return (obj as FrameworkElement)?.Name;
		}

		public StyleDeclarationBlock GetStyle(DependencyObject obj)
		{
			return (StyleDeclarationBlock)ReadSafe(obj, Css.StyleProperty);
		}

		public StyleSheet GetStyleSheet(DependencyObject obj)
		{
			return (StyleSheet)ReadSafe(obj, Css.StyleSheetProperty);
		}

		public bool GetHandledCss(DependencyObject obj)
		{
			return ((bool?)ReadSafe(obj, Css.HandledCssProperty) ?? false);
		}

		public void SetAppliedMatchingStyles(DependencyObject obj, string[] value)
		{
			obj.SetValue(Css.AppliedMatchingStylesProperty, value);
		}

		public void SetClass(DependencyObject obj, string value)
		{
			obj.SetValue(Css.ClassProperty, value);
		}

		public void SetHadStyle(DependencyObject obj, bool? value)
		{
			obj.SetValue(Css.HadStyleProperty, value);
		}

		public void SetInitialStyle(DependencyObject obj, Style value)
		{
			obj.SetValue(Css.InitialStyleProperty, value);
		}

		public void SetMatchingStyles(DependencyObject obj, string[] value)
		{
			obj.SetValue(Css.MatchingStylesProperty, value);
		}

		public void SetName(DependencyObject obj, string value)
		{
			(obj as FrameworkElement).Name = value;
		}

		public void SetStyle(DependencyObject obj, StyleDeclarationBlock value)
		{
			obj.SetValue(Css.StyleProperty, value);
		}

		public void SetStyleSheet(DependencyObject obj, StyleSheet value)
		{
			obj.SetValue(Css.StyleSheetProperty, value);
		}
		
		public void SetHandledCss(DependencyObject obj, bool value)
		{
			obj.SetValue(Css.HandledCssProperty, value);
		}

		public bool IsLoaded(DependencyObject obj)
		{
			if (obj is FrameworkElement)
			{
				return (obj as FrameworkElement).IsLoaded;
			}

			if (obj is FrameworkContentElement)
			{
				return (obj as FrameworkContentElement).IsLoaded;
			}

			return DesignerProperties.GetIsInDesignMode(obj);
		}

		public void RegisterLoadedOnce(DependencyObject element, Action<object> func)
		{
			var frameworkElement = element as FrameworkElement;
			if (frameworkElement != null)
			{
				RoutedEventHandler handler = null;
				handler = (s, e) =>
				{
					frameworkElement.Loaded -= handler;
					func(s);
				};

				frameworkElement.Loaded += handler;
			}
			else
			{
				var frameworkContentElement = element as FrameworkContentElement;
				if (frameworkContentElement != null)
				{
					RoutedEventHandler handler = null;
					handler = (s, e) =>
					{
						frameworkContentElement.Loaded -= handler;
						func(s);
					};

					frameworkContentElement.Loaded += handler;
				}
			}
		}
	}
}

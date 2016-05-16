using Windows.UI.Xaml;

namespace XamlCSS.UWP
{
	public class Css
	{
		public readonly static BaseCss<DependencyObject, FrameworkElement, Style, DependencyProperty> instance =
			new BaseCss<DependencyObject, FrameworkElement, Style, DependencyProperty>(
				new DependencyPropertyService(),
				new TreeNodeProvider(),
				new StyleResourceService(),
				new StyleService()
				);

		#region dependency properties

		public static readonly DependencyProperty MatchingStylesProperty =
			DependencyProperty.RegisterAttached(
				"MatchingStyles",
				typeof(string[]),
				typeof(Css),
				new PropertyMetadata(null));
		public static string[] GetMatchingStyles(DependencyObject obj)
		{
			return obj.ReadLocalValue(MatchingStylesProperty) as string[];
		}
		public static void SetMatchingStyles(DependencyObject obj, string[] value)
		{
			obj.SetValue(MatchingStylesProperty, value);
		}

		public static readonly DependencyProperty AppliedMatchingStylesProperty =
			DependencyProperty.RegisterAttached(
				"AppliedMatchingStyles",
				typeof(string[]),
				typeof(Css),
				new PropertyMetadata(null));
		public static string[] GetAppliedMatchingStyles(DependencyObject obj)
		{
			return obj.ReadLocalValue(AppliedMatchingStylesProperty) as string[];
		}
		public static void SetAppliedMatchingStyles(DependencyObject obj, string[] value)
		{
			obj.SetValue(AppliedMatchingStylesProperty, value);
		}

		public static readonly DependencyProperty InitialStyleProperty =
			DependencyProperty.RegisterAttached("InitialStyle", typeof(Style),
			typeof(Css), new PropertyMetadata(null));
		public static Style GetInitialStyle(DependencyObject obj)
		{
			return obj.ReadLocalValue(InitialStyleProperty) as Style;
		}
		public static void SetInitialStyle(DependencyObject obj, Style value)
		{
			obj.SetValue(InitialStyleProperty, value ?? DependencyProperty.UnsetValue);
		}

		public static readonly DependencyProperty HadStyleProperty =
			DependencyProperty.RegisterAttached("HadStyle", typeof(bool?),
			typeof(Css), new PropertyMetadata(null));
		public static bool? GetHadStyle(DependencyObject obj)
		{
			return obj.ReadLocalValue(HadStyleProperty) as bool?;
		}
		public static void SetHadStyle(DependencyObject obj, bool? value)
		{
			obj.SetValue(HadStyleProperty, value ?? DependencyProperty.UnsetValue);
		}

		public static readonly DependencyProperty StyleProperty =
			DependencyProperty.RegisterAttached("Style", typeof(StyleDeclarationBlock),
			typeof(Css), new PropertyMetadata(null, StylePropertyAttached));
		public static StyleDeclarationBlock GetStyle(DependencyObject obj)
		{
			return obj.ReadLocalValue(StyleProperty) as StyleDeclarationBlock;
		}
		public static void SetStyle(DependencyObject obj, StyleDeclarationBlock value)
		{
			obj.SetValue(StyleProperty, value ?? DependencyProperty.UnsetValue);
		}

		public static readonly DependencyProperty StyleSheetProperty =
			DependencyProperty.RegisterAttached("StyleSheet", typeof(StyleSheet),
			typeof(Css), new PropertyMetadata(null, StyleSheetPropertyAttached));
		public static StyleSheet GetStyleSheet(DependencyObject obj)
		{
			return obj.ReadLocalValue(StyleSheetProperty) as StyleSheet;
		}
		public static void SetStyleSheet(DependencyObject obj, StyleSheet value)
		{
			obj.SetValue(StyleSheetProperty, value ?? DependencyProperty.UnsetValue);
		}

		public static readonly DependencyProperty ClassProperty =
			DependencyProperty.RegisterAttached("Class", typeof(string),
			typeof(Css), new PropertyMetadata(null, null));
		public static string GetClass(DependencyObject obj)
		{
			return obj.ReadLocalValue(ClassProperty) as string;
		}
		public static void SetClass(DependencyObject obj, string value)
		{
			obj.SetValue(ClassProperty, value ?? DependencyProperty.UnsetValue);
		}

		public static readonly DependencyProperty IsLoadedProperty =
			DependencyProperty.RegisterAttached("IsLoaded", typeof(bool),
			typeof(Css), new PropertyMetadata(null, null));
		public static bool GetIsLoaded(DependencyObject obj)
		{
			var res = obj.ReadLocalValue(IsLoadedProperty);
			if (res == DependencyProperty.UnsetValue)
				return false;
			return (bool)res;
		}
		public static void SetIsLoaded(DependencyObject obj, bool value)
		{
			obj.SetValue(IsLoadedProperty, value == true ? true : DependencyProperty.UnsetValue);
		}

		#endregion

		#region attached behaviours

		private static void StyleSheetPropertyAttached(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FrameworkElement frameworkElement = d as FrameworkElement;

			var newStyleSheet = (StyleSheet)e.NewValue;

			if (newStyleSheet == null)
			{
				instance.RemoveStyleResources(frameworkElement, (StyleSheet)e.OldValue);
				return;
			}
			if (GetIsLoaded(frameworkElement) ||
				frameworkElement.Parent != null)
			{
				instance.UpdateElement(d);
			}
			else
			{
				Css.SetIsLoaded(frameworkElement, true);
				instance.UpdateElement(frameworkElement);
			}
		}
		private static void StylePropertyAttached(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			instance.UpdateElement(d as FrameworkElement);
		}

		#endregion
	}
}

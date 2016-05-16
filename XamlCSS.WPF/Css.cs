using System.Windows;

namespace XamlCSS.WPF
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

		public static readonly DependencyProperty MatchingStylesProperty =
			DependencyProperty.RegisterAttached(
				"MatchingStyles",
				typeof(string[]),
				typeof(Css),
				new PropertyMetadata(null));
		public static string[] GetMatchingStyles(DependencyObject obj)
		{
			return obj.GetValue(MatchingStylesProperty) as string[];
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
			return obj.GetValue(AppliedMatchingStylesProperty) as string[];
		}
		public static void SetAppliedMatchingStyles(DependencyObject obj, string[] value)
		{
			obj.SetValue(AppliedMatchingStylesProperty, value);
		}

		public static readonly DependencyProperty InitialStyleProperty =
			DependencyProperty.RegisterAttached(
				"InitialStyle",
				typeof(Style),
				typeof(Css),
				new PropertyMetadata(null));
		public static Style GetInitialStyle(DependencyObject obj)
		{
			return obj.GetValue(InitialStyleProperty) as Style;
		}
		public static void SetInitialStyle(DependencyObject obj, Style value)
		{
			obj.SetValue(InitialStyleProperty, value);
		}

		public static readonly DependencyProperty HadStyleProperty =
			DependencyProperty.RegisterAttached(
				"HadStyle",
				typeof(bool?),
				typeof(Css),
				new PropertyMetadata(null));
		public static bool? GetHadStyle(DependencyObject obj)
		{
			return obj.GetValue(HadStyleProperty) as bool?;
		}
		public static void SetHadStyle(DependencyObject obj, bool? value)
		{
			obj.SetValue(HadStyleProperty, value);
		}

		public static readonly DependencyProperty StyleProperty =
			DependencyProperty.RegisterAttached(
				"Style",
				typeof(StyleDeclarationBlock),
				typeof(Css),
				new PropertyMetadata(null, Css.StylePropertyAttached));
		private static void StylePropertyAttached(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			instance.UpdateElement(d);
		}
		public static StyleDeclarationBlock GetStyle(DependencyObject obj)
		{
			return obj.GetValue(StyleProperty) as StyleDeclarationBlock;
		}
		public static void SetStyle(DependencyObject obj, StyleDeclarationBlock value)
		{
			obj.SetValue(StyleProperty, value);
		}
		public static void SetStyleSheet(DependencyObject obj, StyleSheet value)
		{
			obj.SetValue(StyleSheetProperty, value);
		}

		public static readonly DependencyProperty StyleSheetProperty =
			DependencyProperty.RegisterAttached(
				"StyleSheet",
				typeof(StyleSheet),
				typeof(Css),
			new PropertyMetadata(null, Css.StyleSheetPropertyAttached));
		private static void StyleSheetPropertyAttached(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var frameworkElement = d as FrameworkElement;

			var newStyleSheet = (StyleSheet)e.NewValue;

			if (newStyleSheet == null)
			{
				instance.RemoveStyleResources(frameworkElement, (StyleSheet)e.OldValue);
				return;
			}

			if (instance.dependencyPropertyService.IsLoaded(frameworkElement))
				instance.EnqueueRenderStyleSheet(frameworkElement, e.NewValue as StyleSheet, frameworkElement);
			else
			{
				instance.dependencyPropertyService.RegisterLoadedOnce(
					frameworkElement, 
					f => instance.EnqueueRenderStyleSheet(f as FrameworkElement, e.NewValue as StyleSheet, f as FrameworkElement));
			}
		}

		public static readonly DependencyProperty ClassProperty =
			DependencyProperty.RegisterAttached(
				"Class",
				typeof(string),
				typeof(Css),
				new PropertyMetadata(null));
		public static string GetClass(DependencyObject obj)
		{
			return obj.GetValue(ClassProperty) as string;
		}
		public static void SetClass(DependencyObject obj, string value)
		{
			obj.SetValue(ClassProperty, value);
		}
	}
}

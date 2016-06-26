using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Xaml.Internals;
using XamlCSS.Dom;
using XamlCSS.Windows.Media;

namespace XamlCSS.XamarinForms
{
	public class Css
	{
		public readonly static BaseCss<BindableObject, Element, Style, BindableProperty> instance =
			new BaseCss<BindableObject, Element, Style, BindableProperty>(
				new DependencyPropertyService(),
				new TreeNodeProvider(),
				new StyleResourceService(),
				new StyleService(),
				DomElementBase<BindableObject, Element>.GetPrefix(typeof(Button)),
				new MarkupExtensionParser()
				);

		public static void Initialize()
		{
			VisualTreeHelper.SubTreeAdded += VisualTreeHelper_ChildAdded;
			VisualTreeHelper.SubTreeRemoved += VisualTreeHelper_ChildRemoved;
		}

		public static void EnqueueRenderStyleSheet(Element styleSheetHolder, StyleSheet styleSheet, Element startFrom)
		{
			instance.EnqueueRenderStyleSheet(styleSheetHolder, styleSheet, startFrom as Element);
		}

		public static readonly BindableProperty MatchingStylesProperty =
			BindableProperty.CreateAttached(
				"MatchingStyles",
				typeof(string[]),
				typeof(Css),
				null,
				BindingMode.TwoWay);
		public static string[] GetMatchingStyles(BindableObject obj)
		{
			return obj.GetValue(MatchingStylesProperty) as string[];
		}
		public static void SetMatchingStyles(BindableObject obj, string[] value)
		{
			obj.SetValue(MatchingStylesProperty, value);
		}

		public static readonly BindableProperty AppliedMatchingStylesProperty =
			BindableProperty.CreateAttached(
				"AppliedMatchingStyles",
				typeof(string[]),
				typeof(Css),
				null,
				BindingMode.TwoWay);
		public static string[] GetAppliedMatchingStyles(BindableObject obj)
		{
			return obj.GetValue(AppliedMatchingStylesProperty) as string[];
		}
		public static void SetAppliedMatchingStyles(BindableObject obj, string[] value)
		{
			obj.SetValue(AppliedMatchingStylesProperty, value);
		}

		public static readonly BindableProperty IdProperty =
			BindableProperty.CreateAttached(
				"Id",
				typeof(string),
				typeof(Css),
				null,
				BindingMode.TwoWay);
		public static string GetId(BindableObject obj)
		{
			return obj.GetValue(IdProperty) as string;
		}
		public static void SetId(BindableObject obj, string value)
		{
			obj.SetValue(IdProperty, value);
		}

		public static readonly BindableProperty InitialStyleProperty =
			BindableProperty.CreateAttached(
				"InitialStyle",
				typeof(Style),
				typeof(Css),
				null,
				BindingMode.TwoWay);
		public static Style GetInitialStyle(BindableObject obj)
		{
			return obj.GetValue(InitialStyleProperty) as Style;
		}
		public static void SetInitialStyle(BindableObject obj, Style value)
		{
			obj.SetValue(InitialStyleProperty, value);
		}

		public static readonly BindableProperty HadStyleProperty =
			BindableProperty.CreateAttached(
				"HadStyle",
				typeof(bool?),
				typeof(Css),
				null,
				BindingMode.TwoWay);
		public static bool? GetHadStyle(BindableObject obj)
		{
			return obj.GetValue(HadStyleProperty) as bool?;
		}
		public static void SetHadStyle(BindableObject obj, bool? value)
		{
			obj.SetValue(HadStyleProperty, value);
		}

		public static readonly BindableProperty StyleProperty =
			BindableProperty.CreateAttached(
				"Style",
				typeof(StyleDeclarationBlock),
				typeof(Css),
				null,
				BindingMode.TwoWay,
				null,
				Css.StylePropertyAttached);
		public static StyleDeclarationBlock GetStyle(BindableObject obj)
		{
			return obj.GetValue(StyleProperty) as StyleDeclarationBlock;
		}
		public static void SetStyle(BindableObject obj, StyleDeclarationBlock value)
		{
			obj.SetValue(StyleProperty, value);
		}

		public static readonly BindableProperty StyleSheetProperty =
			BindableProperty.CreateAttached(
				"StyleSheet",
				typeof(StyleSheet),
				typeof(Css),
				null,
				BindingMode.TwoWay,
				null,
				Css.StyleSheetPropertyAttached
				);
		public static StyleSheet GetStyleSheet(BindableObject obj)
		{
			return obj.GetValue(StyleSheetProperty) as StyleSheet;
		}
		public static void SetStyleSheet(BindableObject obj, StyleSheet value)
		{
			obj.SetValue(StyleSheetProperty, value);
		}

		public static readonly BindableProperty ClassProperty =
			BindableProperty.CreateAttached(
				"Class",
				typeof(string),
				typeof(Css),
				null,
				BindingMode.TwoWay);
		public static string GetClass(BindableObject obj)
		{
			return obj.GetValue(ClassProperty) as string;
		}
		public static void SetClass(BindableObject obj, string value)
		{
			obj.SetValue(ClassProperty, value);
		}

		private static void VisualTreeHelper_ChildAdded(object sender, EventArgs e)
		{
			instance.UpdateElement(sender as BindableObject);
		}
		private static void VisualTreeHelper_ChildRemoved(object sender, EventArgs e)
		{
			instance.UnapplyMatchingStyles(sender as Element);
		}

		private static void StyleSheetPropertyAttached(BindableObject d, object oldValue, object newValue)
		{
			var frameworkElement = d as Element;
			if (frameworkElement == null)
				return;
			var newStyleSheet = (StyleSheet)newValue;

			if (newStyleSheet == null)
			{
				instance.RemoveStyleResources(frameworkElement, oldValue as StyleSheet);
				return;
			}

			VisualTreeHelper.Include(frameworkElement);

			if (frameworkElement.Parent != null)
			{
				EnqueueRenderStyleSheet(frameworkElement, newStyleSheet, frameworkElement);
			}
			else
			{
				instance.dependencyPropertyService.RegisterLoadedOnce(frameworkElement, f => instance.UpdateElement(frameworkElement));
			}
		}

		private static void StylePropertyAttached(BindableObject d, object oldValue, object newValue)
		{
			var frameworkElement = d as Element;
			if (frameworkElement == null)
				return;
			instance.UpdateElement(frameworkElement);
		}
	}
}

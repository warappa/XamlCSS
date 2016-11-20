using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using XamlCSS.Dom;

namespace XamlCSS.WPF
{
	public class Css
	{
		public readonly static BaseCss<DependencyObject, DependencyObject, Style, DependencyProperty> instance =
			new BaseCss<DependencyObject, DependencyObject, Style, DependencyProperty>(
				new DependencyPropertyService(),
				new TreeNodeProvider(new DependencyPropertyService()),
				new StyleResourceService(),
				new StyleService(),
				DomElementBase<DependencyObject, DependencyProperty>.GetPrefix(typeof(System.Windows.Controls.Button)),
				new MarkupExtensionParser(),
				Application.Current.Dispatcher.Invoke
				);

		private static TimeSpan _lastRendering;

		static Css()
		{
			CompositionTarget.Rendering += (sender, e) =>
			{
				var evt = e as RenderingEventArgs;
				if (evt.RenderingTime == _lastRendering)
					return;

				_lastRendering = evt.RenderingTime;

				instance.ExecuteApplyStyles();
			};
		}
		
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
		private static void StyleSheetPropertyAttached(DependencyObject element, DependencyPropertyChangedEventArgs e)
		{
			var newStyleSheet = (StyleSheet)e.NewValue;

			if (newStyleSheet == null)
			{
				instance.RemoveStyleResources(element, (StyleSheet)e.OldValue);
				return;
			}

			if (instance.dependencyPropertyService.IsLoaded(element))
			{
				instance.EnqueueRenderStyleSheet(element, e.NewValue as StyleSheet, element);
			}
			else
			{
				instance.dependencyPropertyService.RegisterLoadedOnce(
					element,
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

		public static readonly DependencyProperty HandledCssProperty =
			DependencyProperty.RegisterAttached(
				"HandledCss",
				typeof(bool),
				typeof(Css),
				new PropertyMetadata(false));
		public static string GetHandledCss(DependencyObject obj)
		{
			return obj.GetValue(HandledCssProperty) as string;
		}
		public static void SetHandledCss(DependencyObject obj, string value)
		{
			obj.SetValue(HandledCssProperty, value);
		}

        public static readonly DependencyProperty DomElementProperty =
            DependencyProperty.RegisterAttached(
                "DomElement",
                typeof(IDomElement<DependencyObject>),
                typeof(Css),
                new PropertyMetadata(null));
        public static IDomElement<DependencyObject> GetDomElement(DependencyObject obj)
        {
            return obj.GetValue(DomElementProperty) as IDomElement<DependencyObject>;
        }
        public static void SetDomElement(DependencyObject obj, IDomElement<DependencyObject> value)
        {
            obj.SetValue(DomElementProperty, value);
        }
    }
}

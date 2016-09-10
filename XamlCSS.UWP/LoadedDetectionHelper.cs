using System;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XamlCSS.UWP
{
	public static class LoadedDetectionHelper
	{
		private static Type[] GetUITypesFromAssemblyByType(Type type)
		{
			return type.GetTypeInfo().Assembly
				.GetTypes()
				.Where(x =>
					x.GetTypeInfo().IsAbstract == false &&
					x.GetTypeInfo().IsInterface == false &&
					typeof(FrameworkElement).GetTypeInfo().IsAssignableFrom(x.GetTypeInfo())
				)
				.ToArray();
		}
		public static void Initialize()
		{
			var frame = Window.Current.Content as Frame;

			var uiTypes = GetUITypesFromAssemblyByType(frame.GetType())
				.Concat(GetUITypesFromAssemblyByType(typeof(Window)))
				.ToArray();

			var style = new Style(typeof(FrameworkElement));
			style.Setters.Add(new Setter(LoadDetectionProperty, true));

			foreach (var t in uiTypes)
			{
				frame.Resources.Add(t, style);
			}
		}

		#region LoadDetection

		public static readonly DependencyProperty LoadDetectionProperty =
			DependencyProperty.RegisterAttached("LoadDetection", typeof(bool), typeof(LoadedDetectionHelper),
												new PropertyMetadata(false, OnLoadDetectionChanged));

		public static bool GetLoadDetection(UIElement element)
		{
			var res = element.ReadLocalValue(LoadDetectionProperty);

			return res == DependencyProperty.UnsetValue ? false : (bool)res;
		}
		public static void SetLoadDetection(UIElement element, bool value)
		{
			element.SetValue(LoadDetectionProperty, value);
		}

		private static void OnLoadDetectionChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs ev)
		{
			var obj = dpo as FrameworkElement;
			if ((bool)ev.NewValue == true && 
				Css.GetIsLoaded(dpo) == false)
			{
				if (dpo is FrameworkElement)
				{
					Css.SetIsLoaded(obj, true);

					(dpo as FrameworkElement).Unloaded -= LoadedDetectionHelper_Unloaded;
					(dpo as FrameworkElement).Unloaded += LoadedDetectionHelper_Unloaded;
					Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
					{
						Css.instance.UpdateElement(obj);
					});
				}
			}
		}

		private static void LoadedDetectionHelper_Unloaded(object sender, RoutedEventArgs e)
		{
			(sender as FrameworkElement).Unloaded -= LoadedDetectionHelper_Unloaded;

			Css.instance.UnapplyMatchingStyles(sender as FrameworkElement);
		}

		#endregion
	}
}

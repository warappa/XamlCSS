using System.Windows;

namespace XamlCSS.WPF
{
	public class LoadedDetectionHelper
	{
		public static void Initialize()
		{
			EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.SizeChangedEvent, new RoutedEventHandler(OnSizeChanged));

			EventManager.RegisterClassHandler(typeof(UIElement), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnLoaded), true);
			EventManager.RegisterClassHandler(typeof(ContentElement), FrameworkContentElement.LoadedEvent, new RoutedEventHandler(OnLoaded), true);
		}

		private static void OnLoaded(object sender, RoutedEventArgs e)
		{
		}

		private static void UpdateStyle(FrameworkElement sender)
		{
			if (sender != null &&
				sender.TemplatedParent == null)
				Css.instance.UpdateElement(sender);
		}

		private static void OnSizeChanged(object sender, RoutedEventArgs e)
		{
			SetLoadDetection((Window)sender, true);
		}

		#region LoadDetection

		public static readonly DependencyProperty LoadDetectionProperty =
			DependencyProperty.RegisterAttached("LoadDetection", typeof(bool), typeof(LoadedDetectionHelper),
												new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, OnLoadDetectionChanged));

		public static bool GetLoadDetection(UIElement element)
		{
			return (bool)element.GetValue(LoadDetectionProperty);
		}
		public static void SetLoadDetection(UIElement element, bool value)
		{
			element.SetValue(LoadDetectionProperty, value);
		}

		private static void OnLoadDetectionChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs ev)
		{
			if ((bool)ev.NewValue == true)
			{
				if (dpo is FrameworkElement)
					(dpo as FrameworkElement).Loaded += LoadedEventHandler;
				else if (dpo is FrameworkContentElement)
					(dpo as FrameworkContentElement).Loaded += LoadedEventHandler;
			}
			else
			{
				if (dpo is FrameworkElement)
					(dpo as FrameworkElement).Loaded -= LoadedEventHandler;
				else if (dpo is FrameworkContentElement)
					(dpo as FrameworkContentElement).Loaded -= LoadedEventHandler;
			}
		}

		private static readonly RoutedEventHandler LoadedEventHandler = delegate (object sender, RoutedEventArgs e)
		{
			UpdateStyle(sender as FrameworkElement);
		};

		#endregion
	}
}

using System;
using System.Windows;

namespace XamlCSS.WPF
{
    public class LoadedDetectionHelper
    {
        public static event EventHandler SubTreeAdded;
        public static event EventHandler SubTreeRemoved;
        private static bool initialized = false;
        public static void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            EventManager.RegisterClassHandler(typeof(UIElement), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnLoaded), true);
            EventManager.RegisterClassHandler(typeof(ContentElement), FrameworkContentElement.LoadedEvent, new RoutedEventHandler(OnLoaded), true);
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetLoadDetection(sender as UIElement, true);
        }

        private static void UpdateStyle(DependencyObject sender)
        {

            if (sender != null &&
                ((sender as FrameworkElement)?.TemplatedParent == null) &&
                ((sender as FrameworkContentElement)?.TemplatedParent == null))
            {
                Css.instance.UpdateElement(sender);
            }
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
            if (element == null)
            {
                return;
            }

            element.SetValue(LoadDetectionProperty, value);
        }

        private static void OnLoadDetectionChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs ev)
        {
            if ((bool)ev.NewValue == true)
            {
                SubTreeAdded?.Invoke(dpo, new EventArgs());

                if (dpo is FrameworkElement)
                {
                    (dpo as FrameworkElement).Loaded += LoadedEventHandler;
                }
                else if (dpo is FrameworkContentElement)
                {
                    (dpo as FrameworkContentElement).Loaded += LoadedEventHandler;
                }
            }
            else
            {
                SubTreeRemoved?.Invoke(dpo, new EventArgs());

                if (dpo is FrameworkElement)
                {
                    (dpo as FrameworkElement).Loaded -= LoadedEventHandler;
                }
                else if (dpo is FrameworkContentElement)
                {
                    (dpo as FrameworkContentElement).Loaded -= LoadedEventHandler;
                }
            }
        }

        private static readonly RoutedEventHandler LoadedEventHandler = delegate (object sender, RoutedEventArgs e)
        {
            UpdateStyle(sender as DependencyObject);
        };

        #endregion
    }
}

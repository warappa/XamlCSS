using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XamlCSS.UWP
{
    public static class LoadedDetectionHelper
    {
        public static event EventHandler SubTreeAdded;
        public static event EventHandler SubTreeRemoved;
        private static IEnumerable<Type> GetUITypesFromAssemblyByType(Type type)
        {
            if (type == null)
            {
                return new Type[0];
            }
            return type.GetTypeInfo().Assembly
                .GetTypes()
                .Where(x =>
                    x.GetTypeInfo().IsAbstract == false &&
                    x.GetTypeInfo().IsInterface == false &&
                    typeof(FrameworkElement).GetTypeInfo().IsAssignableFrom(x.GetTypeInfo())
                )
                .ToList();
        }

        private static bool initialized = false;
        public static void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            FrameworkElement frame = Window.Current.Content as FrameworkElement;
            
            var uiTypes = GetUITypesFromAssemblyByType(frame.GetType())
                .Concat(GetUITypesFromAssemblyByType(typeof(Window)))
                .Distinct()
                .ToList();

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

                    SubTreeAdded?.Invoke(obj, new EventArgs());

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

            SubTreeRemoved?.Invoke(sender, new EventArgs());

            Css.instance.UnapplyMatchingStyles(sender as FrameworkElement);
        }

        #endregion
    }
}

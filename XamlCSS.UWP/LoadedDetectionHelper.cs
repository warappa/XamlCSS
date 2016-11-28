using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;

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
            
            var frame = Window.Current?.Content as FrameworkElement;
            if (frame == null)
            {
                return;
            }

            initialized = true;

            var uiTypes = GetUITypesFromAssemblyByType(frame.GetType())
                .Concat(GetUITypesFromAssemblyByType(typeof(Window)))
                .Distinct()
                .ToList();
            
            foreach (var t in uiTypes)
            {
                var style = new Style(t);
                style.Setters.Add(new Setter(LoadDetectionProperty, true));

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
            var element = dpo as FrameworkElement;
            if ((bool)ev.NewValue == true)
            {
                Debug.WriteLine("Added (OnLoadDetectionChanged)");

                element.Loaded += Obj_Loaded;
                element.Unloaded += LoadedDetectionHelper_Unloaded;
                SubTreeAdded?.Invoke(dpo, new EventArgs());

                Css.instance.UpdateElement(dpo);
            }

            // Load detection is only relyable for the first time
        }

        private static void ApplyStyle(FrameworkElement obj)
        {
            if (obj == null)
            {
                return;
            }

            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    Css.instance.UpdateElement(obj);
                });
            }
            else
            {
                Css.instance.UpdateElement(obj);
            }
        }

        private static void Obj_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Added (Obj_Loaded)");
            var element = sender as FrameworkElement;
            
            ApplyStyle(element);
        }

        private static void LoadedDetectionHelper_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Removed (LoadedDetectionHelper_Unloaded)");

            (sender as FrameworkElement).Unloaded -= LoadedDetectionHelper_Unloaded;

            Css.instance.UnapplyMatchingStyles(sender as FrameworkElement);

            SubTreeRemoved?.Invoke(sender, new EventArgs());
        }

        #endregion
    }
}

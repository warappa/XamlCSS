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

            initialized = true;

            var frame = Window.Current?.Content as FrameworkElement;
            if (frame == null)
            {
                return;
            }

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
            Debug.WriteLine("-----------------------");
            Debug.WriteLine("OnLoadDetectionChanged");

            var frameworkElement = dpo as FrameworkElement;

            Debug.Write("Element: " + frameworkElement.Name);
            
            if ((bool)ev.NewValue)
            {
                Debug.WriteLine("Added");

                SubTreeAdded?.Invoke(frameworkElement, new EventArgs());

                if (frameworkElement.Parent != null)
                {
                    ApplyStyle(frameworkElement);
                }

                frameworkElement.Loaded -= Obj_Loaded;
                frameworkElement.Loaded += Obj_Loaded;
                frameworkElement.Unloaded -= LoadedDetectionHelper_Unloaded;
                frameworkElement.Unloaded += LoadedDetectionHelper_Unloaded;
            }
            else
            {
                Debug.WriteLine("Removed");

                SubTreeRemoved?.Invoke(frameworkElement, new EventArgs());
                frameworkElement.Loaded -= Obj_Loaded;
                frameworkElement.Unloaded -= LoadedDetectionHelper_Unloaded;
            }
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
            var element = sender as DependencyObject;
            
            ApplyStyle(sender as FrameworkElement);
        }

        private static void LoadedDetectionHelper_Unloaded(object sender, RoutedEventArgs e)
        {
            (sender as FrameworkElement).Unloaded -= LoadedDetectionHelper_Unloaded;

            Css.instance.UnapplyMatchingStyles(sender as FrameworkElement);

            SubTreeRemoved?.Invoke(sender, new EventArgs());
        }

        #endregion
    }
}

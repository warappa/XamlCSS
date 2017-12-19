using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XamlCSS.UWP
{
    public static class LoadedDetectionHelper
    {
        public static event EventHandler SubTreeAdded;
        public static event EventHandler SubTreeRemoved;

        public static IEnumerable<Type> GetUITypesFromAssemblyByType(Type type)
        {
            if (type == null)
            {
                return new Type[0];
            }
            try
            {
                return type.GetTypeInfo().Assembly
                    .GetTypes()
                    .Where(x =>
                        x.GetTypeInfo().IsAbstract == false &&
                        x.GetTypeInfo().IsInterface == false &&
                        typeof(FrameworkElement).GetTypeInfo().IsAssignableFrom(x.GetTypeInfo())
                    )
                    .ToList();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return new List<Type>();
        }

        private static bool initialized = false;


        public static void Reset()
        {
            if (!initialized)
            {
                return;
            }

            initialized = false;
        }

        public static void Initialize()
        {
            if (initialized)
            {
                return;
            }

            try
            {
                var uiTypes = GetUITypesFromAssemblyByType(typeof(Window))
                    .Distinct()
                    .ToList();

                Application.Current.Resources = Application.Current.Resources ?? new ResourceDictionary();
                var style = new Style(typeof(FrameworkElement));
                style.Setters.Add(new Setter(LoadDetectionProperty, true));
                foreach (var t in uiTypes)
                {
                    Application.Current.Resources.Remove(t);
                    Application.Current.Resources[t] = style;
                }

                initialized = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n" + e.StackTrace);
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
            if ((dpo is TextBlock t) &&
                   t.Name == MarkupExtensionParser.MarkupParserHelperId)
            {
                return;
            }

            var element = dpo as FrameworkElement;
            if ((bool)ev.NewValue == true)
            {
                Debug.WriteLine("Added (OnLoadDetectionChanged)");

                element.Loaded += Obj_Loaded;
                element.Unloaded += LoadedDetectionHelper_Unloaded;
                SubTreeAdded?.Invoke(dpo, new EventArgs());

                Css.instance?.UpdateElement(dpo);
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
                    Css.instance?.UpdateElement(obj);
                });
            }
            else
            {
                var dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Interval = TimeSpan.FromMilliseconds(0);
                dispatcherTimer.Tick += (s, e) =>
                  {
                      (s as DispatcherTimer).Stop();
                      Css.instance?.UpdateElement(obj);
                  };
                dispatcherTimer.Start();
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

            // (sender as FrameworkElement).Unloaded -= LoadedDetectionHelper_Unloaded;

            Css.instance?.UnapplyMatchingStyles(sender as FrameworkElement, null);

            SubTreeRemoved?.Invoke(sender, new EventArgs());
        }

        #endregion
    }
}

using System;
using System.Windows;
using XamlCSS.WPF.Dom;

namespace XamlCSS.WPF
{
    public class LoadedDetectionHelper
    {
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
            SetLoadDetection((DependencyObject)sender, true);
        }

        #region LoadDetection

        public static readonly DependencyProperty LoadDetectionProperty =
            DependencyProperty.RegisterAttached("LoadDetection", typeof(bool), typeof(LoadedDetectionHelper),
                                                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, OnLoadDetectionChanged));

        public static bool GetLoadDetection(DependencyObject element)
        {
            return (bool)element.GetValue(LoadDetectionProperty);
        }
        public static void SetLoadDetection(DependencyObject element, bool value)
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
                if (dpo is FrameworkElement frameworkElement)
                {

                    frameworkElement.Loaded += LoadedEventHandler;
                    frameworkElement.Unloaded += UnloadedEventHandler;
                    //frameworkElement.Initialized += FrameworkElement_Initialized;
                    if (frameworkElement.IsLoaded)
                    {
                        LoadedEventHandler.Invoke(frameworkElement, new RoutedEventArgs());
                    }
                }
                else if (dpo is FrameworkContentElement frameworkContentElement)
                {
                    frameworkContentElement.Loaded += LoadedEventHandler;
                    frameworkContentElement.Unloaded += UnloadedEventHandler;
                    //frameworkContentElement.Initialized += FrameworkElement_Initialized;
                    if (frameworkContentElement.IsLoaded)
                    {
                        LoadedEventHandler.Invoke(frameworkContentElement, new RoutedEventArgs());
                    }
                }
            }
            else
            {
                if (dpo is FrameworkElement frameworkElement)
                {
                    frameworkElement.Unloaded -= UnloadedEventHandler;
                    frameworkElement.Loaded -= LoadedEventHandler;
                    frameworkElement.Initialized -= FrameworkElement_Initialized;
                }
                else if (dpo is FrameworkContentElement frameworkContentElement)
                {
                    frameworkContentElement.Unloaded -= UnloadedEventHandler;
                    frameworkContentElement.Loaded -= LoadedEventHandler;
                    frameworkContentElement.Initialized -= FrameworkElement_Initialized;
                }
            }
        }

        private static void FrameworkElement_Initialized(object sender, EventArgs e)
        {
            var dom = Css.instance?.treeNodeProvider.GetDomElement((DependencyObject)sender) as DomElement;
            dom?.UpdateIsReady();
        }

        private static readonly RoutedEventHandler UnloadedEventHandler = delegate (object sender, RoutedEventArgs e)
        {
            if (Css.instance == null)
            {
                return;
            }

            Css.instance.RemoveElement(sender as DependencyObject);
            var dom = Css.instance.treeNodeProvider.GetDomElement(sender as DependencyObject) as DomElement;
            dom?.UpdateIsReady();

            var logicalParent = dom.LogicalParent?.Element;
            var visualParent = dom.Parent?.Element;

            if (logicalParent != visualParent)
                Css.instance.UpdateElement(visualParent);
            Css.instance.UpdateElement(logicalParent);
        };

        private static readonly RoutedEventHandler LoadedEventHandler = delegate (object sender, RoutedEventArgs e)
        {
            if (Css.instance == null)
            {
                return;
            }

            var dom = Css.instance.treeNodeProvider.GetDomElement((DependencyObject)sender) as DomElement;
            dom.UpdateIsReady();

            Css.instance.NewElement(sender as DependencyObject);

            if (dom.ApplyStyleImmediately)
            {
                Css.instance?.ExecuteApplyStyles();
            }
        };

        #endregion
    }
}

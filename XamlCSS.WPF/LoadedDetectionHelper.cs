using System;
using System.Windows;
using System.Windows.Controls;

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
            //if (Css.instance?.dependencyPropertyService.GetInitialStyle((DependencyObject)sender) == null)
            //{
            //    Css.instance?.dependencyPropertyService.SetInitialStyle((DependencyObject)sender, Css.instance?.nativeStyleService.GetStyle((DependencyObject)sender));
            //}
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
            if ((dpo is TextBlock t) &&
                t.Name == MarkupExtensionParser.MarkupParserHelperId)
            {
                return;
            }

            if ((bool)ev.NewValue == true)
            {
                if (dpo is FrameworkElement frameworkElement)
                {
                    
                    frameworkElement.Loaded += LoadedEventHandler;
                    frameworkElement.Unloaded += UnloadedEventHandler;
                    frameworkElement.Initialized += FrameworkElement_Initialized;
                    if (frameworkElement.IsLoaded)
                    {
                        LoadedEventHandler.Invoke(frameworkElement, new RoutedEventArgs());
                    }
                }
                else if (dpo is FrameworkContentElement frameworkContentElement)
                {
                    frameworkContentElement.Loaded += LoadedEventHandler;
                    frameworkContentElement.Unloaded += UnloadedEventHandler;
                    frameworkContentElement.Initialized += FrameworkElement_Initialized;
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
            //SubTreeAdded?.Invoke(sender, e);
            //Css.instance?.NewElement(sender as DependencyObject);
        }

        private static readonly RoutedEventHandler UnloadedEventHandler = delegate (object sender, RoutedEventArgs e)
        {
            Css.instance?.RemoveElement(sender as DependencyObject);
            //Css.instance?.treeNodeProvider.Switch(SelectorType.LogicalTree);
            Css.instance?.UpdateElement(Css.instance?.treeNodeProvider.GetDomElement(sender as DependencyObject)?.LogicalParent?.Element);
            SubTreeRemoved?.Invoke(sender, e);
        };

        private static readonly RoutedEventHandler LoadedEventHandler = delegate (object sender, RoutedEventArgs e)
        {
            //Css.instance.treeNodeProvider.Switch(SelectorType.LogicalTree);
            Css.instance?.treeNodeProvider.GetDomElement((DependencyObject)sender);

            //Css.instance.treeNodeProvider.Switch(SelectorType.VisualTree);
            //Css.instance.treeNodeProvider.GetDomElement((DependencyObject)sender);

            SubTreeAdded?.Invoke(sender, e);
            Css.instance?.NewElement(sender as DependencyObject);
        };

        #endregion
    }
}

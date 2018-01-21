using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using XamlCSS.Dom;
using XamlCSS.Utils;
using XamlCSS.UWP.CssParsing;
using XamlCSS.UWP.Dom;

namespace XamlCSS.UWP
{
    public class Css
    {
        public static BaseCss<DependencyObject, Style, DependencyProperty> instance;

        public static void RunOnUIThread(Action action)
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => action());
            }
            else
            {
                var localTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(0)
                };

                EventHandler<object> handler = null;
                handler = (timer, e) =>
                {
                    localTimer.Tick -= handler;
                    (timer as DispatcherTimer).Stop();
                    action();
                };

                localTimer.Tick += handler;
                localTimer.Start();
            }
        }

        private static bool initialized = false;

        static Css()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Initialize(new[] { Application.Current.GetType().GetTypeInfo().Assembly });
            }
        }

        public static void Reset()
        {
            if (!initialized)
            {
                return;
            }

            CompositionTarget.Rendering -= RenderingHandler;

            LoadedDetectionHelper.Reset();

            instance = null;

            initialized = false;
        }

        public static void Initialize(IEnumerable<Assembly> resourceSearchAssemblies)
        {
            if (initialized)
            {
                return;
            }

            var mapping = new Dictionary<string, List<string>>
            {
                {
                    "http://schemas.microsoft.com/winfx/2006/xaml/presentation",
                    new List<string>
                    {
                        typeof(Windows.UI.Xaml.Data.Binding).AssemblyQualifiedName.Replace(".Binding,", ","),
                        typeof(Windows.UI.Xaml.Shapes.Rectangle).AssemblyQualifiedName.Replace(".Rectangle,", ","),
                        typeof(Windows.UI.Xaml.Controls.Button).AssemblyQualifiedName.Replace(".Button,", ","),
                        typeof(Windows.UI.Xaml.FrameworkElement).AssemblyQualifiedName.Replace(".FrameworkElement,", ","),
                        typeof(Windows.UI.Xaml.Documents.Run).AssemblyQualifiedName.Replace(".Run,", ",")
                    }
                }
            };

            TypeHelpers.Initialze(mapping, false);

            var dependencyPropertyService = new DependencyPropertyService();
            var markupExtensionParser = new MarkupExtensionParser();
            var cssTypeHelper = new CssTypeHelper<DependencyObject, DependencyProperty, Style>(markupExtensionParser, dependencyPropertyService);

            instance = new BaseCss<DependencyObject, Style, DependencyProperty>(
                dependencyPropertyService,
                new LogicalTreeNodeProvider(dependencyPropertyService),
                new StyleResourceService(),
                new StyleService(dependencyPropertyService),
                DomElementBase<DependencyObject, DependencyProperty>.GetAssemblyQualifiedNamespaceName(typeof(Button)),
                markupExtensionParser,
                RunOnUIThread,
                new CssFileProvider(resourceSearchAssemblies, cssTypeHelper)
                );

            LoadedDetectionHelper.Initialize();

            CompositionTarget.Rendering += RenderingHandler;

            initialized = true;
        }

        private static void RenderingHandler(object sender, object e)
        {
            instance?.ExecuteApplyStyles();
        }

        #region dependency properties
        
        public static readonly DependencyProperty InitialStyleProperty =
            DependencyProperty.RegisterAttached("InitialStyle", typeof(Style),
            typeof(Css), new PropertyMetadata(null));
        public static Style GetInitialStyle(DependencyObject obj)
        {
            return obj.ReadLocalValue(InitialStyleProperty) as Style;
        }
        public static void SetInitialStyle(DependencyObject obj, Style value)
        {
            obj.SetValue(InitialStyleProperty, value ?? DependencyProperty.UnsetValue);
        }
        
        public static readonly DependencyProperty StyleProperty =
            DependencyProperty.RegisterAttached("Style", typeof(StyleDeclarationBlock),
            typeof(Css), new PropertyMetadata(null, StylePropertyAttached));
        public static StyleDeclarationBlock GetStyle(DependencyObject obj)
        {
            return obj.ReadLocalValue(StyleProperty) as StyleDeclarationBlock;
        }
        public static void SetStyle(DependencyObject obj, StyleDeclarationBlock value)
        {
            obj.SetValue(StyleProperty, value ?? DependencyProperty.UnsetValue);
        }

        public static readonly DependencyProperty StyleSheetProperty =
            DependencyProperty.RegisterAttached("StyleSheet", typeof(StyleSheet),
            typeof(Css), new PropertyMetadata(null, StyleSheetPropertyChanged));
        public static StyleSheet GetStyleSheet(DependencyObject obj)
        {
            var read = obj.ReadLocalValue(StyleSheetProperty);
            if (read is BindingExpression)
                read = obj.GetValue(StyleSheetProperty);
            return read as StyleSheet;
        }
        public static void SetStyleSheet(DependencyObject obj, StyleSheet value)
        {
            obj.SetValue(StyleSheetProperty, value ?? DependencyProperty.UnsetValue);
        }

        public static readonly DependencyProperty ClassProperty =
            DependencyProperty.RegisterAttached("Class", typeof(string),
            typeof(Css), new PropertyMetadata(null, ClassPropertyAttached));
        private static void ClassPropertyAttached(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            var domElement = GetDomElement(element) as DomElementBase<DependencyObject, DependencyProperty>;
            domElement?.ResetClassList();

            Css.instance?.UpdateElement(element);
        }

        public static string GetClass(DependencyObject obj)
        {
            return obj.ReadLocalValue(ClassProperty) as string;
        }
        public static void SetClass(DependencyObject obj, string value)
        {
            obj.SetValue(ClassProperty, value ?? DependencyProperty.UnsetValue);
        }
        
        public static readonly DependencyProperty DomElementProperty =
            DependencyProperty.RegisterAttached("DomElement", typeof(bool),
            typeof(Css), new PropertyMetadata(null, null));

        public static IDomElement<DependencyObject> GetDomElement(DependencyObject obj)
        {
            var res = obj.ReadLocalValue(DomElementProperty);
            if (res == DependencyProperty.UnsetValue)
                return null;
            return res as IDomElement<DependencyObject>;
        }
        public static void SetDomElement(DependencyObject obj, IDomElement<DependencyObject> value)
        {
            obj.SetValue(DomElementProperty, value ?? DependencyProperty.UnsetValue);
        }

        public static readonly DependencyProperty VisualDomElementProperty =
            DependencyProperty.RegisterAttached("VisualDomElement", typeof(bool),
            typeof(Css), new PropertyMetadata(null, null));

        public static IDomElement<DependencyObject> GetVisualDomElement(DependencyObject obj)
        {
            var res = obj.ReadLocalValue(VisualDomElementProperty);
            if (res == DependencyProperty.UnsetValue)
                return null;
            return res as IDomElement<DependencyObject>;
        }
        public static void SetVisualDomElement(DependencyObject obj, IDomElement<DependencyObject> value)
        {
            obj.SetValue(VisualDomElementProperty, value ?? DependencyProperty.UnsetValue);
        }
        
        #endregion

        #region attached behaviours

        private static void StyleSheetPropertyChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                var oldStyleSheet = e.OldValue as StyleSheet;
                oldStyleSheet.PropertyChanged -= NewStyleSheet_PropertyChanged;

                instance?.EnqueueRemoveStyleSheet(element, oldStyleSheet);
            }

            var newStyleSheet = (StyleSheet)e.NewValue;

            if (newStyleSheet == null)
            {
                return;
            }

            newStyleSheet.PropertyChanged += NewStyleSheet_PropertyChanged;

            instance?.EnqueueRenderStyleSheet(element, newStyleSheet);
        }

        private static void NewStyleSheet_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StyleSheet.Content))
            {
                var styleSheet = sender as StyleSheet;
                var attachedTo = styleSheet.AttachedTo as FrameworkElement;

                instance?.EnqueueUpdateStyleSheet(attachedTo, styleSheet);
            }
        }

        private static void StylePropertyAttached(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            instance?.UpdateElement(d as FrameworkElement);
        }

        #endregion
    }
}

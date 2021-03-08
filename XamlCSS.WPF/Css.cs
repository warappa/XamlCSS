using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using XamlCSS.Dom;
using XamlCSS.Utils;
using XamlCSS.WPF.CssParsing;
using XamlCSS.WPF.Dom;

namespace XamlCSS.WPF
{
    public class Css
    {
        public static BaseCss<DependencyObject, Style, DependencyProperty> instance;
        //private static DispatcherTimer timer;
        private static System.Timers.Timer timer;

        public static readonly IDictionary<string, List<string>> DefaultCssNamespaceMapping = new Dictionary<string, List<string>>
        {
            {
                "http://schemas.microsoft.com/winfx/2006/xaml/presentation",
                new List<string>
                {
                    typeof(System.Windows.Data.Binding).AssemblyQualifiedName.Replace(".Binding,", ","),
                    typeof(System.Windows.Navigation.NavigationWindow).AssemblyQualifiedName.Replace(".NavigationWindow,", ","),
                    typeof(System.Windows.Shapes.Rectangle).AssemblyQualifiedName.Replace(".Rectangle,", ","),
                    typeof(System.Windows.Controls.Button).AssemblyQualifiedName.Replace(".Button,", ","),
                    typeof(System.Windows.FrameworkElement).AssemblyQualifiedName.Replace(".FrameworkElement,", ","),
                    typeof(System.Windows.Documents.Run).AssemblyQualifiedName.Replace(".Run,", ","),
                    typeof(System.Windows.Controls.Primitives.ScrollBar).AssemblyQualifiedName.Replace(".ScrollBar,", ","),
                    typeof(System.Windows.Media.TextOptions).AssemblyQualifiedName.Replace(".TextOptions,", ",")
                }
            }
        };

        static Css()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Initialize();
            }
        }

        public static void Initialize(IDictionary<string, List<string>> cssNamespaceMapping = null)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            cssNamespaceMapping = cssNamespaceMapping ?? DefaultCssNamespaceMapping;

            TypeHelpers.Initialze(cssNamespaceMapping);

            var defaultCssNamespace = cssNamespaceMapping.Keys.First();
            var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            var dependencyPropertyService = new DependencyPropertyService();
            var visualTreeNodeWithLogicalFallbackProvider = new TreeNodeProvider(dependencyPropertyService);
            var markupExtensionParser = new MarkupExtensionParser();
            var cssTypeHelper = new CssTypeHelper<DependencyObject, DependencyProperty, Style>(markupExtensionParser, dependencyPropertyService);
            
            instance = new BaseCss<DependencyObject, Style, DependencyProperty>(
                dependencyPropertyService,
                visualTreeNodeWithLogicalFallbackProvider,
                new StyleResourceService(),
                new StyleService(new DependencyPropertyService(), new MarkupExtensionParser()),
                defaultCssNamespace,
                markupExtensionParser,
                dispatcher.Invoke,
                new CssFileProvider(cssTypeHelper)
                );


            // add CompositionTarget.Rendering handler for startup
            CompositionTarget.Rendering += RenderingHandler;

            timer = new System.Timers.Timer(16);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            // Warmup(markupExtensionParser, defaultCssNamespace);
            //Warm();

            LoadedDetectionHelper.Initialize();
        }

        private static void RenderingHandler(object sender, EventArgs e)
        {
            if (instance?.ExecuteApplyStyles() == true)
            {
                // after the first successful applying remove the handler.
                CompositionTarget.Rendering -= RenderingHandler;
            }
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                instance?.ExecuteApplyStyles();
            }
            catch (Exception exc)
            {
                
            }
        }

        public static readonly DependencyProperty InitialStyleProperty =
            DependencyProperty.RegisterAttached(
                "InitialStyle",
                typeof(Style),
                typeof(Css),
                new PropertyMetadata(null));
        public static Style GetInitialStyle(DependencyObject obj)
        {
            return ReadSafe<Style>(obj, InitialStyleProperty);
        }
        public static void SetInitialStyle(DependencyObject obj, Style value)
        {
            obj.SetValue(InitialStyleProperty, value);
        }

        public static readonly DependencyProperty StyleProperty =
            DependencyProperty.RegisterAttached(
                "Style",
                typeof(StyleDeclarationBlock),
                typeof(Css),
                new PropertyMetadata(null, Css.StylePropertyAttached));
        private static void StylePropertyAttached(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            if (instance is null)
            {
                return;
            }

            if (instance.treeNodeProvider.TryGetDomElement(element, out var domElement) != true)
            {
                return; // doesn't exist yet, no update necessary
            }

            if (domElement.IsReady == true)
            {
                instance.UpdateElement(element);
            }
        }
        public static StyleDeclarationBlock GetStyle(DependencyObject obj)
        {
            return ReadSafe<StyleDeclarationBlock>(obj, StyleProperty);
        }
        public static void SetStyle(DependencyObject obj, StyleDeclarationBlock value)
        {
            obj.SetValue(StyleProperty, value);
        }

        public static readonly DependencyProperty StyleSheetProperty =
            DependencyProperty.RegisterAttached(
                "StyleSheet",
                typeof(StyleSheet),
                typeof(Css),
            new PropertyMetadata(null, Css.StyleSheetPropertyChanged));
        public static StyleSheet GetStyleSheet(DependencyObject obj)
        {
            return ReadSafe<StyleSheet>(obj, StyleSheetProperty);
        }
        public static void SetStyleSheet(DependencyObject obj, StyleSheet value)
        {
            obj.SetValue(StyleSheetProperty, value);
        }
        private static void StyleSheetPropertyChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue)
            {
                return;
            }

            if (e.OldValue is StyleSheet oldStyleSheet)
            {
                oldStyleSheet.PropertyChanged -= StyleSheet_PropertyChanged;

                if (oldStyleSheet.AttachedTo != null)
                {
                    instance?.EnqueueRemoveStyleSheet(element, (StyleSheet)e.OldValue);
                }
            }

            var newStyleSheet = (StyleSheet)e.NewValue;

            if (newStyleSheet == null)
            {
                return;
            }

            newStyleSheet.PropertyChanged += StyleSheet_PropertyChanged;
            // newStyleSheet.AttachedTo = element;

            instance?.EnqueueRenderStyleSheet(element, e.NewValue as StyleSheet);
        }

        private static void StyleSheet_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StyleSheet.Content))
            {
                var styleSheet = sender as StyleSheet;
                var attachedTo = styleSheet.AttachedTo as FrameworkElement;

                instance?.EnqueueUpdateStyleSheet(attachedTo, styleSheet);
            }
        }

        public static readonly DependencyProperty ClassProperty =
            DependencyProperty.RegisterAttached(
                "Class",
                typeof(string),
                typeof(Css),
                new PropertyMetadata(null, ClassPropertyAttached));
        public static string GetClass(DependencyObject obj)
        {
            return ReadSafe<string>(obj, ClassProperty);
        }
        public static void SetClass(DependencyObject obj, string value)
        {
            obj.SetValue(ClassProperty, value);
        }

        private static void ClassPropertyAttached(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            if (instance is null)
            {
                return;
            }

            if (instance.treeNodeProvider.TryGetDomElement(element, out var domElementBase) != true)
            {
                return;
            }

            var domElement = (DomElementBase<DependencyObject, DependencyProperty>)domElementBase;

            domElement.ResetClassList();
            if (domElement.IsReady == true)
            {
                instance.UpdateElement(element);
            }
        }

        public static readonly DependencyProperty DomElementProperty =
            DependencyProperty.RegisterAttached(
                "DomElement",
                typeof(IDomElement<DependencyObject, DependencyProperty>),
                typeof(Css),
                new PropertyMetadata(null));
        private static bool initialized;

        public static IDomElement<DependencyObject, DependencyProperty> GetDomElement(DependencyObject obj)
        {
            return ReadSafe<IDomElement<DependencyObject, DependencyProperty>>(obj, DomElementProperty);
        }
        public static void SetDomElement(DependencyObject obj, IDomElement<DependencyObject, DependencyProperty> value)
        {
            obj.SetValue(DomElementProperty, value);
        }

        public static readonly DependencyProperty ApplyStyleImmediatelyProperty =
            DependencyProperty.RegisterAttached(
                "ApplyStyleImmediately",
                typeof(bool),
                typeof(Css),
                new PropertyMetadata(false));
        public static bool GetApplyStyleImmediately(DependencyObject obj)
        {
            return (bool)obj?.GetValue(ApplyStyleImmediatelyProperty);
        }
        public static void SetApplyStyleImmediately(DependencyObject obj, bool value)
        {
            obj?.SetValue(ApplyStyleImmediatelyProperty, value);
        }

        private static T ReadSafe<T>(DependencyObject obj, DependencyProperty property)
        {
            var val = obj.GetValue(property);
            if (val == DependencyProperty.UnsetValue)
            {
                return default(T);
            }

            return (T)val;
        }
    }
}

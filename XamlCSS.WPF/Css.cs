using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using XamlCSS.CssParsing;
using XamlCSS.Dom;
using XamlCSS.Utils;
using XamlCSS.WPF.CssParsing;
using XamlCSS.WPF.Dom;

namespace XamlCSS.WPF
{
    public class Css
    {
        public static BaseCss<DependencyObject, Style, DependencyProperty> instance;

        private static EventHandler RenderingHandler()
        {
            return (sender, e) =>
            {
                instance?.ExecuteApplyStyles();
            };
        }

        static Css()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Initialize();
            }
        }

        public static void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            CompositionTarget.Rendering += RenderingHandler();

            var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            IDependencyPropertyService<DependencyObject, Style, DependencyProperty> dependencyPropertyService =
                new DependencyPropertyService();
            var visualTreeNodeProvider = new VisualTreeNodeProvider(dependencyPropertyService);
            var logicalTreeNodeProvider = new LogicalTreeNodeProvider(dependencyPropertyService);
            var visualTreeNodeWithLogicalFallbackProvider = new VisualWithLogicalFallbackTreeNodeProvider(dependencyPropertyService, visualTreeNodeProvider, logicalTreeNodeProvider);
            var markupExtensionParser = new MarkupExtensionParser();
            var cssTypeHelper = new CssTypeHelper<DependencyObject, DependencyProperty, Style>(markupExtensionParser, dependencyPropertyService);
            var switchableTreeNodeProvider = new SwitchableTreeNodeProvider(dependencyPropertyService, visualTreeNodeWithLogicalFallbackProvider, logicalTreeNodeProvider);
            var defaultCssNamespace = DomElementBase<DependencyObject, DependencyProperty>.GetNamespaceUri(typeof(System.Windows.Controls.Button));

            instance = new BaseCss<DependencyObject, Style, DependencyProperty>(
                dependencyPropertyService,
                switchableTreeNodeProvider,
                new StyleResourceService(),
                new StyleService(new DependencyPropertyService(), new MarkupExtensionParser()),
                defaultCssNamespace,
                markupExtensionParser,
                dispatcher.Invoke,
                new CssFileProvider(cssTypeHelper)
                );

            // Warmup(markupExtensionParser, defaultCssNamespace);

            LoadedDetectionHelper.Initialize();
        }

        private static void Warmup(MarkupExtensionParser markupExtensionParser, string defaultCssNamespace)
        {
            // warmup parser
            markupExtensionParser.Parse("true", Application.Current?.MainWindow ?? new FrameworkElement(), new[] { new CssNamespace("", defaultCssNamespace) });

            TypeHelpers.GetPropertyAccessor(typeof(FrameworkElement), "IsLoaded");

            var styleSheet = CssParser.Parse("*{Background: #StaticResource no;}");

            var f = new Button();
            f.SetValue(Css.StyleSheetProperty, styleSheet);
            f.Content = new TextBlock();

            instance.EnqueueNewElement(f, styleSheet, f);
            instance.ExecuteApplyStyles();
        }

        public static readonly DependencyProperty MatchingStylesProperty =
            DependencyProperty.RegisterAttached(
                "MatchingStyles",
                typeof(string[]),
                typeof(Css),
                new PropertyMetadata(null));
        public static string[] GetMatchingStyles(DependencyObject obj)
        {
            return ReadSafe<string[]>(obj, MatchingStylesProperty);
        }
        public static void SetMatchingStyles(DependencyObject obj, string[] value)
        {
            obj.SetValue(MatchingStylesProperty, value);
        }

        public static readonly DependencyProperty AppliedMatchingStylesProperty =
            DependencyProperty.RegisterAttached(
                "AppliedMatchingStyles",
                typeof(string[]),
                typeof(Css),
                new PropertyMetadata(null));
        public static string[] GetAppliedMatchingStyles(DependencyObject obj)
        {
            return ReadSafe<string[]>(obj, AppliedMatchingStylesProperty);
        }
        public static void SetAppliedMatchingStyles(DependencyObject obj, string[] value)
        {
            obj.SetValue(AppliedMatchingStylesProperty, value);
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

        public static readonly DependencyProperty HadStyleProperty =
            DependencyProperty.RegisterAttached(
                "HadStyle",
                typeof(bool?),
                typeof(Css),
                new PropertyMetadata(null));
        public static bool? GetHadStyle(DependencyObject obj)
        {
            return ReadSafe<bool?>(obj, HadStyleProperty);
        }
        public static void SetHadStyle(DependencyObject obj, bool? value)
        {
            obj.SetValue(HadStyleProperty, value);
        }

        public static readonly DependencyProperty StyledByStyleSheetProperty =
            DependencyProperty.RegisterAttached(
                "StyledByStyleSheet",
                typeof(StyleSheet),
                typeof(Css),
                new PropertyMetadata(null));
        public static StyleSheet GetStyledByStyleSheet(DependencyObject obj)
        {
            return ReadSafe<StyleSheet>(obj, StyledByStyleSheetProperty);
        }
        public static void SetStyledByStyleSheet(DependencyObject obj, StyleSheet value)
        {
            obj.SetValue(StyledByStyleSheetProperty, value);
        }

        public static readonly DependencyProperty StyleProperty =
            DependencyProperty.RegisterAttached(
                "Style",
                typeof(StyleDeclarationBlock),
                typeof(Css),
                new PropertyMetadata(null, Css.StylePropertyAttached));
        private static void StylePropertyAttached(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            instance?.UpdateElement(d);
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
            if (e.OldValue != null)
            {
                (e.OldValue as StyleSheet).PropertyChanged -= StyleSheet_PropertyChanged;

                instance?.EnqueueRemoveStyleSheet(element, (StyleSheet)e.OldValue);
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
            var domElement = GetDomElement(element) as DomElementBase<DependencyObject, DependencyProperty>;
            domElement?.ResetClassList();

            Css.instance?.UpdateElement(element);
        }

        public static readonly DependencyProperty HandledCssProperty =
            DependencyProperty.RegisterAttached(
                "HandledCss",
                typeof(bool),
                typeof(Css),
                new PropertyMetadata(false));
        public static bool GetHandledCss(DependencyObject obj)
        {
            return ReadSafe<bool>(obj, HandledCssProperty);
        }
        public static void SetHandledCss(DependencyObject obj, bool value)
        {
            obj.SetValue(HandledCssProperty, value);
        }

        public static readonly DependencyProperty DomElementProperty =
            DependencyProperty.RegisterAttached(
                "DomElement",
                typeof(IDomElement<DependencyObject>),
                typeof(Css),
                new PropertyMetadata(null));
        private static bool initialized;

        public static IDomElement<DependencyObject> GetDomElement(DependencyObject obj)
        {
            return ReadSafe<IDomElement<DependencyObject>>(obj, DomElementProperty);
        }
        public static void SetDomElement(DependencyObject obj, IDomElement<DependencyObject> value)
        {
            obj.SetValue(DomElementProperty, value);
        }

        public static readonly DependencyProperty VisualDomElementProperty =
            DependencyProperty.RegisterAttached(
                "VisualDomElement",
                typeof(IDomElement<DependencyObject>),
                typeof(Css),
                new PropertyMetadata(null));

        public static IDomElement<DependencyObject> GetVisualDomElement(DependencyObject obj)
        {
            return ReadSafe<IDomElement<DependencyObject>>(obj, VisualDomElementProperty);
        }
        public static void SetVisualDomElement(DependencyObject obj, IDomElement<DependencyObject> value)
        {
            obj.SetValue(VisualDomElementProperty, value);
        }

        private static T ReadSafe<T>(DependencyObject obj, DependencyProperty property)
        {
            var val = obj.ReadLocalValue(property);
            if (val == DependencyProperty.UnsetValue)
            {
                return default(T);
            }

            return (T)val;
        }

    }
}

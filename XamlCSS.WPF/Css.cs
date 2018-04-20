using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
            IDependencyPropertyService<DependencyObject, Style, DependencyProperty> dependencyPropertyService =
                new DependencyPropertyService();
            //var visualTreeNodeProvider = new VisualTreeNodeProvider(dependencyPropertyService);
            //var logicalTreeNodeProvider = new LogicalTreeNodeProvider(dependencyPropertyService);
            var visualTreeNodeWithLogicalFallbackProvider = new TreeNodeProvider(dependencyPropertyService);
            var markupExtensionParser = new MarkupExtensionParser();
            var cssTypeHelper = new CssTypeHelper<DependencyObject, DependencyProperty, Style>(markupExtensionParser, dependencyPropertyService);
            //var switchableTreeNodeProvider = new SwitchableTreeNodeProvider(dependencyPropertyService, visualTreeNodeWithLogicalFallbackProvider, logicalTreeNodeProvider);

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

            CompositionTarget.Rendering += RenderingHandler();

            // Warmup(markupExtensionParser, defaultCssNamespace);
            Warm();

            LoadedDetectionHelper.Initialize();
        }

        private static void Warm()
        {
            XamlWriter.Save(new Storyboard());
            XamlWriter.Save(new DoubleAnimation());
            //XamlWriter.Save(new Trigger());
            XamlWriter.Save(new TriggerAction());
            XamlWriter.Save(new DataTrigger());
            XamlWriter.Save(new EventTrigger());

            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic))
            {
                Console.WriteLine($"{ass.FullName}");

                foreach (var m in ass.GetExportedTypes().SelectMany(y =>
                    y.GetMethods(BindingFlags.DeclaredOnly
                        | BindingFlags.NonPublic
                        | BindingFlags.Public
                        | BindingFlags.Instance
                        | BindingFlags.Static)
                    .Where(x => !x.ContainsGenericParameters && !x.IsAbstract && !x.Attributes.HasFlag(MethodAttributes.PinvokeImpl))
                    .ToList()))
                {
                    try
                    {
                        System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(m.MethodHandle);
                    }
                    catch { }
                }

                foreach (var m in ass.GetExportedTypes()
                    .Where(x => !x.ContainsGenericParameters && !x.IsAbstract && !x.IsGenericTypeDefinition)
                    .SelectMany(y =>
                    y.GetProperties()
                    .Where(x => !x.DeclaringType.ContainsGenericParameters && !x.DeclaringType.IsAbstract && !x.DeclaringType.IsGenericTypeDefinition)
                    .ToList()))
                {
                    if (m.SetMethod != null &&
                        !m.SetMethod.ContainsGenericParameters &&
                        !m.SetMethod.IsAbstract)
                    {
                        try
                        {
                            System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(m.SetMethod.MethodHandle);
                        }
                        catch
                        {

                        }
                    }
                    if (m.GetMethod != null &&
                        !m.GetMethod.ContainsGenericParameters &&
                        !m.GetMethod.IsAbstract &&
                        !m.GetMethod.Attributes.HasFlag(MethodAttributes.PinvokeImpl))
                    {
                        try
                        {
                            System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(m.GetMethod.MethodHandle);
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        private static void Warmup(MarkupExtensionParser markupExtensionParser, string defaultCssNamespace)
        {
            // warmup parser
            markupExtensionParser.Parse("true", Application.Current?.MainWindow ?? new FrameworkElement(), new[] { new CssNamespace("", defaultCssNamespace) });

            TypeHelpers.GetPropertyAccessor(typeof(FrameworkElement), "IsLoaded");

            var styleSheet = CssParser.Parse("*{Background: #DynamicResource no;}");

            var f = new Button();
            f.SetValue(Css.StyleSheetProperty, styleSheet);
            f.Content = new TextBlock();

            instance.EnqueueNewElement(f, styleSheet, f);
            instance.ExecuteApplyStyles();
        }

        public static readonly DependencyProperty SerializedTriggerProperty =
            DependencyProperty.RegisterAttached(
                "SerializedTrigger",
                typeof(string),
                typeof(Css),
                new PropertyMetadata(null));
        public static string GetSerializedTrigger(DependencyObject obj)
        {
            return ReadSafe<string>(obj, SerializedTriggerProperty);
        }
        public static void SetSerializedTrigger(DependencyObject obj, string value)
        {
            obj.SetValue(SerializedTriggerProperty, value);
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
            if (e.OldValue == e.NewValue)
            {
                return;
            }

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
            //var domElement = instance?.treeNodeProvider.GetDomElement(element) as DomElementBase<DependencyObject, DependencyProperty>;
            var domElement = GetDomElement(element) as DomElementBase<DependencyObject, DependencyProperty>;
            var read = GetClass(element);
            domElement?.ResetClassList();

            instance?.UpdateElement(element);
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

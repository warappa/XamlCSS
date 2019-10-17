﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamlCSS.Dom;
using XamlCSS.Utils;
using XamlCSS.Windows.Media;
using XamlCSS.XamarinForms.CssParsing;
using XamlCSS.XamarinForms.Dom;
using XamlCSS.XamarinForms.Internals;

namespace XamlCSS.XamarinForms
{
    public class Css
    {
        public static BaseCss<BindableObject, Style, BindableProperty> instance;
        public static IDictionary<string, List<string>> DefaultCssNamespaceMapping = new Dictionary<string, List<string>>
        {
            {
                "http://xamarin.com/schemas/2014/forms",
                new List<string>
                {
                    typeof(Xamarin.Forms.Button).AssemblyQualifiedName.Replace(".Button,", ","),
                }
            }
        };

        private static Timer uiTimer;

        private static Element rootElement;
        private static object lockObject = new object();
        private static bool initialized = false;

        private static void StartUiTimer()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(5), () =>
            {
                instance?.ExecuteApplyStyles();
                return initialized;
            });
        }

        public static void Reset()
        {
            lock (lockObject)
            {
                if (initialized == false)
                {
                    return;
                }

                //VisualTreeHelper.SubTreeAdded -= VisualTreeHelper_ChildAdded;
                //VisualTreeHelper.SubTreeRemoved -= VisualTreeHelper_ChildRemoved;

                //VisualTreeHelper.Reset();
                LoadedDetectionHelper.Reset();

                uiTimer?.Cancel();
                uiTimer?.Dispose();
                uiTimer = null;

                instance = null;

                initialized = false;
            }
        }

        public static void Initialize(Element rootElement, Assembly[] resourceSearchAssemblies = null, IDictionary<string, List<string>> cssNamespaceMapping = null)
        {
            lock (lockObject)
            {
                if (initialized &&
                    rootElement == Css.rootElement)
                {
                    return;
                }

                Reset();

                initialized = true;

                cssNamespaceMapping = cssNamespaceMapping ?? DefaultCssNamespaceMapping;

                TypeHelpers.Initialize(cssNamespaceMapping, true);

                var defaultCssNamespace = cssNamespaceMapping.Keys.First();
                var markupExtensionParser = new MarkupExtensionParser();
                var dependencyPropertyService = new DependencyPropertyService();
                var cssTypeHelper = new CssTypeHelper<BindableObject, BindableProperty, Style>(markupExtensionParser, dependencyPropertyService);

                instance = new BaseCss<BindableObject, Style, BindableProperty>(
                    dependencyPropertyService,
                    new TreeNodeProvider(dependencyPropertyService),
                    new StyleResourceService(),
                    new StyleService(dependencyPropertyService, markupExtensionParser),
                    defaultCssNamespace,
                    markupExtensionParser,
                    Device.BeginInvokeOnMainThread,
                    new CssFileProvider(resourceSearchAssemblies ?? new Assembly[0], cssTypeHelper)
                    );

                Css.rootElement = rootElement;

                LoadedDetectionHelper.Initialize(rootElement);

                if (rootElement is Application)
                {
                    var application = rootElement as Application;

                    if (application.MainPage == null)
                    {
                        PropertyChangedEventHandler handler = null;
                        handler = (s, e) =>
                        {
                            if (e.PropertyName == nameof(Application.MainPage) &&
                                application.MainPage != null)
                            {
                                application.PropertyChanged -= handler;
                                instance?.ExecuteApplyStyles();
                            }
                        };

                        application.PropertyChanged += handler;
                    }
                }

                StartUiTimer();
            }
        }

        public static readonly BindableProperty IdProperty =
            BindableProperty.CreateAttached(
                "Id",
                typeof(string),
                typeof(Css),
                null,
                BindingMode.TwoWay);
        public static string GetId(BindableObject obj)
        {
            return obj.GetValue(IdProperty) as string;
        }
        public static void SetId(BindableObject obj, string value)
        {
            obj.SetValue(IdProperty, value);
        }

        public static readonly BindableProperty InitialStyleProperty =
            BindableProperty.CreateAttached(
                "InitialStyle",
                typeof(Style),
                typeof(Css),
                null,
                BindingMode.TwoWay);
        public static Style GetInitialStyle(BindableObject obj)
        {
            return obj.GetValue(InitialStyleProperty) as Style;
        }
        public static void SetInitialStyle(BindableObject obj, Style value)
        {
            obj.SetValue(InitialStyleProperty, value);
        }

        public static readonly BindableProperty StyleProperty =
            BindableProperty.CreateAttached(
                "Style",
                typeof(StyleDeclarationBlock),
                typeof(Css),
                null,
                BindingMode.TwoWay,
                null,
                Css.StylePropertyAttached);
        public static StyleDeclarationBlock GetStyle(BindableObject obj)
        {
            return obj.GetValue(StyleProperty) as StyleDeclarationBlock;
        }
        public static void SetStyle(BindableObject obj, StyleDeclarationBlock value)
        {
            obj.SetValue(StyleProperty, value);
        }

        public static readonly BindableProperty StyleSheetProperty =
            BindableProperty.CreateAttached(
                "StyleSheet",
                typeof(StyleSheet),
                typeof(Css),
                null,
                BindingMode.TwoWay,
                null,
                Css.StyleSheetPropertyChanged
                );
        public static StyleSheet GetStyleSheet(BindableObject obj)
        {
            return obj.GetValue(StyleSheetProperty) as StyleSheet;
        }
        public static void SetStyleSheet(BindableObject obj, StyleSheet value)
        {
            obj.SetValue(StyleSheetProperty, value);
        }

        public static readonly BindableProperty ClassProperty =
            BindableProperty.CreateAttached(
                "Class",
                typeof(string),
                typeof(Css),
                null,
                BindingMode.TwoWay,
                null,
                ClassPropertyChanged);
        private static void ClassPropertyChanged(BindableObject element, object oldValue, object newValue)
        {
            if (instance is null)
            {
                return;
            }

            IDomElement<BindableObject, BindableProperty> domElementBase = null;
            if (instance.treeNodeProvider.TryGetDomElement(element, out domElementBase) != true)
            {
                return;
            }

            var domElement = (DomElementBase<BindableObject, BindableProperty>)domElementBase;

            domElement.ResetClassList();
            if (domElement.IsReady == true)
            {
                instance.UpdateElement(element);
            }
        }
        public static string GetClass(BindableObject obj)
        {
            return obj.GetValue(ClassProperty) as string;
        }
        public static void SetClass(BindableObject obj, string value)
        {
            obj.SetValue(ClassProperty, value);
        }

        public static readonly BindableProperty DomElementProperty =
            BindableProperty.CreateAttached(
                "DomElement",
                typeof(IDomElement<BindableObject, BindableProperty>),
                typeof(Css),
                null,
                BindingMode.TwoWay);
        public static IDomElement<BindableObject, BindableProperty> GetDomElement(BindableObject obj)
        {
            return obj?.GetValue(DomElementProperty) as IDomElement<BindableObject, BindableProperty>;
        }
        public static void SetDomElement(BindableObject obj, IDomElement<BindableObject, BindableProperty> value)
        {
            obj?.SetValue(DomElementProperty, value);
        }

        public static readonly BindableProperty ApplyStyleImmediatelyProperty =
            BindableProperty.CreateAttached(
                "ApplyStyleImmediately",
                typeof(bool),
                typeof(Css),
                false,
                BindingMode.TwoWay);
        public static bool GetApplyStyleImmediately(BindableObject obj)
        {
            return (bool)obj?.GetValue(ApplyStyleImmediatelyProperty);
        }
        public static void SetApplyStyleImmediately(BindableObject obj, bool value)
        {
            obj?.SetValue(ApplyStyleImmediatelyProperty, value);
        }

        private static void StyleSheetPropertyChanged(BindableObject bindableObject, object oldValue, object newValue)
        {
            var element = bindableObject as Element;

            if (oldValue != null)
            {
                var oldStyleSheet = oldValue as StyleSheet;
                oldStyleSheet.PropertyChanged -= StyleSheet_PropertyChanged;

                instance?.EnqueueRemoveStyleSheet(element, (StyleSheet)oldValue);
            }

            var newStyleSheet = (StyleSheet)newValue;

            if (newStyleSheet == null)
            {
                return;
            }

            newStyleSheet.PropertyChanged += StyleSheet_PropertyChanged;

            instance?.EnqueueRenderStyleSheet(element, newStyleSheet);
        }

        private static void StyleSheet_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StyleSheet.Content))
            {
                var styleSheet = sender as StyleSheet;
                var attachedTo = styleSheet.AttachedTo as Element;

                instance?.EnqueueUpdateStyleSheet(attachedTo, styleSheet);
            }
        }

        private static void StylePropertyAttached(BindableObject element, object oldValue, object newValue)
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

        public static readonly BindableProperty AdditionalChildrenProperty =
            BindableProperty.CreateAttached(
                "AdditionalChildren",
                typeof(IList<BindableObject>),
                typeof(Css),
                null,
                BindingMode.TwoWay);
        public static IList<BindableObject> GetAdditionalChildren(BindableObject obj)
        {
            var value = obj?.GetValue(AdditionalChildrenProperty) as IList<BindableObject>;

            return value;
        }
        public static void SetAdditionalChildren(BindableObject obj, IList<BindableObject> value)
        {
            obj?.SetValue(AdditionalChildrenProperty, value);
        }

        public static void AddAdditionalChild(BindableObject parent, BindableObject child)
        {
            if (ReferenceEquals(parent, null) ||
                ReferenceEquals(child, null))
            {
                return;
            }

            var additionalChildren = Css.GetAdditionalChildren(parent);
            if (additionalChildren == null)
            {
                additionalChildren = new List<BindableObject>();
                Css.SetAdditionalChildren(parent, additionalChildren);
            }

            additionalChildren.Add(child);

            var domParent = Css.GetDomElement(parent) as DomElement;
            domParent?.ReloadChildren();

            Css.instance?.NewElement(child);
        }

        public static void RemoveAdditionalChild(BindableObject parent, BindableObject child)
        {
            if (ReferenceEquals(parent, null) ||
                ReferenceEquals(child, null))
            {
                return;
            }

            var additionalChildren = Css.GetAdditionalChildren(parent);
            if (additionalChildren == null)
            {
                return;
            }

            additionalChildren.Remove(child);

            if (additionalChildren.Count == 0)
            {
                Css.SetAdditionalChildren(parent, null);
            }

            var domParent = Css.GetDomElement(parent) as DomElement;
            domParent?.ReloadChildren();

            Css.instance?.RemoveElement(child);
        }
    }
}

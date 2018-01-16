using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XamlCSS.CssParsing;
using XamlCSS.Dom;
using XamlCSS.Utils;

namespace XamlCSS
{
    public class BaseCss<TDependencyObject, TUIElement, TStyle, TDependencyProperty>
        where TDependencyObject : class
        where TUIElement : class, TDependencyObject
        where TStyle : class
        where TDependencyProperty : class
    {
        public readonly IDependencyPropertyService<TDependencyObject, TUIElement, TStyle, TDependencyProperty> dependencyPropertyService;
        public readonly ISwitchableTreeNodeProvider<TDependencyObject> treeNodeProvider;
        public readonly IStyleResourcesService applicationResourcesService;
        public readonly INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService;
        private readonly IMarkupExtensionParser markupExpressionParser;
        private Action<Action> uiInvoker;
        private List<RenderInfo<TDependencyObject, TUIElement>> items = new List<RenderInfo<TDependencyObject, TUIElement>>();

        public BaseCss(IDependencyPropertyService<TDependencyObject, TUIElement, TStyle, TDependencyProperty> dependencyPropertyService,
            ISwitchableTreeNodeProvider<TDependencyObject> treeNodeProvider,
            IStyleResourcesService applicationResourcesService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService,
            string defaultCssNamespace,
            IMarkupExtensionParser markupExpressionParser,
            Action<Action> uiInvoker,
            ICssFileProvider fileProvider)
        {
            this.dependencyPropertyService = dependencyPropertyService;
            this.treeNodeProvider = treeNodeProvider;
            this.applicationResourcesService = applicationResourcesService;
            this.nativeStyleService = nativeStyleService;
            this.markupExpressionParser = markupExpressionParser;
            this.uiInvoker = uiInvoker;
            this.cssTypeHelper = new CssTypeHelper<TDependencyObject, TUIElement, TDependencyProperty, TStyle>(markupExpressionParser, dependencyPropertyService);

            CssParser.Initialize(defaultCssNamespace, fileProvider);
            StyleSheet.GetParent = parent => treeNodeProvider.GetParent((TDependencyObject)parent);
            StyleSheet.GetStyleSheet = treeNode => dependencyPropertyService.GetStyleSheet((TDependencyObject)treeNode);
        }

        protected bool executeApplyStylesExecuting;
        private CssTypeHelper<TDependencyObject, TUIElement, TDependencyProperty, TStyle> cssTypeHelper;
        private int noopCount = 0;
        public void ExecuteApplyStyles()
        {

            if (executeApplyStylesExecuting)
            {
                return;
            }

            executeApplyStylesExecuting = true;

            List<RenderInfo<TDependencyObject, TUIElement>> copy;

            //lock (items)
            {
                if (items.Count == 0)
                {
                    noopCount++;
                    executeApplyStylesExecuting = false;
                    return;
                }

                copy = items.Distinct().ToList();
                items.Clear();
            }
            Debug.WriteLine("Noops: " + noopCount);
            noopCount = 0;

            //applicationResourcesService.BeginUpdate();

            "Render".Measure(() =>
            {
                BaseCSS2<TDependencyObject, TUIElement, TStyle, TDependencyProperty>
                    .Render(copy, treeNodeProvider, dependencyPropertyService, nativeStyleService, applicationResourcesService, cssTypeHelper);
            });

            Investigate.Print();
            executeApplyStylesExecuting = false;
        }

        public void EnqueueRenderStyleSheet(TUIElement styleSheetHolder, StyleSheet styleSheet)
        {
            if (styleSheetHolder == null ||
                styleSheet == null)
            {
                return;
            }

            lock (items)
            {
                items.Add(new RenderInfo<TDependencyObject, TUIElement>
                {
                    RenderTargetKind = RenderTargetKind.Stylesheet,
                    ChangeKind = ChangeKind.New,
                    StyleSheetHolder = styleSheetHolder,
                    StyleSheet = styleSheet,
                    StartFrom = null
                });
            }
        }

        public void EnqueueUpdateStyleSheet(TUIElement styleSheetHolder, StyleSheet styleSheet)
        {
            if (styleSheetHolder == null ||
                styleSheet == null)
            {
                return;
            }

            lock (items)
            {
                items.Add(new RenderInfo<TDependencyObject, TUIElement>
                {
                    RenderTargetKind = RenderTargetKind.Stylesheet,
                    ChangeKind = ChangeKind.Update,
                    StyleSheetHolder = styleSheetHolder,
                    StyleSheet = styleSheet,
                    StartFrom = null
                });
            }
        }

        public void EnqueueUpdateElement(TUIElement styleSheetHolder, StyleSheet styleSheet, TUIElement startFrom)
        {
            if (styleSheetHolder == null ||
                styleSheet == null)
            {
                return;
            }

            lock (items)
            {
                items.Add(new RenderInfo<TDependencyObject, TUIElement>
                {
                    RenderTargetKind = RenderTargetKind.Element,
                    ChangeKind = ChangeKind.Update,
                    StyleSheetHolder = styleSheetHolder,
                    StyleSheet = styleSheet,
                    StartFrom = startFrom
                });
            }
        }

        public void EnqueueNewElement(TUIElement styleSheetHolder, StyleSheet styleSheet, TUIElement startFrom)
        {
            if (styleSheetHolder == null ||
                styleSheet == null)
            {
                return;
            }

            lock (items)
            {
                items.Add(new RenderInfo<TDependencyObject, TUIElement>
                {
                    RenderTargetKind = RenderTargetKind.Element,
                    ChangeKind = ChangeKind.New,
                    StyleSheetHolder = styleSheetHolder,
                    StyleSheet = styleSheet,
                    StartFrom = startFrom
                });
            }
        }

        public void NewElement(TDependencyObject sender)
        {
            if (sender == null)
            {
                return;
            }

            var parent = GetStyleSheetParent(sender as TDependencyObject) as TUIElement;
            if (parent == null)
            {
                return;
            }

            /*if (dependencyPropertyService.GetInitialStyle(sender) == null)
            {
                dependencyPropertyService.SetInitialStyle(sender, nativeStyleService.GetStyle(sender));
            }*/
            EnqueueNewElement(
                parent,
                dependencyPropertyService.GetStyleSheet(parent),
                sender as TUIElement);
        }

        public void EnqueueRemoveElement(TUIElement styleSheetHolder, StyleSheet styleSheet, TUIElement startFrom)
        {
            if (styleSheetHolder == null ||
                styleSheet == null)
            {
                return;
            }

            lock (items)
            {
                items.Add(new RenderInfo<TDependencyObject, TUIElement>
                {
                    RenderTargetKind = RenderTargetKind.Element,
                    ChangeKind = ChangeKind.Remove,
                    StyleSheetHolder = styleSheetHolder,
                    StyleSheet = styleSheet,
                    StartFrom = startFrom
                });
            }
        }

        public void RemoveElement(TDependencyObject sender)
        {
            if (sender == null)
            {
                return;
            }

            var parent = GetStyleSheetParent(sender as TDependencyObject) as TUIElement;
            if (parent == null)
            {
                return;
            }

            EnqueueRemoveElement(
                parent,
                dependencyPropertyService.GetStyleSheet(parent),
                sender as TUIElement);
        }

        public void UpdateElement(TDependencyObject sender)
        {
            if (sender == null)
            {
                return;
            }

            var parent = GetStyleSheetParent(sender as TDependencyObject) as TUIElement;
            if (parent == null)
            {
                return;
            }

            EnqueueUpdateElement(
                parent,
                dependencyPropertyService.GetStyleSheet(parent),
                sender as TUIElement);
        }

        public void EnqueueRemoveStyleSheet(TUIElement styleSheetHolder, StyleSheet styleSheet)
        {
            if (styleSheetHolder == null ||
                styleSheet == null)
            {
                return;
            }

            lock (items)
            {
                items.Add(new RenderInfo<TDependencyObject, TUIElement>
                {
                    RenderTargetKind = RenderTargetKind.Stylesheet,
                    ChangeKind = ChangeKind.Remove,
                    StyleSheetHolder = styleSheetHolder,
                    StyleSheet = styleSheet,
                    StartFrom = null
                });
            }
        }

        private StyleSheet GetStyleSheetFromTree(IDomElement<TDependencyObject> domElement)
        {
            var current = domElement;
            StyleSheet styleSheet = null;
            while (current != null &&
                (styleSheet = dependencyPropertyService.GetStyleSheet(current.Element)) == null)
            {
                current = current.Parent;
            }

            return styleSheet;
        }

        private TDependencyObject GetStyleSheetParent(TDependencyObject obj)
        {
            var currentBindableObject = obj;

            try
            {
                while (currentBindableObject != null)
                {
                    var styleSheet = dependencyPropertyService.GetStyleSheet(currentBindableObject);
                    if (styleSheet != null)
                    {
                        return currentBindableObject;
                    }

                    currentBindableObject = treeNodeProvider.GetParent(currentBindableObject as TDependencyObject);
                }
            }
            finally
            {
            }

            return null;
        }
    }
}

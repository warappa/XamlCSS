using System;
using System.Collections.Generic;
using System.Linq;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class BaseCss<TDependencyObject, TUIElement, TStyle, TDependencyProperty>
        where TDependencyObject : class
        where TUIElement : class, TDependencyObject
        where TStyle : class
        where TDependencyProperty : class
    {
        public readonly IDependencyPropertyService<TDependencyObject, TUIElement, TStyle, TDependencyProperty> dependencyPropertyService;
        public readonly ITreeNodeProvider<TDependencyObject> treeNodeProvider;
        public readonly IStyleResourcesService applicationResourcesService;
        public readonly INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService;
        private readonly IMarkupExtensionParser markupExpressionParser;
        private Action<Action> uiInvoker;
        private List<RenderInfo<TDependencyObject, TUIElement>> items = new List<RenderInfo<TDependencyObject, TUIElement>>();

        public BaseCss(IDependencyPropertyService<TDependencyObject, TUIElement, TStyle, TDependencyProperty> dependencyPropertyService,
            ITreeNodeProvider<TDependencyObject> treeNodeProvider,
            IStyleResourcesService applicationResourcesService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService,
            string defaultCssNamespace,
            IMarkupExtensionParser markupExpressionParser,
            Action<Action> uiInvoker)
        {
            this.dependencyPropertyService = dependencyPropertyService;
            this.treeNodeProvider = treeNodeProvider;
            this.applicationResourcesService = applicationResourcesService;
            this.nativeStyleService = nativeStyleService;
            this.markupExpressionParser = markupExpressionParser;
            this.uiInvoker = uiInvoker;

            CssParser.Initialize(defaultCssNamespace);
        }

        protected bool executeApplyStylesExecuting;

        public void ExecuteApplyStyles()
        {
            if (executeApplyStylesExecuting)
            {
                return;
            }

            executeApplyStylesExecuting = true;

            try
            {
                List<RenderInfo<TDependencyObject, TUIElement>> copy;

                lock (items)
                {
                    if (items.Any() == false)
                    {
                        return;
                    }

                    copy = items.Distinct().ToList();
                    items = new List<RenderInfo<TDependencyObject, TUIElement>>();
                }

                var invalidateItem = copy.FirstOrDefault(x => x.Remove);
                if (invalidateItem != null)
                {
                    RemoveStyleResourcesInternal(invalidateItem.StyleSheetHolder, invalidateItem.StyleSheet);
                    copy.Remove(invalidateItem);
                }

                copy.RemoveAll(x => x.Remove);

                if (copy.Any())
                {
                    foreach (var item in copy)
                    {
                        CalculateStylesInternal(item.StyleSheetHolder, item.StyleSheet, item.StartFrom);

                        ApplyMatchingStyles(item.StartFrom ?? item.StyleSheetHolder);
                    }
                }
            }
            finally
            {
                executeApplyStylesExecuting = false;
            }
        }

        public void EnqueueRenderStyleSheet(TUIElement styleSheetHolder, StyleSheet styleSheet, TUIElement startFrom)
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
                    StyleSheetHolder = styleSheetHolder,
                    StyleSheet = styleSheet,
                    StartFrom = startFrom,
                    Remove = false
                });
            }
        }

        public void EnqueueRemoveStyleSheet(TUIElement styleSheetHolder, StyleSheet styleSheet, TUIElement startFrom)
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
                    StyleSheetHolder = styleSheetHolder,
                    StyleSheet = styleSheet,
                    StartFrom = startFrom,
                    Remove = true
                });
            }
        }

        protected void CalculateStylesInternal(TUIElement styleResourceReferenceHolder, StyleSheet styleSheet, TUIElement startFrom)
        {
            if (styleResourceReferenceHolder == null ||
                styleSheet == null)
            {
                return;
            }
            
            UnapplyMatchingStylesInternal(startFrom ?? styleResourceReferenceHolder);

            IDomElement<TDependencyObject> root = null;

            IDomElement<TDependencyObject> visualTree = null;
            IDomElement<TDependencyObject> logicalTree = null;

            foreach (var rule in styleSheet.Rules)
            {
                if (rule.SelectorType == SelectorType.VisualTree)
                {
                    if (visualTree == null)
                    {
                        visualTree = treeNodeProvider.GetVisualTree(startFrom ?? styleResourceReferenceHolder);
                        visualTree.XamlCssStyleSheets.Clear();
                        visualTree.XamlCssStyleSheets.Add(styleSheet);
                    }

                    root = visualTree;
                }
                else
                {
                    if (logicalTree == null)
                    {
                        logicalTree = treeNodeProvider.GetLogicalTree(startFrom ?? styleResourceReferenceHolder);
                        logicalTree.XamlCssStyleSheets.Clear();
                        logicalTree.XamlCssStyleSheets.Add(styleSheet);
                    }

                    root = logicalTree;
                }

                // apply our selector
                var matchedNodes = root.QuerySelectorAllWithSelf(rule.SelectorString)
                    .Where(x => x != null)
                    .Cast<IDomElement<TDependencyObject>>()
                    .ToList();

                var matchedElementTypes = matchedNodes
                    .Select(x => x.Element.GetType())
                    .Distinct()
                    .ToList();

                applicationResourcesService.EnsureResources();

                foreach (var matchedElementType in matchedElementTypes)
                {
                    var resourceKey = nativeStyleService.GetStyleResourceKey(matchedElementType, rule.SelectorString);

                    if (applicationResourcesService.Contains(resourceKey))
                    {
                        continue;
                    }

                    var propertyStyleValues = CreateStyleDictionaryFromDeclarationBlock(
                        styleSheet.Namespaces,
                        rule.DeclarationBlock,
                        matchedElementType,
                        startFrom ?? styleResourceReferenceHolder);

                    if (propertyStyleValues.Keys.Count == 0)
                    {
                        continue;
                    }

                    var style = nativeStyleService.CreateFrom(propertyStyleValues, matchedElementType);

                    applicationResourcesService.SetResource(resourceKey, style);
                }

                foreach (var matchingNode in matchedNodes)
                {
                    var element = matchingNode.Element;

                    var matchingStyles = dependencyPropertyService.GetMatchingStyles(element) ?? new string[0];

                    var resourceKey = nativeStyleService.GetStyleResourceKey(element.GetType(), rule.SelectorString);

                    if (applicationResourcesService.Contains(resourceKey))
                    {
                        dependencyPropertyService.SetMatchingStyles(element, matchingStyles.Concat(new[] { resourceKey }).Distinct().ToArray());
                    }
                }
            }
        }

        private Dictionary<TDependencyProperty, object> CreateStyleDictionaryFromDeclarationBlock(
            List<CssNamespace> namespaces, 
            StyleDeclarationBlock declarationBlock, 
            Type matchedType,
            TDependencyObject dependencyObject)
        {
            var propertyStyleValues = new Dictionary<TDependencyProperty, object>();

            foreach (var i in declarationBlock)
            {
                TDependencyProperty property;

                if (i.Property.Contains("."))
                {
                    string typename = null;
                    string propertyName = null;

                    if (i.Property.Contains("|"))
                    {
                        var strs = i.Property.Split('|', '.');
                        var alias = strs[0];
                        var namespaceFragments = namespaces
                            .First(x => x.Alias == alias)
                            .Namespace
                            .Split(',');

                        typename = $"{namespaceFragments[0]}.{strs[1]}, {string.Join(",", namespaceFragments.Skip(1))}";
                        propertyName = strs[2];
                    }
                    else
                    {
                        var strs = i.Property.Split('.');
                        var namespaceFragments = namespaces
                            .First(x => x.Alias == "")
                            .Namespace
                            .Split(',');

                        typename = $"{namespaceFragments[0]}.{strs[0]}, {string.Join(",", namespaceFragments.Skip(1))}";
                        propertyName = strs[1];
                    }

                    property = dependencyPropertyService.GetBindableProperty(Type.GetType(typename), propertyName);
                }
                else
                {
                    property = dependencyPropertyService.GetBindableProperty(matchedType, i.Property);
                }

                if (property == null)
                {
                    continue;
                }

                object propertyValue = null;
                if (i.Value is string &&
                    ((string)i.Value).StartsWith("{", StringComparison.Ordinal))
                {
                    propertyValue = markupExpressionParser.ProvideValue((string)i.Value, dependencyObject);
                }
                else
                {
                    propertyValue = dependencyPropertyService.GetBindablePropertyValue(matchedType, property, i.Value);
                }

                propertyStyleValues[property] = propertyValue;
            }

            return propertyStyleValues;
        }

        public void RemoveStyleResources(TUIElement styleResourceReferenceHolder, StyleSheet styleSheet)
        {
            EnqueueRemoveStyleSheet(styleResourceReferenceHolder, styleSheet, null);
        }
        protected void RemoveStyleResourcesInternal(TUIElement styleResourceReferenceHolder, StyleSheet styleSheet)
        {
            UnapplyMatchingStylesInternal(styleResourceReferenceHolder);

            var resourceKeys = applicationResourcesService.GetKeys()
                .OfType<string>()
                .Where(x => x.StartsWith(nativeStyleService.BaseStyleResourceKey, StringComparison.Ordinal))
                .ToList();

            foreach (var key in resourceKeys)
            {
                applicationResourcesService.RemoveResource(key);
            }
        }

        private void ApplyMatchingStyles(TUIElement visualElement)
        {
            if (visualElement == null ||
                dependencyPropertyService.GetHandledCss(visualElement))
            {
                return;
            }

            foreach (var child in treeNodeProvider.GetChildren(visualElement).ToList())
            {
                ApplyMatchingStyles(child as TUIElement);
            }

            var matchingStyles = dependencyPropertyService.GetMatchingStyles(visualElement);
            var appliedMatchingStyles = dependencyPropertyService.GetAppliedMatchingStyles(visualElement);

            if (matchingStyles == appliedMatchingStyles ||
                (
                    matchingStyles != null &&
                    appliedMatchingStyles != null &&
                    matchingStyles.SequenceEqual(appliedMatchingStyles)
                ))
            {
                return;
            }

            object styleToApply = null;

            if (matchingStyles?.Length == 1)
            {
                if (applicationResourcesService.Contains(matchingStyles[0]) == true)
                {
                    styleToApply = applicationResourcesService.GetResource(matchingStyles[0]);
                }

                if (styleToApply != null)
                {
                    nativeStyleService.SetStyle(visualElement, (TStyle)styleToApply);
                }
            }
            else if (matchingStyles?.Length > 1)
            {
                var dict = new Dictionary<TDependencyProperty, object>();

                foreach (var matchingStyle in matchingStyles)
                {
                    object s = null;
                    if (applicationResourcesService.Contains(matchingStyle) == true)
                    {
                        s = applicationResourcesService.GetResource(matchingStyle);
                    }

                    var subDict = nativeStyleService.GetStyleAsDictionary(s as TStyle);

                    if (subDict != null)
                    {
                        foreach (var i in subDict)
                        {
                            dict[i.Key] = i.Value;
                        }
                    }
                }

                if (dict.Keys.Count > 0)
                {
                    styleToApply = nativeStyleService.CreateFrom(dict, visualElement.GetType());
                }

                if (styleToApply != null)
                {
                    nativeStyleService.SetStyle(visualElement, (TStyle)styleToApply);
                }
            }

            dependencyPropertyService.SetHandledCss(visualElement, true);
            dependencyPropertyService.SetAppliedMatchingStyles(visualElement, matchingStyles);
        }

        public void UnapplyMatchingStyles(TDependencyObject bindableObject)
        {
            uiInvoker(() =>
            {
                UnapplyMatchingStylesInternal(bindableObject);
            });
        }
        protected void UnapplyMatchingStylesInternal(TDependencyObject bindableObject)
        {
            if (bindableObject == null)
            {
                return;
            }

            foreach (var child in treeNodeProvider.GetChildren(bindableObject).ToList())
            {
                UnapplyMatchingStylesInternal(child);
            }

            dependencyPropertyService.SetHandledCss(bindableObject, false);
            dependencyPropertyService.SetMatchingStyles(bindableObject, null);
            dependencyPropertyService.SetAppliedMatchingStyles(bindableObject, null);
            nativeStyleService.SetStyle(bindableObject, dependencyPropertyService.GetInitialStyle(bindableObject));
        }

        public void UpdateElement(TDependencyObject sender)
        {
            var parent = GetStyleSheetParent(sender as TDependencyObject) as TUIElement;
            if (parent == null)
            {
                return;
            }

            EnqueueRenderStyleSheet(
                parent,
                dependencyPropertyService.GetStyleSheet(parent),
                sender as TUIElement);
        }

        private TDependencyObject GetStyleSheetParent(TDependencyObject obj)
        {
            var currentBindableObject = obj;
            while (currentBindableObject != null)
            {
                var styleSheet = dependencyPropertyService.GetStyleSheet(currentBindableObject);
                if (styleSheet != null)
                    return currentBindableObject;

                currentBindableObject = treeNodeProvider.GetParent(currentBindableObject as TUIElement);
            }

            return null;
        }
    }
}

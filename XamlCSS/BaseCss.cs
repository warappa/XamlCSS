using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    items.Clear();
                }

                var invalidateAllItems = copy
                    .Where(x => x.Remove && x.StartFrom == null)
                    .GroupBy(x => x.StyleSheetHolder)
                    .Select(x => x.First())
                    .ToList();

                var handledStyleSheetHolders = invalidateAllItems
                    .Select(x => x.StyleSheetHolder)
                    .ToList();

                foreach (var invalidateAllItem in invalidateAllItems)
                {
                    RemoveStyleResourcesInternal(invalidateAllItem.StyleSheetHolder, invalidateAllItem.StyleSheet);
                    UnapplyMatchingStylesInternal(invalidateAllItem.StyleSheetHolder, invalidateAllItem.StyleSheet);
                }

                var invalidateSpecificItems = copy
                    .Where(x => x.Remove && x.StartFrom != null)
                    .GroupBy(x => x.StartFrom)
                    .Select(x => x.First())
                    .Where(x => !handledStyleSheetHolders.Contains(x.StyleSheetHolder))
                    .ToList();

                foreach (var invalidateSpecificItem in invalidateSpecificItems)
                {
                    UnapplyMatchingStylesInternal(invalidateSpecificItem.StartFrom, invalidateSpecificItem.StyleSheet);
                }

                copy.RemoveAll(x => x.Remove);

                if (copy.Any())
                {
                    foreach (var item in copy)
                    {
                        CalculateStylesInternal(item.StyleSheetHolder, item.StyleSheet, item.StartFrom);

                        ApplyMatchingStyles(item.StartFrom ?? item.StyleSheetHolder, item.StyleSheet);
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

        private void Recursive(TDependencyObject element, int level, TDependencyObject expectedParent)
        {
            if (element == null)
            {
                return;
            }

            if (expectedParent != treeNodeProvider.GetParent(element))
            {
                Debug.WriteLine("!!!!!");
                Debug.WriteLine($"Expected parent: { dependencyPropertyService.GetName(expectedParent) }");
                Debug.WriteLine($"Actual parent:   { dependencyPropertyService.GetName(treeNodeProvider.GetParent(element)) }");
                Debug.WriteLine("!!!!!");
            }

            Debug.WriteLine(new String(' ', level) + element.GetType().Name + "#" + dependencyPropertyService.GetName(element));
            var children = treeNodeProvider.GetChildren(element);
            foreach (var child in children)
            {
                Recursive(child, level + 1, element);
            }
        }

        private void RecursiveDom(IDomElement<TDependencyObject> domElement, int level, IDomElement<TDependencyObject> expectedParent)
        {
            if (domElement == null)
            {
                return;
            }

            if (expectedParent != domElement.ParentElement)
            {
                Debug.WriteLine("!!!!!");
                Debug.WriteLine($"Expected parent: { domElement.TagName + "#" + expectedParent.Id }");
                Debug.WriteLine($"Actual parent:   { domElement.ParentElement.TagName + "#" + domElement.ParentElement.Id }");
                Debug.WriteLine("!!!!!");
            }

            Debug.WriteLine(new String(' ', level) + domElement.Element.GetType().Name + "#" + domElement.Id);

            var children = treeNodeProvider.GetDomElementChildren(domElement);
            foreach (var child in children)
            {
                RecursiveDom(child, level + 1, domElement);
            }
        }

        protected void CalculateStylesInternal(TUIElement styleResourceReferenceHolder, StyleSheet styleSheet, TUIElement startFrom)
        {
            if (styleResourceReferenceHolder == null ||
                styleSheet == null)
            {
                return;
            }

            // PrintHerarchyDebugInfo(styleResourceReferenceHolder, startFrom);

            UnapplyMatchingStylesInternal(startFrom ?? styleResourceReferenceHolder, styleSheet);

            var requiredStyleInfos = UpdateMatchingStyles(styleResourceReferenceHolder, styleSheet, startFrom);

            GenerateStyles(styleResourceReferenceHolder, styleSheet, startFrom, requiredStyleInfos);
        }

        private void GenerateStyles(TUIElement styleResourceReferenceHolder, StyleSheet styleSheet, TUIElement startFrom, List<StyleMatchInfo> styleMatchInfos)
        {
            applicationResourcesService.EnsureResources();

            foreach (var styleMatchInfo in styleMatchInfos)
            {
                var matchedElementType = styleMatchInfo.MatchedType;

                var resourceKey = nativeStyleService.GetStyleResourceKey(styleSheet.Id, matchedElementType, styleMatchInfo.Rule.SelectorString);

                if (applicationResourcesService.Contains(resourceKey))
                {
                    continue;
                }

                var propertyStyleValues = CreateStyleDictionaryFromDeclarationBlock(
                    styleSheet.Namespaces,
                    styleMatchInfo.Rule.DeclarationBlock,
                    matchedElementType,
                    startFrom ?? styleResourceReferenceHolder);

                var nativeTriggers = styleMatchInfo.Rule.DeclarationBlock.Triggers
                    .Select(x => nativeStyleService.CreateTrigger(styleSheet, x, styleMatchInfo.MatchedType, styleResourceReferenceHolder));

                if (propertyStyleValues.Keys.Count == 0)
                {
                    continue;
                }

                var style = nativeStyleService.CreateFrom(propertyStyleValues, nativeTriggers, matchedElementType);

                applicationResourcesService.SetResource(resourceKey, style);
            }
        }

        public class StyleMatchInfo
        {
            public StyleRule Rule { get; set; }
            public Type MatchedType { get; set; }
        }
        private List<StyleMatchInfo> UpdateMatchingStyles(TUIElement styleResourceReferenceHolder, StyleSheet styleSheet, TUIElement startFrom)
        {
            var requiredStyleInfos = new List<StyleMatchInfo>();
            IDomElement<TDependencyObject> root = null;

            IDomElement<TDependencyObject> visualTree = null;
            IDomElement<TDependencyObject> logicalTree = null;

            foreach (var rule in styleSheet.Rules)
            {
                if (rule.SelectorType == SelectorType.VisualTree)
                {
                    if (visualTree == null)
                    {
                        visualTree = treeNodeProvider.GetDomElement(startFrom ?? styleResourceReferenceHolder);
                        visualTree.XamlCssStyleSheets.Clear();
                        visualTree.XamlCssStyleSheets.Add(styleSheet);
                    }

                    root = visualTree;
                }
                else
                {
                    if (logicalTree == null)
                    {
                        logicalTree = treeNodeProvider.GetDomElement(startFrom ?? styleResourceReferenceHolder);
                        logicalTree.XamlCssStyleSheets.Clear();
                        logicalTree.XamlCssStyleSheets.Add(styleSheet);
                    }

                    root = logicalTree;
                }

                // apply our selector
                var matchedNodes = root.QuerySelectorAllWithSelf(rule.SelectorString)
                    .Where(x => x != null)
                    .Cast<IDomElement<TDependencyObject>>()
                    .Where(x => treeNodeProvider.GetParent(x.Element) != null) // workaround WPF: somehow dom-tree is out of sync with UI-tree!?!
                    .ToList();

                var otherStyleElements = matchedNodes
                    .Where(x =>
                    {
                        var parent = GetStyleSheetParent(x.Element);
                        // parent happens to be null if class was changed by binding
                        if (parent == null)
                        {
                            return true;
                        }
                        var elementStyleSheet = dependencyPropertyService.GetStyleSheet(parent);
                        return elementStyleSheet != null && elementStyleSheet != styleSheet;
                    }).ToList();

                matchedNodes = matchedNodes.Except(otherStyleElements).ToList();

                // Debug.WriteLine($"matchedNodes: ({matchedNodes.Count}) " + string.Join(", ", matchedNodes.Select(x => x.Id)));

                var matchedElementTypes = matchedNodes
                    .Select(x => x.Element.GetType())
                    .Distinct()
                    .ToList();

                // Debug.WriteLine($"Matched Types: ({matchedElementTypes.Count}) " + string.Join(", ", matchedElementTypes.Select(x => x.Name)));

                foreach (var matchingNode in matchedNodes)
                {
                    var element = matchingNode.Element;

                    var matchingStyles = dependencyPropertyService.GetMatchingStyles(element) ?? new string[0];

                    var resourceKey = nativeStyleService.GetStyleResourceKey(styleSheet.Id, element.GetType(), rule.SelectorString);

                    dependencyPropertyService.SetMatchingStyles(element, matchingStyles.Concat(new[] { resourceKey }).Distinct().ToArray());

                    if (requiredStyleInfos.Any(x => x.Rule == rule && x.MatchedType == element.GetType()) == false)
                    {
                        requiredStyleInfos.Add(new StyleMatchInfo
                        {
                            Rule = rule,
                            MatchedType = element.GetType()
                        });
                    }
                }
            }

            return requiredStyleInfos;
        }

        private void PrintHerarchyDebugInfo(TUIElement styleResourceReferenceHolder, TUIElement startFrom)
        {
            Debug.WriteLine("");
            Debug.WriteLine("------------------");
            Debug.WriteLine("Print FrameworkElement hierarchy:");
            Debug.WriteLine("----------------");
            Debug.WriteLine("----------------");

            var s = startFrom ?? styleResourceReferenceHolder;
            Recursive(s, 0, treeNodeProvider.GetParent(s));

            Debug.WriteLine("");
            Debug.WriteLine("Print DomElement hierarchy:");
            Debug.WriteLine("----------------");

            var sDom = treeNodeProvider.GetDomElement(s);
            RecursiveDom(sDom, 0, (IDomElement<TDependencyObject>)sDom.ParentElement);

            Debug.WriteLine("----------------");
            Debug.WriteLine("----------------");
        }

        private Dictionary<TDependencyProperty, object> CreateStyleDictionaryFromDeclarationBlock(
            List<CssNamespace> namespaces,
            StyleDeclarationBlock declarationBlock,
            Type matchedType,
            TDependencyObject dependencyObject)
        {
            var propertyStyleValues = new Dictionary<TDependencyProperty, object>();

            foreach (var styleDeclaration in declarationBlock)
            {
                var property = cssTypeHelper.GetDependencyProperty(namespaces, matchedType, styleDeclaration.Property);

                if (property == null)
                {
                    continue;
                }

                var propertyValue = cssTypeHelper.GetPropertyValue(matchedType, dependencyObject, styleDeclaration.Value, property);

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
            // Debug.WriteLine("----------------");
            // Debug.WriteLine("RemoveStyleResourcesInternal");

            var resourceKeys = applicationResourcesService.GetKeys()
                .OfType<string>()
                .Where(x => x.StartsWith(nativeStyleService.BaseStyleResourceKey + "_" + styleSheet.Id, StringComparison.Ordinal))
                .ToList();

            // Debug.WriteLine(" - remove resourceKeys: " + string.Join(", ", resourceKeys));

            foreach (var key in resourceKeys)
            {
                applicationResourcesService.RemoveResource(key);
            }
        }

        private void ApplyMatchingStyles(TUIElement visualElement, StyleSheet styleSheet)
        {
            if (visualElement == null ||
                dependencyPropertyService.GetHandledCss(visualElement))
            {
                return;
            }

            var currentStyleSheet = dependencyPropertyService.GetStyleSheet(visualElement);
            if (currentStyleSheet != null &&
                currentStyleSheet != styleSheet)
            {
                return;
            }

            foreach (var child in treeNodeProvider.GetChildren(visualElement).ToList())
            {
                ApplyMatchingStyles(child as TUIElement, styleSheet);
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
                var listTriggers = new List<TDependencyObject>();

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

                        var triggers = nativeStyleService.GetTriggersAsList(s as TStyle);
                        listTriggers.AddRange(triggers);
                    }
                }

                if (dict.Keys.Count > 0)
                {
                    styleToApply = nativeStyleService.CreateFrom(dict, listTriggers, visualElement.GetType());
                }

                if (styleToApply != null)
                {
                    nativeStyleService.SetStyle(visualElement, (TStyle)styleToApply);
                }
            }

            dependencyPropertyService.SetHandledCss(visualElement, true);
            dependencyPropertyService.SetAppliedMatchingStyles(visualElement, matchingStyles);

            // Debug.WriteLine($"Applying: {string.Join(", ", dependencyPropertyService.GetMatchingStyles(visualElement) ?? new string[0])}");
        }

        public void UnapplyMatchingStyles(TDependencyObject bindableObject, StyleSheet styleSheet)
        {
            uiInvoker(() =>
            {
                UnapplyMatchingStylesInternal(bindableObject, styleSheet);
            });
        }
        protected void UnapplyMatchingStylesInternal(TDependencyObject bindableObject, StyleSheet styleSheet)
        {
            if (bindableObject == null)
            {
                return;
            }

            var currentStyleSheet = dependencyPropertyService.GetStyleSheet(bindableObject);
            if (currentStyleSheet != null &&
                currentStyleSheet != styleSheet)
            {
                return;
            }

            foreach (var child in treeNodeProvider.GetChildren(bindableObject).ToList())
            {
                UnapplyMatchingStylesInternal(child, styleSheet);
            }

            /*var matchingStyles = dependencyPropertyService.GetMatchingStyles(bindableObject) ?? new string[0];
            if (matchingStyles.Length > 0)
            {
                Debug.WriteLine($"Unapply: {string.Join(", ", dependencyPropertyService.GetMatchingStyles(bindableObject) ?? new string[0])}");
            }*/

            dependencyPropertyService.SetHandledCss(bindableObject, false);
            dependencyPropertyService.SetMatchingStyles(bindableObject, null);
            dependencyPropertyService.SetAppliedMatchingStyles(bindableObject, null);
            nativeStyleService.SetStyle(bindableObject, dependencyPropertyService.GetInitialStyle(bindableObject));
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

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

                var removeStylesheetInfos = copy
                    .Where(x =>
                        x.RenderTargetKind == RenderTargetKind.Stylesheet &&
                        (x.ChangeKind == ChangeKind.Remove || x.ChangeKind == ChangeKind.Update))
                    .GroupBy(x => x.StyleSheetHolder)
                    .Select(x => x.First())
                    .ToList();

                var handledStyleSheetHolders = removeStylesheetInfos
                    .Select(x => x.StyleSheetHolder)
                    .ToList();

                foreach (var removeStylesheetInfo in removeStylesheetInfos)
                {
                    // sets HandledCss and MatchingStyles false/null
                    UnapplyMatchingStylesInternal(removeStylesheetInfo.StyleSheetHolder, removeStylesheetInfo.StyleSheet);

                    if (removeStylesheetInfo.ChangeKind == ChangeKind.Remove)
                    {
                        removeStylesheetInfo.StyleSheet.AttachedTo = null;
                    }

                    // compares MatchingStyles and AppliedMachingStyles and 
                    // sets AppliedMatchingStyles and Style null
                    RemoveOutdatedStylesFromElementInternal(removeStylesheetInfo.StyleSheetHolder, removeStylesheetInfo.StyleSheet, true, true);

                    // remove Style resources of Stylesheet
                    RemoveStyleResourcesInternal(removeStylesheetInfo.StyleSheetHolder, removeStylesheetInfo.StyleSheet);
                }

                var removedElementInfos = copy
                    .Where(x =>
                        x.RenderTargetKind == RenderTargetKind.Element &&
                        x.ChangeKind == ChangeKind.Remove)
                    .GroupBy(x => x.StyleSheetHolder)
                    .Select(x => x.First())
                    .ToList();

                foreach (var removedElementInfo in removedElementInfos)
                {
                    // sets HandledCss and MatchingStyles false/null
                    UnapplyMatchingStylesInternal(removedElementInfo.StartFrom, removedElementInfo.StyleSheet);
                    // compares MatchingStyles and AppliedMachingStyles and 
                    // sets AppliedMatchingStyles and Style null
                    RemoveOutdatedStylesFromElementInternal(removedElementInfo.StartFrom, removedElementInfo.StyleSheet, true, true);
                }

                copy.RemoveAll(x => x.ChangeKind == ChangeKind.Remove ||
                    (
                        x.RenderTargetKind == RenderTargetKind.Element && 
                        handledStyleSheetHolders.Contains(x.StyleSheetHolder)
                    ));

                if (copy.Any())
                {
                    foreach (var item in copy.Where(x => x.ChangeKind == ChangeKind.Update))
                    {
                        // sets HandledCss and MatchingStyles false/null
                        UnapplyMatchingStylesInternal(item.StartFrom ?? item.StyleSheetHolder, item.StyleSheet);
                    }

                    foreach (var item in copy)
                    {
                        CalculateStylesInternal(item.StyleSheetHolder, item.StyleSheet, item.StartFrom ?? item.StyleSheetHolder);
                        RemoveOutdatedStylesFromElementInternal(item.StartFrom ?? item.StyleSheetHolder, item.StyleSheet, true, true);
                        ApplyMatchingStyles(item.StartFrom ?? item.StyleSheetHolder, item.StyleSheet);
                    }
                }
            }
            finally
            {
                executeApplyStylesExecuting = false;
            }
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

        private List<StyleMatchInfo> UpdateMatchingStyles(TUIElement styleResourceReferenceHolder, StyleSheet styleSheet, TUIElement startFrom)
        {
            var requiredStyleInfos = new List<StyleMatchInfo>();
            IDomElement<TDependencyObject> root = null;

            IDomElement<TDependencyObject> visualTree = null;
            IDomElement<TDependencyObject> logicalTree = null;

            foreach (var rule in styleSheet.Rules)
            {
                // Debug.WriteLine($"--- RULE {rule.SelectorString} ----");
                if (rule.SelectorType == SelectorType.VisualTree)
                {
                    treeNodeProvider.Switch(SelectorType.VisualTree);
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
                    treeNodeProvider.Switch(SelectorType.LogicalTree);
                    if (logicalTree == null)
                    {
                        if (!treeNodeProvider.IsInTree(startFrom ?? styleResourceReferenceHolder))
                        {
                            continue;
                        }

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

                var matchedElementTypes = matchedNodes
                    .Select(x => x.Element.GetType())
                    .Distinct()
                    .ToList();

                foreach (var matchingNode in matchedNodes)
                {
                    var element = matchingNode.Element;

                    var oldMatchingStyles = dependencyPropertyService.GetMatchingStyles(element) ?? new string[0];

                    var resourceKey = nativeStyleService.GetStyleResourceKey(styleSheet.Id, element.GetType(), rule.SelectorString);

                    var newMatchingStyles = oldMatchingStyles.Concat(new[] { resourceKey }).Distinct()
                        .Select(x => new
                        {
                            key = x,
                            selector = new Selector { Value = x.Split('{')[1] }
                        })
                        .OrderBy(x => x.selector.IdSpecificity)
                        .ThenBy(x => x.selector.ClassSpecificity)
                        .ThenBy(x => x.selector.SimpleSpecificity)
                        .ToArray();

                    var newMatchingStylesStrings = newMatchingStyles.Select(x => x.key).ToArray();
                    // Debug.WriteLine($"'{rule.SelectorString}' {GetPath(matchingNode)}: " + string.Join("|", newMatchingStylesStrings));
                    dependencyPropertyService.SetMatchingStyles(element, newMatchingStylesStrings);

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

        protected void CalculateStylesInternal(TUIElement styleResourceReferenceHolder, StyleSheet styleSheet, TUIElement startFrom)
        {
            if (styleResourceReferenceHolder == null ||
                styleSheet == null)
            {
                return;
            }

            // PrintHerarchyDebugInfo(styleResourceReferenceHolder, startFrom);

            var requiredStyleInfos = UpdateMatchingStyles(styleResourceReferenceHolder, styleSheet, startFrom);

            GenerateStyles(styleResourceReferenceHolder, styleSheet, startFrom, requiredStyleInfos);
        }
        
        public void RemoveStyleResources(TUIElement styleResourceReferenceHolder, StyleSheet styleSheet)
        {
            EnqueueRemoveStyleSheet(styleResourceReferenceHolder, styleSheet);
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

                CreateStyleDictionaryFromDeclarationBlockResult<TDependencyProperty> result = null;
                try
                {
                    result = CreateStyleDictionaryFromDeclarationBlock(
                        styleSheet.Namespaces,
                        styleMatchInfo.Rule.DeclarationBlock,
                        matchedElementType,
                        startFrom ?? styleResourceReferenceHolder);

                    var propertyStyleValues = result.PropertyStyleValues;

                    foreach (var error in result.Errors)
                    {
                        styleSheet.AddError($@"ERROR in Selector ""{styleMatchInfo.Rule.SelectorString}"": {error}");
                    }

                    var nativeTriggers = styleMatchInfo.Rule.DeclarationBlock.Triggers
                        .Select(x => nativeStyleService.CreateTrigger(styleSheet, x, styleMatchInfo.MatchedType, styleResourceReferenceHolder));

                    if (propertyStyleValues.Keys.Count == 0)
                    {
                        continue;
                    }

                    var style = nativeStyleService.CreateFrom(propertyStyleValues, nativeTriggers, matchedElementType);

                    applicationResourcesService.SetResource(resourceKey, style);
                }
                catch (Exception e)
                {
                    styleSheet.AddError($@"ERROR in Selector ""{styleMatchInfo.Rule.SelectorString}"": {e.Message}");
                }
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

            if (AppliedStyleIdsAreMatchedStyleIds(appliedMatchingStyles, matchingStyles))
            {
                dependencyPropertyService.SetHandledCss(visualElement, true);
                return;
            }

            object styleToApply = null;

            if (matchingStyles == null)
            {
                // RemoveOutdatedStylesFromElementInternal(visualElement, styleSheet, true, true);
            }
            else if (matchingStyles?.Length == 1)
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
            if (bindableObject == null ||
                dependencyPropertyService.GetHandledCss(bindableObject) == false)
            {
                return;
            }

            var currentStyleSheet = dependencyPropertyService.GetStyleSheet(bindableObject);
            if (currentStyleSheet != null &&
                (currentStyleSheet != styleSheet && 
                (styleSheet != null && currentStyleSheet.AttachedTo != styleSheet.AttachedTo)))
            {
                return;
            }

            foreach (var child in treeNodeProvider.GetChildren(bindableObject).ToList())
            {
                UnapplyMatchingStylesInternal(child, styleSheet);
            }

            dependencyPropertyService.SetHandledCss(bindableObject, false);
            dependencyPropertyService.SetMatchingStyles(bindableObject, null);
        }

        public void RemoveOutdatedStyles(TDependencyObject bindableObject, StyleSheet styleSheet, bool recursive, bool resetStyle)
        {
            uiInvoker(() =>
            {
                RemoveOutdatedStylesFromElementInternal(bindableObject, styleSheet, recursive, resetStyle);
            });
        }
        protected void RemoveOutdatedStylesFromElementInternal(TDependencyObject bindableObject, StyleSheet styleSheet, bool recursive, bool resetStyle)
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

            if (recursive)
            {
                foreach (var child in treeNodeProvider.GetChildren(bindableObject).ToList())
                {
                    RemoveOutdatedStylesFromElementInternal(child, styleSheet, recursive, resetStyle);
                }
            }

            var appliedMatchingStyles = dependencyPropertyService.GetAppliedMatchingStyles(bindableObject);
            var matchingStyles = dependencyPropertyService.GetMatchingStyles(bindableObject);

            if (!AppliedStyleIdsAreMatchedStyleIds(appliedMatchingStyles, matchingStyles))
            {
                dependencyPropertyService.SetAppliedMatchingStyles(bindableObject, null);

                if (resetStyle)
                {
                    nativeStyleService.SetStyle(bindableObject, dependencyPropertyService.GetInitialStyle(bindableObject));
                }
            }
        }

        private static bool AppliedStyleIdsAreMatchedStyleIds(string[] appliedMatchingStyles, string[] matchingStyles)
        {
            return matchingStyles == appliedMatchingStyles ||
                            (
                                matchingStyles != null &&
                                appliedMatchingStyles != null &&
                                matchingStyles.SequenceEqual(appliedMatchingStyles)
                            );
        }

        private CreateStyleDictionaryFromDeclarationBlockResult<TDependencyProperty> CreateStyleDictionaryFromDeclarationBlock(
            CssNamespaceCollection namespaces,
            StyleDeclarationBlock declarationBlock,
            Type matchedType,
            TDependencyObject dependencyObject)
        {
            var result = new CreateStyleDictionaryFromDeclarationBlockResult<TDependencyProperty>();

            foreach (var styleDeclaration in declarationBlock)
            {
                var propertyInfo = cssTypeHelper.GetDependencyPropertyInfo(namespaces, matchedType, styleDeclaration.Property);

                if (propertyInfo == null)
                {
                    continue;
                }

                try
                {
                    var propertyValue = cssTypeHelper.GetPropertyValue(propertyInfo.DeclaringType, dependencyObject, propertyInfo.Name, styleDeclaration.Value, propertyInfo.Property, namespaces);

                    result.PropertyStyleValues[propertyInfo.Property] = propertyValue;
                }
                catch
                {
                    result.Errors.Add($"Cannot get property-value for '{styleDeclaration.Property}' with value '{styleDeclaration.Value}'!");
                }
            }

            return result;
        }

        private TDependencyObject GetStyleSheetParent(TDependencyObject obj)
        {
            var currentBindableObject = obj;
            var oldSelectorType = treeNodeProvider.CurrentSelectorType;
            try
            {
                treeNodeProvider.Switch(SelectorType.LogicalTree);
                while (currentBindableObject != null)
                {
                    var styleSheet = dependencyPropertyService.GetStyleSheet(currentBindableObject);
                    if (styleSheet != null)
                    {
                        treeNodeProvider.Switch(oldSelectorType);
                        return currentBindableObject;
                    }

                    currentBindableObject = treeNodeProvider.GetParent(currentBindableObject as TUIElement);
                }
            }
            finally
            {
                treeNodeProvider.Switch(oldSelectorType);
            }

            return null;
        }
    }
}

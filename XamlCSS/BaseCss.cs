using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using XamlCSS.CssParsing;
using XamlCSS.Dom;
using XamlCSS.Utils;

namespace XamlCSS
{
    public class BaseCss<TDependencyObject, TStyle, TDependencyProperty>
        where TDependencyObject : class
        where TStyle : class
        where TDependencyProperty : class
    {
        public readonly IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService;
        public readonly ITreeNodeProvider<TDependencyObject, TDependencyProperty> treeNodeProvider;
        public readonly IStyleResourcesService applicationResourcesService;
        public readonly INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService;

        protected bool executeApplyStylesExecuting;

        private readonly IMarkupExtensionParser markupExpressionParser;
        private Action<Action> uiInvoker;
        private List<RenderInfo<TDependencyObject>> items = new List<RenderInfo<TDependencyObject>>();
        private CssTypeHelper<TDependencyObject, TDependencyProperty, TStyle> cssTypeHelper;
        private int noopCount = 0;
        private static readonly IList<ISelector> emptySelectorList = new ReadOnlyCollection<ISelector>(new List<ISelector>());

        public BaseCss(IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            ITreeNodeProvider<TDependencyObject, TDependencyProperty> treeNodeProvider,
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
            cssTypeHelper = new CssTypeHelper<TDependencyObject, TDependencyProperty, TStyle>(markupExpressionParser, dependencyPropertyService);

            CssParser.Initialize(defaultCssNamespace, fileProvider);
            StyleSheet.GetParent = parent => treeNodeProvider.GetParent((TDependencyObject)parent, SelectorType.VisualTree);
            StyleSheet.GetStyleSheet = treeNode => dependencyPropertyService.GetStyleSheet((TDependencyObject)treeNode);
        }

        public bool ExecuteApplyStyles()
        {
            if (executeApplyStylesExecuting)
            {
                return false;
            }

            if (items.Count == 0)
            {
                noopCount++;
                //executeApplyStylesExecuting = false;
                return false;
            }


            uiInvoker(() =>
            {
                executeApplyStylesExecuting = true;

                List<RenderInfo<TDependencyObject>> copy;

                //lock (items)
                {
                

                    copy = items.Distinct().ToList();
                    items.Clear();
                }

                noopCount = 0;
                Render(copy, treeNodeProvider, dependencyPropertyService, nativeStyleService, applicationResourcesService, cssTypeHelper);
                executeApplyStylesExecuting = false;
            });

            //Investigate.Print();
            // HierarchyDebugExtensions.PrintHerarchyDebugInfo(treeNodeProvider, dependencyPropertyService, copy.First().StyleSheetHolder, copy.First().StyleSheetHolder, SelectorType.LogicalTree);

            return true;
        }

        private static void RemoveStyleResourcesInternal(
            TDependencyObject styleResourceReferenceHolder,
            StyleSheet styleSheet,
            IStyleResourcesService applicationResourcesService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService)
        {
            var resourceKey = $"{nativeStyleService.BaseStyleResourceKey}_{styleSheet.Id}";
            var resourceKeys = applicationResourcesService.GetKeys()
                .OfType<string>()
                .Where(x => x.StartsWith(resourceKey, StringComparison.Ordinal))
                .ToList();

            foreach (var key in resourceKeys)
            {
                applicationResourcesService.RemoveResource(key);
            }
        }

        public static void Render(
            List<RenderInfo<TDependencyObject>> copy,
            ITreeNodeProvider<TDependencyObject, TDependencyProperty> treeNodeProvider,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService,
            IStyleResourcesService applicationResourcesService,
            CssTypeHelper<TDependencyObject, TDependencyProperty, TStyle> cssTypeHelper)
        {
            try
            {
                applicationResourcesService.BeginUpdate();

                RemoveOldStyleObjects(copy, nativeStyleService, applicationResourcesService);
                SetAttachedToToNull(copy, dependencyPropertyService, treeNodeProvider, nativeStyleService);
                SetAttachedToToNewStyleSheet(copy, dependencyPropertyService, treeNodeProvider, nativeStyleService);
                RemoveRemovedDomElements(copy, treeNodeProvider, nativeStyleService);

                var styleUpdateInfos = new Dictionary<TDependencyObject, StyleUpdateInfo>();

                var newOrUpdatedStyleSheets = copy
                   .Where(x =>
                   x.RenderTargetKind == RenderTargetKind.Stylesheet)
                   .Select(x => x.StyleSheet)
                   .Distinct()
                   .ToHashSet();

                var newOrUpdatedStyleHolders = copy
                    .Where(x =>
                        x.ChangeKind == ChangeKind.New ||
                        x.ChangeKind == ChangeKind.Update ||
                        x.ChangeKind == ChangeKind.Remove)
                    .Select(x => new { x.ChangeKind, x.StartFrom, x.StyleSheet, x.StyleSheetHolder })
                    .Distinct()
                    .ToHashSet();

                foreach (var item in newOrUpdatedStyleHolders)
                {
                    var start = item.StartFrom ?? item.StyleSheetHolder;

                    var domElement = treeNodeProvider.GetDomElement(start);
                    if (domElement.IsInLogicalTree != true)
                    {
                        continue;
                    }

                    EnsureParents(domElement, treeNodeProvider, dependencyPropertyService, nativeStyleService, styleUpdateInfos, SelectorType.LogicalTree);

                    var discardOldMatchingStyles = newOrUpdatedStyleSheets.Contains(item.StyleSheet);
                    SetupStyleInfo(domElement, item.StyleSheet, styleUpdateInfos, treeNodeProvider, dependencyPropertyService, nativeStyleService, discardOldMatchingStyles, item.ChangeKind == ChangeKind.Remove, SelectorType.LogicalTree);
                }

                foreach (var item in newOrUpdatedStyleHolders)
                {
                    var start = item.StartFrom ?? item.StyleSheetHolder;

                    var domElement = treeNodeProvider.GetDomElement(start);
                    if (domElement.IsInVisualTree != true)
                    {
                        continue;
                    }

                    EnsureParents(domElement, treeNodeProvider, dependencyPropertyService, nativeStyleService, styleUpdateInfos, SelectorType.VisualTree);

                    var discardOldMatchingStyles = newOrUpdatedStyleSheets.Contains(item.StyleSheet);
                    SetupStyleInfo(domElement, item.StyleSheet, styleUpdateInfos, treeNodeProvider, dependencyPropertyService, nativeStyleService, discardOldMatchingStyles, item.ChangeKind == ChangeKind.Remove, SelectorType.VisualTree);
                }

                var results = new List<IList<IDomElement<TDependencyObject, TDependencyProperty>>>();
                var distinctCopy = copy.Select(x => new { x.StartFrom, x.StyleSheetHolder, x.StyleSheet }).Distinct().ToList();

                foreach (var item in distinctCopy)
                {
                    var start = item.StartFrom ?? item.StyleSheetHolder;

                    if (!styleUpdateInfos.ContainsKey(start))
                    {
                        continue;
                    }

                    var domElement = treeNodeProvider.GetDomElement(start);

                    var result = UpdateMatchingStyles(item.StyleSheet, domElement, styleUpdateInfos, dependencyPropertyService, nativeStyleService);
                    results.Add(result);
                }

                var allFound = results.SelectMany(x => x);
                var allFoundElements = allFound.Select(x => x.Element).ToHashSet();
                var allNotFoundKeys = styleUpdateInfos
                    .Where(x => !allFoundElements.Contains(x.Key))
                    .ToList();

                foreach (var item in allNotFoundKeys)
                {
                    var styleUpdateInfo = item.Value;

                    styleUpdateInfo.OldMatchedSelectors = emptySelectorList;
                    styleUpdateInfo.DoMatchCheck = SelectorType.None;
                    // remove style
                    var initialStyle = (TStyle)styleUpdateInfo.InitialStyle;

                    nativeStyleService.SetStyle(item.Key, initialStyle);
                }

                var groups = styleUpdateInfos.Where(x => allFoundElements.Contains(x.Key)).GroupBy(x => x.Value.CurrentStyleSheet).ToList();
                foreach (var group in groups)
                {
                    GenerateStyles(
                        group.Key,
                        group.ToDictionary(x => x.Key, x => x.Value),
                        applicationResourcesService,
                        dependencyPropertyService,
                        nativeStyleService,
                        cssTypeHelper);
                }

                foreach (var f in allFound)
                {
                    ApplyMatchingStylesNode(f, applicationResourcesService, nativeStyleService);
                }
            }
            finally
            {
                applicationResourcesService.EndUpdate();
            }

        }
        private static void ReevaluateStylesheetInSubTree(IDomElement<TDependencyObject, TDependencyProperty> domElement, StyleSheet oldStyleSheet,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService, StyleSheet newStyleSheet)
        {
            if (domElement.StyleInfo == null)
            {
                return;
            }

            if (domElement == null ||
                !ReferenceEquals(domElement.StyleInfo.CurrentStyleSheet, oldStyleSheet))
            {
                return;
            }

            domElement.StyleInfo.CurrentStyleSheet = newStyleSheet; //GetStyleSheetFromTree(domElement, dependencyPropertyService);
            domElement.ClearAttributeWatcher();

            foreach (var child in domElement.LogicalChildNodes.Concat(domElement.ChildNodes).Distinct())
            {
                ReevaluateStylesheetInSubTree(child, oldStyleSheet, dependencyPropertyService, nativeStyleService, newStyleSheet);
            }
        }

        private static void SetAttachedToToNewStyleSheet(List<RenderInfo<TDependencyObject>> copy,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            ITreeNodeProvider<TDependencyObject, TDependencyProperty> treeNodeProvider,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService)
        {
            var addedStyleSheets = copy
                            .Where(x => x.RenderTargetKind == RenderTargetKind.Stylesheet &&
                                x.ChangeKind == ChangeKind.New)
                            .Select(x => new { x.StyleSheet, x.StyleSheetHolder })
                            .Distinct()
                            .ToList();

            foreach (var item in addedStyleSheets)
            {
                var domElement = treeNodeProvider.GetDomElement(item.StyleSheetHolder);

                item.StyleSheet.AttachedTo = item.StyleSheetHolder;

                ReevaluateStylesheetInSubTree(domElement, domElement.StyleInfo?.CurrentStyleSheet, dependencyPropertyService, nativeStyleService, item.StyleSheet);
            }
        }

        private static void SetAttachedToToNull(List<RenderInfo<TDependencyObject>> copy,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            ITreeNodeProvider<TDependencyObject, TDependencyProperty> treeNodeProvider,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService)
        {
            var removedStyleSheets = copy
                            .Where(x =>
                                x.RenderTargetKind == RenderTargetKind.Stylesheet &&
                                x.ChangeKind == ChangeKind.Remove)
                            .Select(x => x.StyleSheet)
                            .Distinct()
                            .ToList();

            foreach (var removedStyleSheet in removedStyleSheets)
            {
                var domElement = treeNodeProvider.GetDomElement((TDependencyObject)removedStyleSheet.AttachedTo);
                StyleSheet newStyleSheet = null;// GetStyleSheetFromTree(domElement, dependencyPropertyService);

                ReevaluateStylesheetInSubTree(domElement, removedStyleSheet, dependencyPropertyService, nativeStyleService, newStyleSheet);

                removedStyleSheet.AttachedTo = null;
            }
        }

        private static void RemoveRemovedDomElements(List<RenderInfo<TDependencyObject>> copy,
            ITreeNodeProvider<TDependencyObject, TDependencyProperty> treeNodeProvider, INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService)
        {
            var updatedOrRemovedStyleSheets = copy
                            .Where(x => x.RenderTargetKind == RenderTargetKind.Element)
                            .Where(x =>
                                x.ChangeKind == ChangeKind.Remove)
                            .Select(x => x.StartFrom)
                            .Distinct()
                            .ToList();

            foreach (var item in updatedOrRemovedStyleSheets)
            {
                var domElement = treeNodeProvider.GetDomElement(item);
                if (domElement.StyleInfo is object)
                {
                    nativeStyleService.SetStyle(item, (TStyle)domElement.StyleInfo.InitialStyle);
                    domElement.StyleInfo = null;
                }

                RemoveRemovedDomElementsInternal(domElement, nativeStyleService);
            }
        }

        private static void RemoveRemovedDomElementsInternal(IDomElement<TDependencyObject, TDependencyProperty> domElement, INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService)
        {
            foreach (var item in domElement.ChildNodes)
            {
                if (item.StyleInfo is object)
                {
                    nativeStyleService.SetStyle(item.Element, (TStyle)item.StyleInfo.InitialStyle);
                    item.StyleInfo = null;
                }

                RemoveRemovedDomElementsInternal(item, nativeStyleService);
            }
        }

        private static void RemoveOldStyleObjects(List<RenderInfo<TDependencyObject>> copy,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService, IStyleResourcesService applicationResourcesService)
        {
            var updatedOrRemovedStyleSheets = copy
                            .Where(x => x.RenderTargetKind == RenderTargetKind.Stylesheet)
                            .Where(x =>
                                x.ChangeKind == ChangeKind.Update ||
                                x.ChangeKind == ChangeKind.Remove)
                            .Select(x => new { x.StyleSheet, x.StyleSheetHolder })
                            .Distinct()
                            .ToList();

            foreach (var item in updatedOrRemovedStyleSheets)
            {
                RemoveStyleResourcesInternal(item.StyleSheetHolder, item.StyleSheet, applicationResourcesService, nativeStyleService);

            }
        }

        private static void EnsureParents(IDomElement<TDependencyObject, TDependencyProperty> domElement, ITreeNodeProvider<TDependencyObject, TDependencyProperty> treeNodeProvider,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService,
            IDictionary<TDependencyObject, StyleUpdateInfo> styleUpdateInfos, SelectorType type)
        {
            var current = type == SelectorType.VisualTree ? domElement.Parent : domElement.LogicalParent;
            while (current != null &&
                current.IsReady)
            {
                var styleUpdateInfo = current.StyleInfo = current.StyleInfo ?? (styleUpdateInfos.ContainsKey(current.Element) ? styleUpdateInfos[current.Element] :
                    GetNewStyleUpdateInfo(current, dependencyPropertyService, nativeStyleService));

                if ((styleUpdateInfo.DoMatchCheck & type) != type)
                {
                    return;
                }

                object a;
                a = current.ClassList;
                //"Id".Measure(() => a = current.Id);
                //a = current.TagName;
                //a = current.AssemblyQualifiedNamespaceName;
                //"HasAttribute".Measure(() => a = current.HasAttribute("Name"));
                /*// a = domElement.Parent;
                */

                if (type == SelectorType.VisualTree)
                {
                    a = current.ChildNodes;
                    current = current.Parent;
                }
                else
                {
                    a = current.LogicalChildNodes;
                    current = current.LogicalParent;
                }
            }
        }

        private static bool AppliedStyleIdsAreMatchedStyleIds(IList<ISelector> appliedMatchingStyles, IList<ISelector> matchingStyles)
        {
            return matchingStyles == appliedMatchingStyles ||
                            (
                                matchingStyles != null &&
                                appliedMatchingStyles != null &&
                                matchingStyles.SequenceEqual(appliedMatchingStyles)
                            );
        }

        private static void ApplyMatchingStylesNode(IDomElement<TDependencyObject, TDependencyProperty> domElement,
           IStyleResourcesService applicationResourcesService,
           INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService)
        {
            var visualElement = domElement.Element;

            var matchingStyles = domElement.StyleInfo.CurrentMatchedSelectors;
            var appliedMatchingStyles = domElement.StyleInfo.OldMatchedSelectors;
            var matchingResourceKeys = domElement.StyleInfo.CurrentMatchedResourceKeys;

            if (!AppliedStyleIdsAreMatchedStyleIds(appliedMatchingStyles, matchingStyles))
            {
                object styleToApply = null;

                if (matchingResourceKeys == null)
                {
                }
                else if (matchingResourceKeys.Count == 1)
                {
                    if (applicationResourcesService.Contains(matchingResourceKeys.First()) == true)
                    {
                        styleToApply = applicationResourcesService.GetResource(matchingResourceKeys.First());
                    }

                    try
                    {
                        if (styleToApply != null)
                        {
                            nativeStyleService.SetStyle(visualElement, (TStyle)styleToApply);
                            domElement.StyleInfo.OldMatchedSelectors = matchingStyles.ToList();
                        }
                        else
                        {
                            nativeStyleService.SetStyle(visualElement, null);
                            domElement.StyleInfo.OldMatchedSelectors = emptySelectorList;
                            // Debug.WriteLine("    Style not found! " + matchingResourceKeys[0]);
                        }
                    }
                    catch (Exception exc)
                    {
                        applicationResourcesService.RemoveResource(matchingResourceKeys.First());
                        domElement.StyleInfo.OldMatchedSelectors = emptySelectorList;
                        domElement.XamlCssStyleSheets.First().AddError($"Cannot apply style to an element matching {string.Join(", ", matchingStyles.Select(x => x.Value))}: {exc.Message}");
                        nativeStyleService.SetStyle(visualElement, null);
                        return;
                    }
                }
                else if (matchingResourceKeys.Count > 1)
                {
                    var resourceKey = string.Join(", ", matchingResourceKeys);
                    if (applicationResourcesService.Contains(resourceKey))
                    {
                        try
                        {
                            nativeStyleService.SetStyle(visualElement, (TStyle)applicationResourcesService.GetResource(resourceKey));
                            domElement.StyleInfo.OldMatchedSelectors = matchingStyles.ToList();
                        }
                        catch (Exception exc)
                        {
                            applicationResourcesService.RemoveResource(resourceKey);
                            domElement.StyleInfo.OldMatchedSelectors = emptySelectorList;
                            domElement.XamlCssStyleSheets.First().AddError($"Cannot apply style to an element matching {string.Join(", ", matchingStyles.Select(x => x.Value))}: {exc.Message}");
                            nativeStyleService.SetStyle(visualElement, null);
                            return;
                        }
                    }
                    else
                    {
                        var dict = new Dictionary<TDependencyProperty, object>();
                        var listTriggers = new List<TDependencyObject>();

                        foreach (var matchingResourceKey in matchingResourceKeys)
                        {
                            TStyle s = null;
                            if (applicationResourcesService.Contains(matchingResourceKey) == true)
                            {
                                s = (TStyle)applicationResourcesService.GetResource(matchingResourceKey);
                            }
                            else
                            {
                                // Debug.WriteLine("    Style not found! " + matchingStyle);
                            }

                            if (s != null)
                            {
                                var subDict = nativeStyleService.GetStyleAsDictionary(s);

                                foreach (var i in subDict)
                                {
                                    dict[i.Key] = i.Value;
                                }

                                var triggers = nativeStyleService.GetTriggersAsList(s);
                                listTriggers.AddRange(triggers);
                            }
                        }

                        if (dict.Keys.Count > 0 ||
                            listTriggers.Count > 0)
                        {
                            styleToApply = nativeStyleService.CreateFrom(dict, listTriggers, visualElement.GetType());
                        }

                        try
                        {

                            if (styleToApply != null)
                            {
                                nativeStyleService.SetStyle(visualElement, (TStyle)styleToApply);
                                applicationResourcesService.SetResource(resourceKey, styleToApply);
                            }
                            else
                            {
                                nativeStyleService.SetStyle(visualElement, null);
                            }
                            domElement.StyleInfo.OldMatchedSelectors = matchingStyles.ToList();
                        }
                        catch (Exception exc)
                        {
                            applicationResourcesService.RemoveResource(resourceKey);
                            domElement.StyleInfo.OldMatchedSelectors = emptySelectorList;
                            nativeStyleService.SetStyle(visualElement, null);
                            domElement.XamlCssStyleSheets.First().AddError($"Cannot apply style to an element matching {string.Join(", ", matchingStyles.Select(x => x.Value))}: {exc.Message}");
                            return;
                        }
                    }
                }
                else
                {
                    domElement.StyleInfo.OldMatchedSelectors = matchingStyles.ToList();
                }
            }
        }

        private static IList<IDomElement<TDependencyObject, TDependencyProperty>> UpdateMatchingStyles(
            StyleSheet styleSheet,
            IDomElement<TDependencyObject, TDependencyProperty> startFrom,
            Dictionary<TDependencyObject, StyleUpdateInfo> styleUpdateInfos,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService)
        {
            // var requiredStyleInfos = new List<StyleMatchInfo>();
            var found = new HashSet<IDomElement<TDependencyObject, TDependencyProperty>>();
            if (startFrom == null ||
                !startFrom.IsReady)
            {
                return found.ToList();
            }

            if (startFrom.StyleInfo.DoMatchCheck == SelectorType.None)
            {
                return new List<IDomElement<TDependencyObject, TDependencyProperty>>();
            }

            startFrom.XamlCssStyleSheets.Clear();
            startFrom.XamlCssStyleSheets.Add(styleSheet);

            var traversed = SelectorType.None;

            foreach (var rule in styleSheet.Rules)
            {
                // // Debug.WriteLine($"--- RULE {rule.SelectorString} ----");

                // apply our selector

                var type = SelectorType.LogicalTree;
                if (rule.Selectors[0].StartOnVisualTree())
                {
                    type = SelectorType.VisualTree;
                }

                if ((type == SelectorType.LogicalTree && startFrom.IsInLogicalTree != true) ||
                    (type == SelectorType.VisualTree && startFrom.IsInVisualTree != true)
                )
                {
                    continue;
                }

                traversed |= type;

                var matchedNodes = startFrom.QuerySelectorAllWithSelf(styleSheet, rule.Selectors[0], type)
                        .Where(x => x != null)
                        .Cast<IDomElement<TDependencyObject, TDependencyProperty>>()
                        .ToList();

                var matchedElementTypes = matchedNodes
                    .Select(x => x.Element.GetType())
                    .Distinct()
                    .ToList();

                foreach (var matchingNode in matchedNodes)
                {
                    var element = matchingNode.Element;

                    if (!found.Contains(matchingNode))
                    {
                        found.Add(matchingNode);
                    }

                    matchingNode.StyleInfo.CurrentMatchedSelectors.Add(rule.Selectors[0]);

                    var initialStyle = (TStyle)matchingNode.StyleInfo.InitialStyle;//dependencyPropertyService.GetInitialStyle(element);
                    var discriminator = initialStyle != null ? initialStyle.GetHashCode().ToString() : "";
                    var resourceKey = nativeStyleService.GetStyleResourceKey(styleSheet.Id + discriminator, element.GetType(), rule.SelectorString);
                    //var resourceKey = nativeStyleService.GetStyleResourceKey(rule.StyleSheetId, element.GetType(), rule.SelectorString);

                    if (!matchingNode.StyleInfo.CurrentMatchedResourceKeys.Contains(resourceKey))
                    {
                        matchingNode.StyleInfo.CurrentMatchedResourceKeys.Add(resourceKey);
                    }
                }
            }

            foreach (var f in found)
            {
                f.StyleInfo.DoMatchCheck &= ~traversed;

                f.StyleInfo.CurrentMatchedSelectors = f.StyleInfo.CurrentMatchedSelectors.Distinct()
                        .OrderBy(x => x.IdSpecificity)
                        .ThenBy(x => x.ClassSpecificity)
                        .ThenBy(x => x.SimpleSpecificity)
                        .ToList()
                        //.ToLinkedHashSet()
                        ;
            }

            if ((traversed & SelectorType.VisualTree) > 0)
            {
                MarkAsAlreadyProcessedForSelectorTypeInSubTree(startFrom, styleSheet, SelectorType.VisualTree);
            }

            if ((traversed & SelectorType.LogicalTree) > 0)
            {
                MarkAsAlreadyProcessedForSelectorTypeInSubTree(startFrom, styleSheet, SelectorType.LogicalTree);
            }

            return found.ToList();
        }

        private static void MarkAsAlreadyProcessedForSelectorTypeInSubTree(IDomElement<TDependencyObject, TDependencyProperty> domElement, StyleSheet styleSheet, SelectorType type)
        {
            if (domElement == null ||
                !domElement.IsReady ||
                !ReferenceEquals(domElement.StyleInfo.CurrentStyleSheet, styleSheet))
            {
                return;
            }

            domElement.StyleInfo.DoMatchCheck &= ~type;

            var children = type == SelectorType.VisualTree ? domElement.ChildNodes : domElement.LogicalChildNodes;
            foreach (var child in children)
            {
                MarkAsAlreadyProcessedForSelectorTypeInSubTree(child, styleSheet, type);
            }
        }

        private static StyleSheet GetStyleSheetFromTree(IDomElement<TDependencyObject, TDependencyProperty> domElement,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService)
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

        private static void SetupStyleInfo(
            IDomElement<TDependencyObject, TDependencyProperty> domElement,
            StyleSheet styleSheet,
            Dictionary<TDependencyObject, StyleUpdateInfo> styleUpdateInfos,
            ITreeNodeProvider<TDependencyObject, TDependencyProperty> treeNodeProvider,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService,
            bool styleChanged,
            bool styleSheetRemoved, SelectorType type)
        {
            if (!domElement.IsReady)
            {
                return;
            }

            var styleUpdateInfo = domElement.StyleInfo = domElement.StyleInfo ?? (styleUpdateInfos.ContainsKey(domElement.Element) ? styleUpdateInfos[domElement.Element] :
                GetNewStyleUpdateInfo(domElement, dependencyPropertyService, nativeStyleService));

            styleUpdateInfos[domElement.Element] = styleUpdateInfo;

            var styleSheetFromDom = dependencyPropertyService.GetStyleSheet(domElement.Element);
            if (styleSheetFromDom != null &&
                styleSheetFromDom != styleSheet)
            {
                // another stylesheet's domelement
                SetupStyleInfo(domElement, styleSheetFromDom, styleUpdateInfos, treeNodeProvider, dependencyPropertyService, nativeStyleService, styleChanged, false, type);
                return;
            }

            if (!styleSheetRemoved)
            {
                styleUpdateInfo.CurrentStyleSheet = styleSheet;
            }

            if (styleChanged)
            {
                styleUpdateInfo.OldMatchedSelectors = null;
            }
            styleUpdateInfo.CurrentMatchedSelectors.Clear();
            styleUpdateInfo.CurrentMatchedResourceKeys.Clear();
            styleUpdateInfo.DoMatchCheck |= type;

            var children = type == SelectorType.VisualTree ? domElement.ChildNodes : domElement.LogicalChildNodes;
            foreach (var child in children)
            {
                SetupStyleInfo(child, styleSheet, styleUpdateInfos, treeNodeProvider, dependencyPropertyService, nativeStyleService, styleChanged, styleSheetRemoved, type);
            }
        }

        private static StyleUpdateInfo GetNewStyleUpdateInfo(IDomElement<TDependencyObject, TDependencyProperty> domElement,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService)
        {
            var initialStyle = nativeStyleService.GetStyle(domElement.Element);

            return new StyleUpdateInfo
            {
                MatchedType = domElement.Element.GetType(),
                InitialStyle = initialStyle
            };
        }

        private static void GenerateStyles(StyleSheet styleSheet,
            IDictionary<TDependencyObject, StyleUpdateInfo> styleMatchInfos,
            IStyleResourcesService applicationResourcesService,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService,
            CssTypeHelper<TDependencyObject, TDependencyProperty, TStyle> cssTypeHelper)
        {
            applicationResourcesService.EnsureResources();

            foreach (var styleMatchInfoKeyValue in styleMatchInfos)
            {
                var styleMatchInfo = styleMatchInfoKeyValue.Value;

                var matchedElementType = styleMatchInfo.MatchedType;

                for (var i = 0; i < styleMatchInfo.CurrentMatchedSelectors.Count; i++)
                {
                    var selector = styleMatchInfo.CurrentMatchedSelectors.ElementAt(i);
                    var resourceKey = styleMatchInfo.CurrentMatchedResourceKeys.ElementAt(i);

                    if (applicationResourcesService.Contains(resourceKey))
                    {
                        // Debug.WriteLine($"GenerateStyles: Already contains '{s}' ({matchedElementType.Name})");
                        continue;
                    }

                    // Debug.WriteLine($"GenerateStyles: Generating '{s}' ({matchedElementType.Name})");

                    var rule = styleMatchInfo.CurrentStyleSheet.Rules.Where(x => x.SelectorString == selector.Value).First();

                    CreateStyleDictionaryFromDeclarationBlockResult<TDependencyProperty> result = null;
                    try
                    {
                        result = CreateStyleDictionaryFromDeclarationBlock(
                            styleSheet.Namespaces,
                            rule.DeclarationBlock,
                            matchedElementType,
                            (TDependencyObject)styleSheet.AttachedTo,
                            cssTypeHelper);

                        var propertyStyleValues = result.PropertyStyleValues;

                        foreach (var error in result.Errors)
                        {
                            // Debug.WriteLine($@" ERROR (normal) in Selector ""{rule.SelectorString}"": {error}");
                            styleSheet.AddError($@"ERROR in Selector ""{rule.SelectorString}"": {error}");
                        }

                        var nativeTriggers = rule.DeclarationBlock.Triggers
                            .Select(x => nativeStyleService.CreateTrigger(styleSheet, x, styleMatchInfo.MatchedType, (TDependencyObject)styleSheet.AttachedTo))
                            .ToList();

                        var initalStyle = styleMatchInfo.InitialStyle;// dependencyPropertyService.GetInitialStyle(styleMatchInfoKeyValue.Key);
                        if (initalStyle != null)
                        {
                            var subDict = nativeStyleService.GetStyleAsDictionary(initalStyle as TStyle);

                            foreach (var item in subDict)
                            {
                                // only set not-overridden properties
                                if (!propertyStyleValues.ContainsKey(item.Key))
                                {
                                    propertyStyleValues[item.Key] = item.Value;
                                }
                            }

                            var triggers = nativeStyleService.GetTriggersAsList(initalStyle as TStyle);
                            nativeTriggers.InsertRange(0, triggers);
                        }
                        //Debug.WriteLine("    Values: " + string.Join(", ", propertyStyleValues.Select(x => ((dynamic)x.Key).PropertyName + ": " + x.Value.ToString())));
                        var style = nativeStyleService.CreateFrom(propertyStyleValues, nativeTriggers, matchedElementType);

                        applicationResourcesService.SetResource(resourceKey, style);

                        // Debug.WriteLine("Finished generate Style " + resourceKey);
                    }
                    catch (Exception e)
                    {
                        // Debug.WriteLine($@" ERROR (exception) in Selector ""{rule.SelectorString}"": {e.Message}");
                        styleSheet.AddError($@"ERROR in Selector ""{rule.SelectorString}"": {e.Message}");
                    }
                }
            }
        }

        private static CreateStyleDictionaryFromDeclarationBlockResult<TDependencyProperty> CreateStyleDictionaryFromDeclarationBlock(
            CssNamespaceCollection namespaces,
            StyleDeclarationBlock declarationBlock,
            Type matchedType,
            TDependencyObject dependencyObject,
            CssTypeHelper<TDependencyObject, TDependencyProperty, TStyle> cssTypeHelper)
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

        public void EnqueueRenderStyleSheet(TDependencyObject styleSheetHolder, StyleSheet styleSheet)
        {
            if (styleSheetHolder == null ||
                styleSheet == null)
            {
                return;
            }

            lock (items)
            {
                items.Add(new RenderInfo<TDependencyObject>
                {
                    RenderTargetKind = RenderTargetKind.Stylesheet,
                    ChangeKind = ChangeKind.New,
                    StyleSheetHolder = styleSheetHolder,
                    StyleSheet = styleSheet,
                    StartFrom = null
                });
            }
        }

        public void EnqueueUpdateStyleSheet(TDependencyObject styleSheetHolder, StyleSheet styleSheet)
        {
            if (styleSheetHolder == null ||
                styleSheet == null)
            {
                return;
            }

            lock (items)
            {
                items.Add(new RenderInfo<TDependencyObject>
                {
                    RenderTargetKind = RenderTargetKind.Stylesheet,
                    ChangeKind = ChangeKind.Update,
                    StyleSheetHolder = styleSheetHolder,
                    StyleSheet = styleSheet,
                    StartFrom = null
                });
            }
        }

        public void EnqueueUpdateElement(TDependencyObject styleSheetHolder, StyleSheet styleSheet, TDependencyObject startFrom)
        {
            if (styleSheetHolder == null ||
                styleSheet == null)
            {
                return;
            }

            lock (items)
            {
                items.Add(new RenderInfo<TDependencyObject>
                {
                    RenderTargetKind = RenderTargetKind.Element,
                    ChangeKind = ChangeKind.Update,
                    StyleSheetHolder = styleSheetHolder,
                    StyleSheet = styleSheet,
                    StartFrom = startFrom
                });
            }
        }

        public void EnqueueNewElement(TDependencyObject styleSheetHolder, StyleSheet styleSheet, TDependencyObject startFrom)
        {
            if (styleSheetHolder == null ||
                styleSheet == null)
            {
                return;
            }

            lock (items)
            {
                items.Add(new RenderInfo<TDependencyObject>
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

            var parent = GetStyleSheetParent(sender);
            if (parent == null)
            {
                return;
            }

            EnqueueNewElement(
                parent,
                dependencyPropertyService.GetStyleSheet(parent),
                sender);
        }

        public void EnqueueRemoveElement(TDependencyObject styleSheetHolder, StyleSheet styleSheet, TDependencyObject startFrom)
        {
            if (styleSheetHolder == null ||
                styleSheet == null)
            {
                return;
            }

            lock (items)
            {
                items.Add(new RenderInfo<TDependencyObject>
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

            var parent = GetStyleSheetParent(sender);
            if (parent == null)
            {
                return;
            }

            EnqueueRemoveElement(
                parent,
                dependencyPropertyService.GetStyleSheet(parent),
                sender);
        }

        public void UpdateElement(TDependencyObject sender)
        {
            if (sender == null)
            {
                return;
            }

            var parent = GetStyleSheetParent(sender);
            if (parent == null)
            {
                return;
            }

            EnqueueUpdateElement(
                parent,
                dependencyPropertyService.GetStyleSheet(parent),
                sender);
        }

        public void EnqueueRemoveStyleSheet(TDependencyObject styleSheetHolder, StyleSheet styleSheet)
        {
            if (styleSheetHolder == null ||
                styleSheet == null)
            {
                return;
            }

            lock (items)
            {
                items.Add(new RenderInfo<TDependencyObject>
                {
                    RenderTargetKind = RenderTargetKind.Stylesheet,
                    ChangeKind = ChangeKind.Remove,
                    StyleSheetHolder = styleSheetHolder,
                    StyleSheet = styleSheet,
                    StartFrom = null
                });
            }
        }

        private StyleSheet GetStyleSheetFromTree(IDomElement<TDependencyObject, TDependencyProperty> domElement)
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
            var currentDependencyObject = obj;

            try
            {
                while (currentDependencyObject != null)
                {
                    var styleSheet = dependencyPropertyService.GetStyleSheet(currentDependencyObject);
                    if (!ReferenceEquals(styleSheet, null))
                    {
                        return currentDependencyObject;
                    }

                    // TODO: Only Logical Tree?
                    currentDependencyObject = treeNodeProvider.GetParent(currentDependencyObject, SelectorType.LogicalTree);
                }
            }
            finally
            {
            }

            return null;
        }
    }
}

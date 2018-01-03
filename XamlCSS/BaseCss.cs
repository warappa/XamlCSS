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

                //applicationResourcesService.BeginUpdate();

                BaseCSS2<TDependencyObject, TUIElement, TStyle, TDependencyProperty>
                    .Render(copy, treeNodeProvider, dependencyPropertyService, nativeStyleService, applicationResourcesService, cssTypeHelper);
                return;
                treeNodeProvider.Switch(SelectorType.VisualTree);

                // Debug.WriteLine("-------------------------------------------------");
                // Debug.WriteLine("-------------- NEW FRAME RENDERING --------------");

                // Debug.WriteLine("" + copy.Count);
                /* Debug.WriteLine("" + string.Join("\n", copy
                    .GroupBy(x => new { x.ChangeKind, x.RenderTargetKind, x.StyleSheet, x.StartFrom })
                    .Select(x => x.Key)
                    .Select(x => x.ChangeKind + " " + x.RenderTargetKind + " " + x.StyleSheet.Id + " " + treeNodeProvider.GetDomElement(x.StartFrom)?.GetPath()).Distinct()));
                    */
                //treeNodeProvider.PrintHerarchyDebugInfo(dependencyPropertyService, copy.First().StyleSheetHolder, copy.First().StyleSheetHolder);

                var removeOrUpdateStylesheetInfos = copy
                    .Where(x =>
                        x.RenderTargetKind == RenderTargetKind.Stylesheet &&
                        (x.ChangeKind == ChangeKind.Remove || x.ChangeKind == ChangeKind.Update))
                    //.GroupBy(x => x.StyleSheetHolder)
                    //.Select(x => x.First())
                    .ToList();

                var handledStyleSheetHolders = removeOrUpdateStylesheetInfos
                    .Select(x => x.StyleSheetHolder)
                    .ToList();

                // Debug.WriteLine("removeOrUpdateStylesheetInfos: " + removeOrUpdateStylesheetInfos.Count);
                foreach (var removeOrUpdateStylesheetInfo in removeOrUpdateStylesheetInfos)
                {
                    treeNodeProvider.Switch(SelectorType.VisualTree);

                    // Debug.WriteLine("removeOrUpdateStylesheetInfo UnapplyMatchingStylesInternal:" + treeNodeProvider.GetDomElement(removeOrUpdateStylesheetInfo.StyleSheetHolder).GetPath());
                    // sets HandledCss and MatchingStyles false/null
                    UnapplyMatchingStylesInternal(removeOrUpdateStylesheetInfo.StyleSheetHolder, removeOrUpdateStylesheetInfo.StyleSheet);

                    // compares MatchingStyles and AppliedMachingStyles and 
                    // sets AppliedMatchingStyles and Style null
                    // Debug.WriteLine("removeOrUpdateStylesheetInfo RemoveOutdatedStylesFromElementInternal:" + treeNodeProvider.GetDomElement(removeOrUpdateStylesheetInfo.StyleSheetHolder).GetPath());
                    ResetElementsBecauseStyleSheetChangedInternal(removeOrUpdateStylesheetInfo.StyleSheetHolder, removeOrUpdateStylesheetInfo.StyleSheet, true);

                    // Debug.WriteLine("removeOrUpdateStylesheetInfo RemoveStyleResourcesInternal:" + treeNodeProvider.GetDomElement(removeOrUpdateStylesheetInfo.StyleSheetHolder).GetPath());
                    // remove Style resources of Stylesheet
                    RemoveStyleResourcesInternal(removeOrUpdateStylesheetInfo.StyleSheetHolder, removeOrUpdateStylesheetInfo.StyleSheet);

                    if (removeOrUpdateStylesheetInfo.ChangeKind == ChangeKind.Remove)
                    {
                        removeOrUpdateStylesheetInfo.StyleSheet.AttachedTo = null;
                    }
                }

                // Debug.WriteLine("-------------------------------------------------");

                var removeOrUpdateElementInfos = copy
                    .Where(x =>
                        x.RenderTargetKind == RenderTargetKind.Element &&
                        (x.ChangeKind == ChangeKind.Remove || x.ChangeKind == ChangeKind.Update))
                    //.GroupBy(x => x.StyleSheetHolder)
                    //.Select(x => x.First())
                    .ToList();

                foreach (var removedOrUpdateElementInfo in removeOrUpdateElementInfos)
                {
                    // Debug.WriteLine("removeOrUpdateElementInfos UnapplyMatchingStylesInternal:" + treeNodeProvider.GetDomElement(removedOrUpdateElementInfo.StartFrom).GetPath());
                    // sets HandledCss and MatchingStyles false/null
                    UnapplyMatchingStylesInternal(removedOrUpdateElementInfo.StartFrom, removedOrUpdateElementInfo.StyleSheet);
                    // compares MatchingStyles and AppliedMachingStyles and 
                    // sets AppliedMatchingStyles and Style null
                    //// Debug.WriteLine("removeOrUpdateElementInfos RemoveOutdatedStylesFromElementInternal:" + treeNodeProvider.GetDomElement(removedOrUpdateElementInfo.StartFrom).GetPath());
                    //RemoveOutdatedStylesFromElementInternal(removedOrUpdateElementInfo.StartFrom, removedOrUpdateElementInfo.StyleSheet, true, true);
                    ResetElementsBecauseRemovedInternal(removedOrUpdateElementInfo.StartFrom, removedOrUpdateElementInfo.StyleSheet, removedOrUpdateElementInfo.ChangeKind == ChangeKind.Remove);
                    // ResetElementsBecauseStyleSheetChangedInternal(removedOrUpdateElementInfo.StartFrom, removedOrUpdateElementInfo.StyleSheet, removedOrUpdateElementInfo.ChangeKind == ChangeKind.Remove);
                    RemoveStyleResourcesInternal(removedOrUpdateElementInfo.StartFrom, removedOrUpdateElementInfo.StyleSheet);
                }

                // remove all removed stylesheets & elements
                copy.RemoveAll(x => x.ChangeKind == ChangeKind.Remove);
                // remove all Elements which StyleSheet-Holder is already handled
                copy.RemoveAll(x => x.RenderTargetKind == RenderTargetKind.Element && handledStyleSheetHolders.Contains(x.StyleSheetHolder));

                // Debug.WriteLine("-------------------------------------------------");

                // add/update
                if (copy.Any())
                {
                    foreach (var item in copy.Where(x =>
                         x.RenderTargetKind == RenderTargetKind.Stylesheet &&
                         x.ChangeKind == ChangeKind.New))
                    {
                        // Debug.WriteLine("Stylesheet New:" + treeNodeProvider.GetDomElement(item.StyleSheetHolder).GetPath());
                        item.StyleSheet.AttachedTo = item.StyleSheetHolder;
                        dependencyPropertyService.SetStyledByStyleSheet(item.StyleSheetHolder, item.StyleSheet);
                    }

                    treeNodeProvider.Switch(SelectorType.VisualTree);

                    var starts = copy.Select(x => x.StartFrom ?? x.StyleSheetHolder)
                        .Distinct()
                        .ToList();

                    //foreach (var item in copy.Where(x => x.ChangeKind == ChangeKind.Update))
                    //{
                    //    treeNodeProvider.Switch(SelectorType.VisualTree);
                    //    // sets HandledCss and MatchingStyles false/null
                    //    UnapplyMatchingStylesInternal(item.StartFrom ?? item.StyleSheetHolder, item.StyleSheet);
                    //}

                    // Debug.WriteLine("");
                    // Debug.WriteLine("-------------------------------------------------");

                    // Debug.WriteLine("All Starts:");
                    treeNodeProvider.Switch(SelectorType.VisualTree);
                    // Debug.WriteLine(string.Join("\n", starts.Select(x => treeNodeProvider.GetDomElement(x).GetPath() + " - " + x.GetHashCode()).Distinct()));
                    // Debug.WriteLine("");

                    foreach (var item in copy)
                    {
                        // Debug.WriteLine("-------------------- START");
                        var start = item.StartFrom ?? item.StyleSheetHolder;
                        treeNodeProvider.Switch(SelectorType.VisualTree);
                        // Debug.WriteLine("START:" + treeNodeProvider.GetDomElement(start).GetPath() + " - " + start.GetHashCode());
                        if (!starts.Contains(start))
                        {
                            // Debug.WriteLine("    skipped!");
                            // Debug.WriteLine(string.Join("\n", starts.Select(x => treeNodeProvider.GetDomElement(x).GetPath() + " " + (x == start) + " - " + x.GetHashCode()).Distinct()));
                            continue;
                        }
                        if (dependencyPropertyService.GetHandledCss(start) != true)
                        {
                            // Debug.WriteLine("START CalculateStylesInternal:" + treeNodeProvider.GetDomElement(start).GetPath());
                            CalculateStylesInternal(item.StyleSheetHolder, item.StyleSheet, start, starts);
                            treeNodeProvider.Switch(SelectorType.VisualTree);
                            // crash when removed!!!
                            //// Debug.WriteLine("START RemoveOutdatedStylesFromElementInternal:" + treeNodeProvider.GetDomElement(start).GetPath());
                            //RemoveOutdatedStylesFromElementInternal(start, item.StyleSheet, true, true);
                        }
                    }

                    foreach (var item in copy)
                    {
                        // Debug.WriteLine("-------------------- Apply");
                        var start = item.StartFrom ?? item.StyleSheetHolder;
                        treeNodeProvider.Switch(SelectorType.VisualTree);
                        // Debug.WriteLine("Apply:" + treeNodeProvider.GetDomElement(start).GetPath() + " - " + start.GetHashCode());
                        if (dependencyPropertyService.GetHandledCss(start) != true)
                        {
                            treeNodeProvider.Switch(SelectorType.VisualTree);
                            // crash when removed!!!
                            //// Debug.WriteLine("START RemoveOutdatedStylesFromElementInternal:" + treeNodeProvider.GetDomElement(start).GetPath());
                            //RemoveOutdatedStylesFromElementInternal(start, item.StyleSheet, true, true);
                            treeNodeProvider.Switch(SelectorType.VisualTree);
                            // Debug.WriteLine("START ApplyMatchingStyles:" + treeNodeProvider.GetDomElement(start).GetPath());
                            ApplyMatchingStyles(start, item.StyleSheet);
                        }
                    }
                }
                // Debug.WriteLine("---------------- NEW FRAME DONE -----------------");
                // Debug.WriteLine("-------------------------------------------------");
                applicationResourcesService.EndUpdate();
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

        private List<StyleMatchInfo> UpdateMatchingStyles(TUIElement styleResourceReferenceHolder, StyleSheet styleSheet, TUIElement startFrom, IList<TUIElement> starts)
        {
            var requiredStyleInfos = new List<StyleMatchInfo>();
            IDomElement<TDependencyObject> root = null;

            IDomElement<TDependencyObject> visualTree = null;
            IDomElement<TDependencyObject> logicalTree = null;

            var found = new List<TUIElement>();

            foreach (var rule in styleSheet.Rules)
            {
                // // Debug.WriteLine($"--- RULE {rule.SelectorString} ----");
                if (rule.SelectorType == SelectorType.VisualTree)
                {
                    treeNodeProvider.Switch(SelectorType.VisualTree);
                    if (visualTree == null)
                    {
                        if (!treeNodeProvider.IsInTree(startFrom ?? styleResourceReferenceHolder))
                        {
                            // Debug.WriteLine("    Incorrect node - not visual: " + treeNodeProvider.GetDomElement(startFrom ?? styleResourceReferenceHolder).GetPath());
                            continue;
                        }

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
                            // Debug.WriteLine("    Incorrect node - not logical: " + treeNodeProvider.GetDomElement(startFrom ?? styleResourceReferenceHolder).GetPath());
                            continue;
                        }

                        logicalTree = treeNodeProvider.GetDomElement(startFrom ?? styleResourceReferenceHolder);
                        logicalTree.XamlCssStyleSheets.Clear();
                        logicalTree.XamlCssStyleSheets.Add(styleSheet);
                    }

                    root = logicalTree;
                }

                // apply our selector
                var matchedNodes = root.QuerySelectorAllWithSelf(styleSheet, rule.Selectors[0])
                    .Where(x => x != null)
                    .Cast<IDomElement<TDependencyObject>>()
                    .ToList();

                var otherStyleElements = matchedNodes
                    .Where(x =>
                    {
                        var foundStyleSheet = dependencyPropertyService.GetStyledByStyleSheet(x.Element);
                        if (foundStyleSheet == null)
                        {
                            foundStyleSheet = GetStyleSheetFromTree(x);
                        }

                        return foundStyleSheet != styleSheet;

                        /*if (stylesh == null)
                        {
                            return true;
                        }*/
                        //var elementStyleSheet = dependencyPropertyService.GetStyleSheet(parent);
                        //return elementStyleSheet != null && elementStyleSheet != styleSheet;
                    }).ToList();

                matchedNodes = matchedNodes.Except(otherStyleElements).ToList();

                var matchedElementTypes = matchedNodes
                    .Select(x => x.Element.GetType())
                    .Distinct()
                    .ToList();

                foreach (var matchingNode in matchedNodes)
                {
                    var element = matchingNode.Element;

                    dependencyPropertyService.SetStyledByStyleSheet(element, styleSheet);

                    found.Add((TUIElement)element);

                    var oldMatchingStyles = dependencyPropertyService.GetMatchingStyles(element) ?? new string[0];

                    var resourceKey = nativeStyleService.GetStyleResourceKey(styleSheet.Id, element.GetType(), rule.SelectorString);

                    var newMatchingStyles = oldMatchingStyles.Concat(new[] { resourceKey }).Distinct()
                        .Select(x => new
                        {
                            key = x,
                            selector = CachedSelectorProvider.Instance.GetOrAdd(x.Split('{')[1])
                        })
                        .OrderBy(x => x.selector.IdSpecificity)
                        .ThenBy(x => x.selector.ClassSpecificity)
                        .ThenBy(x => x.selector.SimpleSpecificity)
                        .ToArray();

                    var newMatchingStylesStrings = newMatchingStyles.Select(x => x.key).ToArray();
                    // // Debug.WriteLine($"'{rule.SelectorString}' {GetPath(matchingNode)}: " + string.Join("|", newMatchingStylesStrings));
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

            found = found.Distinct().ToList();

            treeNodeProvider.Switch(SelectorType.VisualTree);
            // Debug.WriteLine("FOUND:\n" + string.Join("\n", found.Select(x => "    " + treeNodeProvider.GetDomElement(x).GetPath() + " - " + x.GetHashCode())));
            foreach (var f in found)
            {
                if (starts.Contains(f))
                {
                    //// Debug.WriteLine($"Found -> Ignore: " + Utils.HierarchyDebugExtensions.GetPath(matchingNode));
                    starts.Remove(f);
                }
            }


            return requiredStyleInfos;
        }

        protected void CalculateStylesInternal(TUIElement styleResourceReferenceHolder, StyleSheet styleSheet, TUIElement startFrom, IList<TUIElement> starts)
        {
            if (styleResourceReferenceHolder == null ||
                styleSheet == null)
            {
                return;
            }

            // Utils.HierarchyDebugExtensions.PrintHerarchyDebugInfo(treeNodeProvider, dependencyPropertyService, styleResourceReferenceHolder, startFrom);

            var requiredStyleInfos = UpdateMatchingStyles(styleResourceReferenceHolder, styleSheet, startFrom, starts);

            GenerateStyles(styleResourceReferenceHolder, styleSheet, startFrom, requiredStyleInfos);
        }

        public void RemoveStyleResources(TUIElement styleResourceReferenceHolder, StyleSheet styleSheet)
        {
            EnqueueRemoveStyleSheet(styleResourceReferenceHolder, styleSheet);
        }
        protected void RemoveStyleResourcesInternal(TUIElement styleResourceReferenceHolder, StyleSheet styleSheet)
        {
            // // Debug.WriteLine("----------------");
            // // Debug.WriteLine("RemoveStyleResourcesInternal");

            var resourceKeys = applicationResourcesService.GetKeys()
                .OfType<string>()
                .Where(x => x.StartsWith(nativeStyleService.BaseStyleResourceKey + "_" + styleSheet.Id, StringComparison.Ordinal))
                .ToList();

            // // Debug.WriteLine(" - remove resourceKeys: " + string.Join(", ", resourceKeys));

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

                // // Debug.WriteLine("Generate Style " + resourceKey);

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

                    if (!propertyStyleValues.Any() &&
                        !nativeTriggers.Any())
                    {
                        // // Debug.WriteLine("no values found -> continue");
                        continue;
                    }

                    var style = nativeStyleService.CreateFrom(propertyStyleValues, nativeTriggers, matchedElementType);

                    applicationResourcesService.SetResource(resourceKey, style);

                    // // Debug.WriteLine("Finished generate Style " + resourceKey);
                }
                catch (Exception e)
                {
                    styleSheet.AddError($@"ERROR in Selector ""{styleMatchInfo.Rule.SelectorString}"": {e.Message}");
                }
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

        private void ApplyMatchingStyles(TUIElement visualElement, StyleSheet styleSheet)
        {
            // Debug.WriteLine("ApplyMatchingStyles " + Utils.HierarchyDebugExtensions.GetPath(treeNodeProvider.GetDomElement(visualElement)));


            var matchingStyles = dependencyPropertyService.GetMatchingStyles(visualElement);
            var appliedMatchingStyles = dependencyPropertyService.GetAppliedMatchingStyles(visualElement);

            if (visualElement == null ||
                dependencyPropertyService.GetHandledCss(visualElement))
            {
                // Debug.WriteLine($"    Already handled ({string.Join(" ", matchingStyles ?? new string[0]) + " / " + string.Join(" ", appliedMatchingStyles ?? new string[0])})");
                return;
            }

            var styledBy = dependencyPropertyService.GetStyledByStyleSheet(visualElement) ??
                GetStyleSheetFromTree(treeNodeProvider.GetDomElement(visualElement));
            if (styledBy != null &&
                styledBy != styleSheet)
            {
                // Debug.WriteLine("    Another Stylesheet");
                return;
            }


            dependencyPropertyService.SetStyledByStyleSheet(visualElement, styledBy);


            if (!AppliedStyleIdsAreMatchedStyleIds(appliedMatchingStyles, matchingStyles))
            {
                // Debug.WriteLine($"    No match: ({string.Join(" ", matchingStyles ?? new string[0]) + " / " + string.Join(" ", appliedMatchingStyles ?? new string[0])})");
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
                    else
                    {
                        // Debug.WriteLine("    Style not found! " + matchingStyles[0]);
                    }
                }
                else if (matchingStyles?.Length > 1)
                {
                    var dict = new Dictionary<TDependencyProperty, object>();
                    var listTriggers = new List<TDependencyObject>();

                    foreach (var matchingStyle in matchingStyles)
                    {
                        TStyle s = null;
                        if (applicationResourcesService.Contains(matchingStyle) == true)
                        {
                            s = (TStyle)applicationResourcesService.GetResource(matchingStyle);
                        }
                        else
                        {
                            // Debug.WriteLine("    Style not found! " + matchingStyle);
                        }

                        if (s != null)
                        {
                            var subDict = nativeStyleService.GetStyleAsDictionary(s as TStyle);

                            foreach (var i in subDict)
                            {
                                dict[i.Key] = i.Value;
                            }

                            var triggers = nativeStyleService.GetTriggersAsList(s as TStyle);
                            listTriggers.AddRange(triggers);
                        }
                    }

                    if (dict.Keys.Count > 0 ||
                        listTriggers.Count > 0)
                    {
                        styleToApply = nativeStyleService.CreateFrom(dict, listTriggers, visualElement.GetType());
                    }

                    if (styleToApply != null)
                    {
                        nativeStyleService.SetStyle(visualElement, (TStyle)styleToApply);
                    }
                }

                dependencyPropertyService.SetAppliedMatchingStyles(visualElement, matchingStyles);
            }

            dependencyPropertyService.SetHandledCss(visualElement, true);

            matchingStyles = dependencyPropertyService.GetMatchingStyles(visualElement);
            appliedMatchingStyles = dependencyPropertyService.GetAppliedMatchingStyles(visualElement);

            // Debug.WriteLine("    Child Count: " + treeNodeProvider.GetChildren(visualElement).ToList().Count);
            // Debug.WriteLine("    matched / applied: " + string.Join(" ", matchingStyles ?? new string[0]) + " / " + string.Join(" ", appliedMatchingStyles ?? new string[0]));
            foreach (var child in treeNodeProvider.GetChildren(visualElement).ToList())
            {
                ApplyMatchingStyles(child as TUIElement, styleSheet);
            }
            // // Debug.WriteLine($"Applying: {string.Join(", ", dependencyPropertyService.GetMatchingStyles(visualElement) ?? new string[0])}");
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
            // Debug.WriteLine("UnapplyMatchingStylesInternal: " + Utils.HierarchyDebugExtensions.GetPath(treeNodeProvider.GetDomElement(bindableObject)));

            if (bindableObject == null/* ||
                dependencyPropertyService.GetHandledCss(bindableObject) == false*/)
            {
                // Debug.WriteLine("already handled -> return");
                return;
            }

            if (bindableObject.GetType().Name == "Run")
            {

            }

            var styledBy = dependencyPropertyService.GetStyledByStyleSheet(bindableObject);
            /*if (styledBy == null)
            {
                // Debug.WriteLine("Not yet styled -> return");
                return;
            }*/

            if (styledBy != null &&
                styledBy != styleSheet)
            {
                // Debug.WriteLine("Another Stylesheet");
                return;
            }

            dependencyPropertyService.SetHandledCss(bindableObject, false);
            dependencyPropertyService.SetMatchingStyles(bindableObject, null);

            foreach (var child in treeNodeProvider.GetChildren(bindableObject).ToList())
            {
                UnapplyMatchingStylesInternal(child, styleSheet);
            }
        }

        protected void ResetElementsBecauseRemovedInternal(TDependencyObject bindableObject, StyleSheet styleSheet, bool removed)
        {
            // Debug.WriteLine("ResetElementsBecauseStyleSheetChangedInternal: " + Utils.HierarchyDebugExtensions.GetPath(treeNodeProvider.GetDomElement(bindableObject)));

            if (bindableObject == null)
            {
                return;
            }

            if (bindableObject.GetType().Name == "TextBlock")
            {

            }

            var styledBy = dependencyPropertyService.GetStyledByStyleSheet(bindableObject);
            if (styledBy != null &&
                styledBy != styleSheet)
            {
                // Debug.WriteLine("    Another Stylesheet -> return");

                nativeStyleService.SetStyle(bindableObject, dependencyPropertyService.GetInitialStyle(bindableObject));

                return;
            }

            //dependencyPropertyService.SetMatchingStyles(bindableObject, null);
            dependencyPropertyService.SetAppliedMatchingStyles(bindableObject, null);
            nativeStyleService.SetStyle(bindableObject, dependencyPropertyService.GetInitialStyle(bindableObject));

            if (removed)
            {
                //dependencyPropertyService.SetStyledByStyleSheet(bindableObject, null);
            }

            foreach (var child in treeNodeProvider.GetChildren(bindableObject).ToList())
            {
                ResetElementsBecauseRemovedInternal(child, styleSheet, removed);
            }
        }

        protected void ResetElementsBecauseStyleSheetChangedInternal(TDependencyObject bindableObject, StyleSheet styleSheet, bool removed)
        {
            // Debug.WriteLine("ResetElementsBecauseStyleSheetChangedInternal: " + Utils.HierarchyDebugExtensions.GetPath(treeNodeProvider.GetDomElement(bindableObject)));

            if (bindableObject == null)
            {
                return;
            }

            if (bindableObject.GetType().Name == "TextBlock")
            {

            }

            var styledBy = dependencyPropertyService.GetStyledByStyleSheet(bindableObject);
            if (styledBy != null &&
                styledBy != styleSheet)
            {
                // Debug.WriteLine("    Another Stylesheet -> return");

                nativeStyleService.SetStyle(bindableObject, dependencyPropertyService.GetInitialStyle(bindableObject));

                return;
            }

            //dependencyPropertyService.SetMatchingStyles(bindableObject, null);
            dependencyPropertyService.SetAppliedMatchingStyles(bindableObject, null);

            nativeStyleService.SetStyle(bindableObject, dependencyPropertyService.GetInitialStyle(bindableObject));

            if (removed)
            {
                dependencyPropertyService.SetStyledByStyleSheet(bindableObject, null);
            }

            foreach (var child in treeNodeProvider.GetChildren(bindableObject).ToList())
            {
                ResetElementsBecauseStyleSheetChangedInternal(child, styleSheet, removed);
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
                //treeNodeProvider.Switch(SelectorType.LogicalTree);
                while (currentBindableObject != null)
                {
                    var styleSheet = dependencyPropertyService.GetStyleSheet(currentBindableObject);
                    if (styleSheet != null)
                    {
                        //treeNodeProvider.Switch(oldSelectorType);
                        return currentBindableObject;
                    }

                    currentBindableObject = treeNodeProvider.GetParent(currentBindableObject as TDependencyObject);
                }
            }
            finally
            {
                //treeNodeProvider.Switch(oldSelectorType);
            }

            return null;
        }
    }
}

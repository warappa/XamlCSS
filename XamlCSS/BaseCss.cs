using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
        public readonly ITreeNodeProvider<TDependencyObject> treeNodeProvider;
        public readonly IStyleResourcesService applicationResourcesService;
        public readonly INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService;

        protected bool executeApplyStylesExecuting;

        private readonly IMarkupExtensionParser markupExpressionParser;
        private Action<Action> uiInvoker;
        private List<RenderInfo<TDependencyObject>> items = new List<RenderInfo<TDependencyObject>>();
        private CssTypeHelper<TDependencyObject, TDependencyProperty, TStyle> cssTypeHelper;
        private int noopCount = 0;

        public BaseCss(IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
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
            this.cssTypeHelper = new CssTypeHelper<TDependencyObject, TDependencyProperty, TStyle>(markupExpressionParser, dependencyPropertyService);

            CssParser.Initialize(defaultCssNamespace, fileProvider);
            StyleSheet.GetParent = parent => treeNodeProvider.GetParent((TDependencyObject)parent, SelectorType.VisualTree);
            StyleSheet.GetStyleSheet = treeNode => dependencyPropertyService.GetStyleSheet((TDependencyObject)treeNode);
        }

        public void ExecuteApplyStyles()
        {
            if (executeApplyStylesExecuting)
            {
                return;
            }

            executeApplyStylesExecuting = true;

            List<RenderInfo<TDependencyObject>> copy;

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

            var styleholder = copy.First().StyleSheetHolder;

            "Render".Measure(() =>
            {
                Render(copy, treeNodeProvider, dependencyPropertyService, nativeStyleService, applicationResourcesService, cssTypeHelper);
            });

            // Investigate.Print();
            // HierarchyDebugExtensions.PrintHerarchyDebugInfo(treeNodeProvider, dependencyPropertyService, copy.First().StyleSheetHolder, copy.First().StyleSheetHolder, SelectorType.LogicalTree);
            executeApplyStylesExecuting = false;
        }

        private static void RemoveStyleResourcesInternal(
            TDependencyObject styleResourceReferenceHolder,
            StyleSheet styleSheet,
            IStyleResourcesService applicationResourcesService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService)
        {
            var resourceKeys = applicationResourcesService.GetKeys()
                .OfType<string>()
                .Where(x => x.StartsWith($"{nativeStyleService.BaseStyleResourceKey}_{styleSheet.Id}", StringComparison.Ordinal))
                .ToList();

            foreach (var key in resourceKeys)
            {
                applicationResourcesService.RemoveResource(key);
            }
        }

        public static void Render(
            List<RenderInfo<TDependencyObject>> copy,
            ITreeNodeProvider<TDependencyObject> treeNodeProvider,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService,
            IStyleResourcesService applicationResourcesService,
            CssTypeHelper<TDependencyObject, TDependencyProperty, TStyle> cssTypeHelper)
        {
            try
            {
                "BeginUpdate".Measure(() => applicationResourcesService.BeginUpdate());

                "RemoveOldStyleObjects".Measure(() =>
                {
                    RemoveOldStyleObjects(copy, nativeStyleService, applicationResourcesService);
                });
                "SetAttachedToToNull".Measure(() =>
                {
                    SetAttachedToToNull(copy, dependencyPropertyService, treeNodeProvider, nativeStyleService);
                });
                "SetAttachedToToNewStyleSheet".Measure(() =>
                {
                    SetAttachedToToNewStyleSheet(copy, dependencyPropertyService, treeNodeProvider, nativeStyleService);
                });

                var styleUpdateInfos = new Dictionary<TDependencyObject, StyleUpdateInfo>();

                var newOrUpdatedStyleSheets = copy
                   .Where(x =>
                   x.RenderTargetKind == RenderTargetKind.Stylesheet)
                   .Select(x => x.StyleSheet)
                   .Distinct()
                   .ToList();

                var newOrUpdatedStyleHolders = copy
                    .Where(x =>
                        x.ChangeKind == ChangeKind.New ||
                        x.ChangeKind == ChangeKind.Update ||
                        x.ChangeKind == ChangeKind.Remove)
                    .Select(x => new { x.ChangeKind, x.StartFrom, x.StyleSheet, x.StyleSheetHolder })
                    .Distinct()
                    .ToList();

                "SetupStyleInfo LOGICAL".Measure(() =>
                {
                    foreach (var item in newOrUpdatedStyleHolders)
                    {
                        var start = item.StartFrom ?? item.StyleSheetHolder;
                        if (!treeNodeProvider.IsInTree(start, SelectorType.LogicalTree))
                        {
                            continue;
                        }
                        var domElement = treeNodeProvider.GetDomElement(start);

                        "EnsureParents".Measure(() =>
                        {
                            EnsureParents(domElement, treeNodeProvider, dependencyPropertyService, nativeStyleService, styleUpdateInfos, SelectorType.LogicalTree);
                        });

                        var discardOldMatchingStyles = newOrUpdatedStyleSheets.Contains(item.StyleSheet);
                        SetupStyleInfo(domElement, item.StyleSheet, styleUpdateInfos, treeNodeProvider, dependencyPropertyService, nativeStyleService, discardOldMatchingStyles, item.ChangeKind == ChangeKind.Remove, SelectorType.LogicalTree);
                    }
                });

                "SetupStyleInfo VisualTree".Measure(() =>
                {
                    foreach (var item in newOrUpdatedStyleHolders)
                    {
                        var start = item.StartFrom ?? item.StyleSheetHolder;
                        if (!treeNodeProvider.IsInTree(start, SelectorType.VisualTree))
                        {
                            continue;
                        }

                        var domElement = treeNodeProvider.GetDomElement(start);

                        "EnsureParents".Measure(() =>
                        {
                            EnsureParents(domElement, treeNodeProvider, dependencyPropertyService, nativeStyleService, styleUpdateInfos, SelectorType.VisualTree);
                        });

                        var discardOldMatchingStyles = newOrUpdatedStyleSheets.Contains(item.StyleSheet);
                        SetupStyleInfo(domElement, item.StyleSheet, styleUpdateInfos, treeNodeProvider, dependencyPropertyService, nativeStyleService, discardOldMatchingStyles, item.ChangeKind == ChangeKind.Remove, SelectorType.VisualTree);
                    }
                });

                var tasks = new List<Task<IList<IDomElement<TDependencyObject>>>>();
                var distinctCopy = copy.Select(x => new { x.StartFrom, x.StyleSheetHolder, x.StyleSheet }).Distinct().ToList();
                $"distinctCopy {distinctCopy.Count} items".Measure(() => { });
                foreach (var item in distinctCopy)
                {
                    var start = item.StartFrom ?? item.StyleSheetHolder;

                    if (!styleUpdateInfos.ContainsKey(start))
                    {
                        continue;
                    }

                    //treeNodeProvider.Switch(SelectorType.VisualTree);

                    var domElement = treeNodeProvider.GetDomElement(start);
                    //visual.StyleInfo = styleUpdateInfos[start];

                    //if (!switchableTreeNodeProvider.IsInTree(start))
                    //{
                    //    visual = null;
                    //}

                    "UpdateMatchingStyles".Measure(() =>
                    {
                        //var task = Task.Run(() =>
                        //{
                        tasks.Add(Task.FromResult(UpdateMatchingStyles(item.StyleSheet, domElement, styleUpdateInfos, dependencyPropertyService, nativeStyleService)));
                        //return UpdateMatchingStyles(item.StyleSheet, logical, visual, styleUpdateInfos, dependencyPropertyService, nativeStyleService);

                        //});
                        //tasks.Add(task);
                    });
                }

                //Task.WaitAll(tasks.ToArray());
                var allFound = tasks.SelectMany(x => x.Result).ToList();
                var allFoundElements = allFound.Select(x => x.Element).ToList();
                "allNotFoundElements".Measure(() =>
                {
                    var allNotFoundKeys = styleUpdateInfos
                        .Where(x => !allFoundElements.Contains(x.Key))
                        .ToList();

                    foreach (var item in allNotFoundKeys)
                    {
                        var styleUpdateInfo = item.Value;

                        // styleUpdateInfo.CurrentMatchedSelectors = new List<string>();
                        styleUpdateInfo.OldMatchedSelectors = new List<string>();
                        styleUpdateInfo.DoMatchCheck = SelectorType.None;
                        // remove style
                        nativeStyleService.SetStyle(item.Key, dependencyPropertyService.GetInitialStyle(item.Key));
                    }
                });

                "GenerateStyles".Measure(() =>
                {
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
                });
                "ApplyMatchingStyles".Measure(() =>
                {
                    //foreach (var item in copy.Select(x => new { x.StartFrom, x.StyleSheetHolder, x.StyleSheet }).Distinct().ToList())
                    //{
                    //    var start = item.StartFrom ?? item.StyleSheetHolder;

                    //    var domElement = treeNodeProvider.GetDomElement(start);

                    //ApplyMatchingStyles(domElement, item.StyleSheet, applicationResourcesService, nativeStyleService);
                    //}

                    foreach (var f in allFound)
                    {
                        ApplyMatchingStylesNode(f, applicationResourcesService, nativeStyleService);
                    }
                });
            }
            finally
            {

                "EndUpdate".Measure(() => applicationResourcesService.EndUpdate());
            }

        }
        private static void ReevaluateStylesheetInSubTree(IDomElement<TDependencyObject> domElement, StyleSheet oldStyleSheet,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService)
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

            domElement.StyleInfo.CurrentStyleSheet = GetStyleSheetFromTree(domElement, dependencyPropertyService);

            foreach (var child in domElement.LogicalChildNodes)
            {
                ReevaluateStylesheetInSubTree(child, oldStyleSheet, dependencyPropertyService, nativeStyleService);
            }
        }

        private static void SetAttachedToToNewStyleSheet(List<RenderInfo<TDependencyObject>> copy,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            ITreeNodeProvider<TDependencyObject> treeNodeProvider,
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

                ReevaluateStylesheetInSubTree(domElement, domElement.StyleInfo?.CurrentStyleSheet, dependencyPropertyService, nativeStyleService);
            }
        }

        private static void SetAttachedToToNull(List<RenderInfo<TDependencyObject>> copy,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            ITreeNodeProvider<TDependencyObject> treeNodeProvider,
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
                ReevaluateStylesheetInSubTree(treeNodeProvider.GetDomElement((TDependencyObject)removedStyleSheet.AttachedTo), removedStyleSheet, dependencyPropertyService, nativeStyleService);

                removedStyleSheet.AttachedTo = null;
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

        private static void EnsureParents(IDomElement<TDependencyObject> domElement, ITreeNodeProvider<TDependencyObject> treeNodeProvider,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService,
            IDictionary<TDependencyObject, StyleUpdateInfo> styleUpdateInfos, SelectorType type)
        {
            var current = type == SelectorType.VisualTree ? domElement.Parent : domElement.LogicalParent;
            while (current != null)
            {
                var styleUpdateInfo = current.StyleInfo = current.StyleInfo ?? (styleUpdateInfos.ContainsKey(current.Element) ? styleUpdateInfos[current.Element] :
                    GetNewStyleUpdateInfo(current, dependencyPropertyService, nativeStyleService));

                if ((styleUpdateInfo.DoMatchCheck & type) != type)
                {
                    return;
                }

                object a;
                "ClassList".Measure(() => a = current.ClassList);
                //"Id".Measure(() => a = current.Id);
                "TagName".Measure(() => a = current.TagName);
                "AssemblyQualifiedNamespaceName".Measure(() => a = current.AssemblyQualifiedNamespaceName);
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

        private static bool AppliedStyleIdsAreMatchedStyleIds(List<string> appliedMatchingStyles, List<string> matchingStyles)
        {
            return matchingStyles == appliedMatchingStyles ||
                            (
                                matchingStyles != null &&
                                appliedMatchingStyles != null &&
                                matchingStyles.SequenceEqual(appliedMatchingStyles)
                            );
        }

        private static void ApplyMatchingStylesNode(IDomElement<TDependencyObject> domElement, 
           IStyleResourcesService applicationResourcesService,
           INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService)
        {
            var visualElement = domElement.Element;

            var matchingStyles = domElement.StyleInfo.CurrentMatchedSelectors;
            var appliedMatchingStyles = domElement.StyleInfo.OldMatchedSelectors;

            if (!AppliedStyleIdsAreMatchedStyleIds(appliedMatchingStyles, matchingStyles))
            {
                object styleToApply = null;

                if (matchingStyles == null)
                {
                    // RemoveOutdatedStylesFromElementInternal(visualElement, styleSheet, true, true);
                }
                else if (matchingStyles?.Count == 1)
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
                        nativeStyleService.SetStyle(visualElement, null);
                        // Debug.WriteLine("    Style not found! " + matchingStyles[0]);
                    }
                }
                else if (matchingStyles?.Count > 1)
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
                    else
                    {
                        nativeStyleService.SetStyle(visualElement, null);
                    }
                }

                domElement.StyleInfo.OldMatchedSelectors = matchingStyles.ToList();
            }
        }

        private static IList<IDomElement<TDependencyObject>> UpdateMatchingStyles(
            StyleSheet styleSheet,
            IDomElement<TDependencyObject> startFrom,
            Dictionary<TDependencyObject, StyleUpdateInfo> styleUpdateInfos,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService)
        {
            // var requiredStyleInfos = new List<StyleMatchInfo>();
            var found = new List<IDomElement<TDependencyObject>>();
            if (startFrom == null)
            {
                return found;
            }

            if (startFrom?.StyleInfo.DoMatchCheck == SelectorType.None)
            {
                $"DoMatchCheck == None".Measure(() => { });
                return new List<IDomElement<TDependencyObject>>();
            }

            startFrom?.XamlCssStyleSheets.Clear();
            startFrom?.XamlCssStyleSheets.Add(styleSheet);

            var traversed = SelectorType.None;

            return $"startFrom {startFrom?.GetPath(SelectorType.LogicalTree) ?? "NULL!?!"}".Measure(() =>
            {
                foreach (var rule in styleSheet.Rules)
                {
                    $"{rule.SelectorString}".Measure(() =>
                    {
                        // // Debug.WriteLine($"--- RULE {rule.SelectorString} ----");

                        // apply our selector

                        var type = SelectorType.LogicalTree;
                        if (rule.Selectors[0].StartOnVisualTree())
                            type = SelectorType.VisualTree;

                        if ((type == SelectorType.LogicalTree && !startFrom.IsInLogicalTree) ||
                            (type == SelectorType.VisualTree && !startFrom.IsInVisualTree)
                        )
                        {
                            return;
                            // continue;
                        }

                        traversed |= type;

                        var matchedNodes = "QuerySelectorAllWithSelf".Measure(() => startFrom.QuerySelectorAllWithSelf(styleSheet, rule.Selectors[0], type)
                                .Where(x => x != null)
                                .Cast<IDomElement<TDependencyObject>>()
                                .ToList());

                        var matchedElementTypes = matchedNodes
                            .Select(x => x.Element.GetType())
                            .Distinct()
                            .ToList();

                        $"foreach {matchedNodes.Count}".Measure(() =>
                        {
                            foreach (var matchingNode in matchedNodes)
                            {
                                var element = matchingNode.Element;

                                if (!found.Contains(matchingNode))
                                {
                                    found.Add(matchingNode);
                                }

                                var discriminator = "GetInitialStyle".Measure(() => dependencyPropertyService.GetInitialStyle(element) != null ? element.GetHashCode().ToString() : "");
                                var resourceKey = "GetStyleResourceKey".Measure(() => nativeStyleService.GetStyleResourceKey(styleSheet.Id + discriminator, element.GetType(), rule.SelectorString));
                                //var resourceKey = nativeStyleService.GetStyleResourceKey(rule.StyleSheetId, element.GetType(), rule.SelectorString);

                                if (!matchingNode.StyleInfo.CurrentMatchedSelectors.Contains(resourceKey))
                                {
                                    matchingNode.StyleInfo.CurrentMatchedSelectors.Add(resourceKey);
                                }
                            }
                        });
                    });
                }

                "found".Measure(() =>
                {
                    found = found.Distinct().ToList();

                    foreach (var f in found)
                    {
                        f.StyleInfo.DoMatchCheck = SelectorType.None;

                        f.StyleInfo.CurrentMatchedSelectors = f.StyleInfo.CurrentMatchedSelectors.Distinct().Select(x => new
                        {
                            key = x,
                            SpecificityResult = SpecificityCalculator.Calculate(x.Split('{')[1])
                        })
                                .OrderBy(x => x.SpecificityResult.IdSpecificity)
                                .ThenBy(x => x.SpecificityResult.ClassSpecificity)
                                .ThenBy(x => x.SpecificityResult.SimpleSpecificity)
                                .ToList()
                                .Select(x => x.key)
                                .ToList();
                    }
                });

                "SetDoMatchCheckToNoneInSubTree".Measure(() =>
                {
                    if ((traversed & SelectorType.VisualTree) > 0)
                        SetDoMatchCheckToNoneInSubTree(startFrom, styleSheet, SelectorType.VisualTree);
                    if ((traversed & SelectorType.LogicalTree) > 0)
                        SetDoMatchCheckToNoneInSubTree(startFrom, styleSheet, SelectorType.LogicalTree);
                });
                return found;
            });
        }

        private static void SetDoMatchCheckToNoneInSubTree(IDomElement<TDependencyObject> domElement, StyleSheet styleSheet, SelectorType type)
        {
            if (domElement == null ||
                !ReferenceEquals(domElement.StyleInfo.CurrentStyleSheet, styleSheet))
            {
                return;
            }

            domElement.StyleInfo.DoMatchCheck = SelectorType.None;

            var children = type == SelectorType.VisualTree ? domElement.ChildNodes : domElement.LogicalChildNodes;
            foreach (var child in children)
            {
                SetDoMatchCheckToNoneInSubTree(child, styleSheet, type);
            }
        }

        private static StyleSheet GetStyleSheetFromTree(IDomElement<TDependencyObject> domElement,
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
            IDomElement<TDependencyObject> domElement,
            StyleSheet styleSheet,
            Dictionary<TDependencyObject, StyleUpdateInfo> styleUpdateInfos,
            ITreeNodeProvider<TDependencyObject> treeNodeProvider,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService,
            bool styleChanged,
            bool styleSheetRemoved, SelectorType type)
        {
            var styleUpdateInfo = "get styleUpdateInfo".Measure(() => domElement.StyleInfo = domElement.StyleInfo ?? (styleUpdateInfos.ContainsKey(domElement.Element) ? styleUpdateInfos[domElement.Element] :
                GetNewStyleUpdateInfo(domElement, dependencyPropertyService, nativeStyleService)));

            styleUpdateInfos[domElement.Element] = styleUpdateInfo;

            var styleSheetFromDom = "GetStyleSheet".Measure(() => dependencyPropertyService.GetStyleSheet(domElement.Element));
            if (styleSheetFromDom != null &&
                styleSheetFromDom != styleSheet)
            {
                // another stylesheet's domelement
                SetupStyleInfo(domElement, styleSheetFromDom, styleUpdateInfos, treeNodeProvider, dependencyPropertyService, nativeStyleService, styleChanged, false, type);
                return;
            }

            "set styleUpdateInfo values".Measure(() =>
            {
                if (!styleSheetRemoved)
                {
                    styleUpdateInfo.CurrentStyleSheet = styleSheet;
                }

                if (styleChanged)
                {
                    styleUpdateInfo.OldMatchedSelectors = new List<string>();
                }
                styleUpdateInfo.CurrentMatchedSelectors.Clear();
                styleUpdateInfo.DoMatchCheck |= type;
            });

            /*
            "fill DomElement".Measure(() =>
            {
                object a;
                "ClassList".Measure(() => a = domElement.ClassList);
                "Id".Measure(() => a = domElement.Id);
                "TagName".Measure(() => a = domElement.TagName);
                "NamespaceUri".Measure(() => a = domElement.AssemblyQualifiedNamespaceName);
                //"HasAttribute".Measure(() => a = domElement.HasAttribute("Name"));
            });
            */

            var children = type == SelectorType.VisualTree ? domElement.ChildNodes : domElement.LogicalChildNodes;
            foreach (var child in children)
            {
                SetupStyleInfo(child, styleSheet, styleUpdateInfos, treeNodeProvider, dependencyPropertyService, nativeStyleService, styleChanged, styleSheetRemoved, type);
            }
        }

        private static StyleUpdateInfo GetNewStyleUpdateInfo(IDomElement<TDependencyObject> domElement,
            IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty> dependencyPropertyService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService)
        {
            if (dependencyPropertyService.GetInitialStyle(domElement.Element) == null)
            {
                dependencyPropertyService.SetInitialStyle(domElement.Element, nativeStyleService.GetStyle(domElement.Element));
            }

            return new StyleUpdateInfo
            {
                MatchedType = domElement.Element.GetType()
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

                foreach (var resourceKey in styleMatchInfo.CurrentMatchedSelectors)
                {
                    var s = resourceKey.Split('{')[1];

                    if (applicationResourcesService.Contains(resourceKey))
                    {
                        // Debug.WriteLine($"GenerateStyles: Already contains '{s}' ({matchedElementType.Name})");
                        continue;
                    }

                    // Debug.WriteLine($"GenerateStyles: Generating '{s}' ({matchedElementType.Name})");

                    var rule = styleMatchInfo.CurrentStyleSheet.Rules.Where(x => x.SelectorString == s).First();

                    CreateStyleDictionaryFromDeclarationBlockResult<TDependencyProperty> result = null;
                    try
                    {
                        result = "CreateStyleDictionaryFromDeclarationBlock".Measure(() => CreateStyleDictionaryFromDeclarationBlock(
                            styleSheet.Namespaces,
                            rule.DeclarationBlock,
                            matchedElementType,
                            (TDependencyObject)styleSheet.AttachedTo,
                            cssTypeHelper));

                        var propertyStyleValues = result.PropertyStyleValues;

                        foreach (var error in result.Errors)
                        {
                            // Debug.WriteLine($@" ERROR (normal) in Selector ""{rule.SelectorString}"": {error}");
                            styleSheet.AddError($@"ERROR in Selector ""{rule.SelectorString}"": {error}");
                        }

                        var nativeTriggers = $"CreateTriggers ({rule.DeclarationBlock.Triggers.Count})".Measure(() => rule.DeclarationBlock.Triggers
                            .Select(x => nativeStyleService.CreateTrigger(styleSheet, x, styleMatchInfo.MatchedType, (TDependencyObject)styleSheet.AttachedTo))
                            .ToList());

                        var initalStyle = dependencyPropertyService.GetInitialStyle(styleMatchInfoKeyValue.Key);
                        if (initalStyle != null)
                        {
                            var subDict = nativeStyleService.GetStyleAsDictionary(initalStyle as TStyle);

                            foreach (var i in subDict)
                            {
                                // only set not-overridden properties
                                if (!propertyStyleValues.ContainsKey(i.Key))
                                {
                                    propertyStyleValues[i.Key] = i.Value;
                                }
                            }

                            var triggers = nativeStyleService.GetTriggersAsList(initalStyle as TStyle);
                            nativeTriggers.InsertRange(0, triggers);
                        }
                        //Debug.WriteLine("    Values: " + string.Join(", ", propertyStyleValues.Select(x => ((dynamic)x.Key).PropertyName + ": " + x.Value.ToString())));
                        var style = "    Create Style".Measure(() => nativeStyleService.CreateFrom(propertyStyleValues, nativeTriggers, matchedElementType));

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

            var parent = GetStyleSheetParent(sender as TDependencyObject);
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

            var parent = GetStyleSheetParent(sender as TDependencyObject);
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

            var parent = GetStyleSheetParent(sender as TDependencyObject);
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
                    currentDependencyObject = treeNodeProvider.GetParent(currentDependencyObject as TDependencyObject, SelectorType.LogicalTree);
                }
            }
            finally
            {
            }

            return null;
        }
    }
}

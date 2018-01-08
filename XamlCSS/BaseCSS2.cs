using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using XamlCSS.Dom;
using XamlCSS.Utils;

namespace XamlCSS
{
    public class StyleUpdateInfo
    {
        public StyleSheet OldStyleSheet { get; set; }
        public List<string> OldMatchedSelectors { get; set; }
        public StyleSheet CurrentStyleSheet { get; set; }
        public List<string> CurrentMatchedSelectors { get; set; } = new List<string>();
        public Type MatchedType { get; internal set; }
        public SelectorType DoMatchCheck { get; internal set; }
    }
    class BaseCSS2<TDependencyObject, TUIElement, TStyle, TDependencyProperty>
            where TDependencyObject : class
            where TUIElement : class, TDependencyObject
            where TStyle : class
            where TDependencyProperty : class
    {
        private static void RemoveStyleResourcesInternal(
            TUIElement styleResourceReferenceHolder,
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
            List<RenderInfo<TDependencyObject, TUIElement>> copy,
            ISwitchableTreeNodeProvider<TDependencyObject> switchableTreeNodeProvider,
            IDependencyPropertyService<TDependencyObject, TUIElement, TStyle, TDependencyProperty> dependencyPropertyService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService,
            IStyleResourcesService applicationResourcesService,
            CssTypeHelper<TDependencyObject, TUIElement, TDependencyProperty, TStyle> cssTypeHelper)
        {
            try
            {
                applicationResourcesService.BeginUpdate();

                "RemoveOldStyleObjects".Measure(() =>
                {
                    RemoveOldStyleObjects(copy, nativeStyleService, applicationResourcesService);
                });
                "SetAttachedToToNull".Measure(() =>
                {
                    SetAttachedToToNull(copy);
                });
                "SetAttachedToToNewStyleSheet".Measure(() =>
                {
                    SetAttachedToToNewStyleSheet(copy);
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
                    .Select(x => new { x.StartFrom, x.StyleSheet, x.StyleSheetHolder })
                    .Distinct()
                    .ToList();

                "SetupStyleInfo LOGICAL".Measure(() =>
                {
                    switchableTreeNodeProvider.Switch(SelectorType.LogicalTree);

                    foreach (var item in newOrUpdatedStyleHolders)
                    {
                        var start = item.StartFrom ?? item.StyleSheetHolder;
                        if (!switchableTreeNodeProvider.IsInTree(start))
                        {
                            continue;
                        }
                        var domElement = switchableTreeNodeProvider.GetDomElement(start);

                        "EnsureParents".Measure(() =>
                        {
                            EnsureParents(domElement, switchableTreeNodeProvider, styleUpdateInfos);
                        });

                        SetupStyleInfo(domElement, item.StyleSheet, styleUpdateInfos, switchableTreeNodeProvider, dependencyPropertyService, false);
                    }
                });

                "SetupStyleInfo VisualTree".Measure(() =>
                {
                    switchableTreeNodeProvider.Switch(SelectorType.VisualTree);
                    foreach (var item in newOrUpdatedStyleHolders)
                    {
                        var start = item.StartFrom ?? item.StyleSheetHolder;
                        if (!switchableTreeNodeProvider.IsInTree(start))
                        {
                            continue;
                        }

                        var domElement = switchableTreeNodeProvider.GetDomElement(start);

                        "EnsureParents".Measure(() =>
                        {
                            EnsureParents(domElement, switchableTreeNodeProvider, styleUpdateInfos);
                        });

                        var discardOldMatchingStyles = newOrUpdatedStyleSheets.Contains(item.StyleSheet);
                        SetupStyleInfo(domElement, item.StyleSheet, styleUpdateInfos, switchableTreeNodeProvider, dependencyPropertyService, discardOldMatchingStyles);
                    }
                });

                var tasks = new List<Task<IList<IDomElement<TDependencyObject>>>>();
                foreach (var item in copy.Select(x => new { x.StartFrom, x.StyleSheetHolder, x.StyleSheet }).Distinct().ToList())
                {
                    var start = item.StartFrom ?? item.StyleSheetHolder;

                    if (!styleUpdateInfos.ContainsKey(start))
                    {
                        continue;
                    }

                    switchableTreeNodeProvider.Switch(SelectorType.LogicalTree);
                    var logical = switchableTreeNodeProvider.GetDomElement(start);
                    logical.StyleInfo = styleUpdateInfos[start];

                    if (!switchableTreeNodeProvider.IsInTree(start))
                    {
                        logical = null;
                    }

                    switchableTreeNodeProvider.Switch(SelectorType.VisualTree);
                    var visual = switchableTreeNodeProvider.GetDomElement(start);
                    visual.StyleInfo = styleUpdateInfos[start];

                    if (!switchableTreeNodeProvider.IsInTree(start))
                    {
                        visual = null;
                    }

                    "UpdateMatchingStyles".Measure(() =>
                    {
                        //var task = Task.Run(() =>
                        //{
                            tasks.Add(Task.FromResult(UpdateMatchingStyles(item.StyleSheet, logical, visual, styleUpdateInfos, dependencyPropertyService, nativeStyleService)));
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
                            nativeStyleService,
                            cssTypeHelper);
                    }
                });
                "ApplyMatchingStyles".Measure(() =>
                {
                    foreach (var item in copy.Select(x => new { x.StartFrom, x.StyleSheetHolder, x.StyleSheet }).Distinct().ToList())
                    {
                        var start = item.StartFrom ?? item.StyleSheetHolder;

                        switchableTreeNodeProvider.Switch(SelectorType.LogicalTree);

                        if (switchableTreeNodeProvider.IsInTree(start))
                        {
                            var logical = switchableTreeNodeProvider.GetDomElement(start);
                            ApplyMatchingStyles(logical, item.StyleSheet, applicationResourcesService, nativeStyleService);
                        }

                        switchableTreeNodeProvider.Switch(SelectorType.VisualTree);
                        if (switchableTreeNodeProvider.IsInTree(start))
                        {
                            var visual = switchableTreeNodeProvider.GetDomElement(start);

                            ApplyMatchingStyles(visual, item.StyleSheet, applicationResourcesService, nativeStyleService);
                        }
                    }
                });
            }
            finally
            {

                applicationResourcesService.EndUpdate();
            }

        }

        private static void SetAttachedToToNewStyleSheet(List<RenderInfo<TDependencyObject, TUIElement>> copy)
        {
            var addedStyleSheets = copy
                            .Where(x => x.RenderTargetKind == RenderTargetKind.Stylesheet &&
                                x.ChangeKind == ChangeKind.New)
                            .Select(x => new { x.StyleSheet, x.StyleSheetHolder })
                            .Distinct()
                            .ToList();

            foreach (var item in addedStyleSheets)
            {
                item.StyleSheet.AttachedTo = item.StyleSheetHolder;
            }
        }

        private static void SetAttachedToToNull(List<RenderInfo<TDependencyObject, TUIElement>> copy)
        {
            var removedStyleSheets = copy
                            .Where(x =>
                                x.RenderTargetKind == RenderTargetKind.Stylesheet &&
                                x.ChangeKind == ChangeKind.Remove)
                            .Select(x => x.StyleSheet)
                            .Distinct()
                            .ToList();

            foreach (var styleSheet in removedStyleSheets)
            {
                styleSheet.AttachedTo = null;
            }
        }

        private static void RemoveOldStyleObjects(List<RenderInfo<TDependencyObject, TUIElement>> copy, INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService, IStyleResourcesService applicationResourcesService)
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

        private static void EnsureParents(IDomElement<TDependencyObject> domElement, ISwitchableTreeNodeProvider<TDependencyObject> switchableTreeNodeProvider,
            IDictionary<TDependencyObject, StyleUpdateInfo> styleUpdateInfos)
        {
            var current = domElement.Parent;
            while (current != null)
            {
                var styleUpdateInfo = current.StyleInfo = current.StyleInfo ?? (styleUpdateInfos.ContainsKey(current.Element) ? styleUpdateInfos[current.Element] : new StyleUpdateInfo
                {
                    MatchedType = current.Element.GetType()
                });

                if ((styleUpdateInfo.DoMatchCheck & switchableTreeNodeProvider.CurrentSelectorType) == switchableTreeNodeProvider.CurrentSelectorType)
                {
                    return;
                }
                
                object a;
                "ClassList".Measure(() => a = current.ClassList);
                "Id".Measure(() => a = current.Id);
                "LocalName".Measure(() => a = current.LocalName);
                "NamespaceUri".Measure(() => a = current.NamespaceUri);
                "Prefix".Measure(() => a = current.Prefix);
                "TagName".Measure(() => a = current.TagName);
                "HasAttribute".Measure(() => a = current.HasAttribute("Name"));
                /*// a = domElement.Parent;
                */

                a = current.ChildNodes;

                current = current.Parent;
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

        private static void ApplyMatchingStyles(IDomElement<TDependencyObject> domElement, StyleSheet styleSheet,
            IStyleResourcesService applicationResourcesService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService)
        {
            var visualElement = domElement.Element;

            if (domElement.StyleInfo == null)
            {
                throw new Exception($"StyleInfo null {domElement.GetType().Name.Replace("DomElement", "")} {domElement.GetPath()}");
            }

            var matchingStyles = domElement.StyleInfo.CurrentMatchedSelectors;
            var appliedMatchingStyles = domElement.StyleInfo.OldMatchedSelectors;

            var styledBy = domElement.StyleInfo.CurrentStyleSheet;
            if (styledBy != null &&
                styledBy != styleSheet)
            {
                // Debug.WriteLine("    Another Stylesheet");
                return;
            }

            domElement.StyleInfo.CurrentStyleSheet = styleSheet;

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

            foreach (var child in domElement.ChildNodes)
            {
                ApplyMatchingStyles(child, styleSheet, applicationResourcesService, nativeStyleService);
            }
        }

        private static IList<IDomElement<TDependencyObject>> UpdateMatchingStyles(
            StyleSheet styleSheet,
            IDomElement<TDependencyObject> startFromLogical,
            IDomElement<TDependencyObject> startFromVisual,
            Dictionary<TDependencyObject, StyleUpdateInfo> styleUpdateInfos,
            IDependencyPropertyService<TDependencyObject, TUIElement, TStyle, TDependencyProperty> dependencyPropertyService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService)
        {
            // var requiredStyleInfos = new List<StyleMatchInfo>();
            IDomElement<TDependencyObject> root = null;

            IDomElement<TDependencyObject> visualTree = null;
            IDomElement<TDependencyObject> logicalTree = null;

            var found = new List<IDomElement<TDependencyObject>>();

            if (startFromVisual?.StyleInfo.DoMatchCheck == SelectorType.None ||
                startFromLogical?.StyleInfo.DoMatchCheck == SelectorType.None)
            {
                return new List<IDomElement<TDependencyObject>>();
            }

            return $"{startFromLogical?.GetPath() ?? startFromVisual?.GetPath() ?? "NULL!?!"}".Measure(() =>
            {

                foreach (var rule in styleSheet.Rules)
                {
                    $"{rule.SelectorString}".Measure(() =>
                    {
                        // // Debug.WriteLine($"--- RULE {rule.SelectorString} ----");
                        if (rule.SelectorType == SelectorType.VisualTree)
                        {
                            if (startFromVisual == null)
                            {
                                //continue;
                                return;
                            }
                            if (visualTree == null)
                            {
                                visualTree = startFromVisual;
                                visualTree?.XamlCssStyleSheets.Clear();
                                visualTree?.XamlCssStyleSheets.Add(styleSheet);
                            }

                            root = visualTree;
                        }
                        else
                        {
                            if (startFromLogical == null)
                            {
                                //continue;
                                return;
                            }
                            if (logicalTree == null)
                            {
                                logicalTree = startFromLogical;
                                logicalTree?.XamlCssStyleSheets.Clear();
                                logicalTree?.XamlCssStyleSheets.Add(styleSheet);
                            }

                            root = logicalTree;
                        }

                        if (root == null)
                        {
                            //continue;
                            return;
                        }

                        // apply our selector
                        var matchedNodes = root.QuerySelectorAllWithSelf(styleSheet, rule.Selectors[0])
                                .Where(x => x != null)
                                .Cast<IDomElement<TDependencyObject>>()
                                .ToList();

                        var matchedElementTypes = matchedNodes
                            .Select(x => x.Element.GetType())
                            .Distinct()
                            .ToList();

                        foreach (var matchingNode in matchedNodes)
                        {
                            var element = matchingNode.Element;

                            found.Add(matchingNode);

                            var resourceKey = nativeStyleService.GetStyleResourceKey(styleSheet.Id, element.GetType(), rule.SelectorString);

                            if (!matchingNode.StyleInfo.CurrentMatchedSelectors.Contains(resourceKey))
                            {
                                matchingNode.StyleInfo.CurrentMatchedSelectors.Add(resourceKey);
                            }
                        }
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
                            selector = CachedSelectorProvider.Instance.GetOrAdd(x.Split('{')[1])
                        })
                                .OrderBy(x => x.selector.IdSpecificity)
                                .ThenBy(x => x.selector.ClassSpecificity)
                                .ThenBy(x => x.selector.SimpleSpecificity)
                                .ToList()
                                .Select(x => x.key)
                                .ToList();
                    }
                });

                "SetDoMatchCheckToNoneInSubTree".Measure(() =>
                {
                    SetDoMatchCheckToNoneInSubTree(startFromLogical, styleSheet);
                    SetDoMatchCheckToNoneInSubTree(startFromVisual, styleSheet);
                });
                return found;
            });
        }

        private static void SetDoMatchCheckToNoneInSubTree(IDomElement<TDependencyObject> domElement, StyleSheet styleSheet)
        {
            if (domElement == null ||
                domElement.StyleInfo.CurrentStyleSheet != styleSheet)
            {
                return;
            }

            domElement.StyleInfo.DoMatchCheck = SelectorType.None;

            foreach (var child in domElement.ChildNodes)
            {
                SetDoMatchCheckToNoneInSubTree(child, styleSheet);
            }
        }

        private static StyleSheet GetStyleSheetFromTree(IDomElement<TDependencyObject> domElement,
            IDependencyPropertyService<TDependencyObject, TUIElement, TStyle, TDependencyProperty> dependencyPropertyService)
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
            ISwitchableTreeNodeProvider<TDependencyObject> switchableTreeNodeProvider,
            IDependencyPropertyService<TDependencyObject, TUIElement, TStyle, TDependencyProperty> dependencyPropertyService,
            bool styleChanged)
        {
            var styleUpdateInfo = "get styleUpdateInfo".Measure(() => domElement.StyleInfo = domElement.StyleInfo ?? (styleUpdateInfos.ContainsKey(domElement.Element) ? styleUpdateInfos[domElement.Element] : new StyleUpdateInfo
            {
                MatchedType = domElement.Element.GetType()
            }));

            if ((styleUpdateInfo.DoMatchCheck & switchableTreeNodeProvider.CurrentSelectorType) == switchableTreeNodeProvider.CurrentSelectorType)
            {
                return;
            }

            var styleSheetFromDom = "GetStyleSheet".Measure(() => dependencyPropertyService.GetStyleSheet(domElement.Element));
            if (styleSheetFromDom != null &&
                styleSheetFromDom != styleSheet)
            {
                // another stylesheet's domelement
                SetupStyleInfo(domElement, styleSheetFromDom, styleUpdateInfos, switchableTreeNodeProvider, dependencyPropertyService, styleChanged);
                return;
            }

            "set styleUpdateInfo values".Measure(() =>
            {
                styleUpdateInfo.CurrentStyleSheet = styleSheet;
                if (styleChanged)
                {
                    styleUpdateInfo.OldMatchedSelectors = new List<string>();
                }
                styleUpdateInfo.CurrentMatchedSelectors.Clear();
                styleUpdateInfo.DoMatchCheck |= switchableTreeNodeProvider.CurrentSelectorType;

                styleUpdateInfos[domElement.Element] = styleUpdateInfo;
            });

            "fill DomElement".Measure(() =>
            {
                object a;
                "ClassList".Measure(() => a = domElement.ClassList);
                "Id".Measure(() => a = domElement.Id);
                "LocalName".Measure(() => a = domElement.LocalName);
                "NamespaceUri".Measure(() => a = domElement.NamespaceUri);
                "Prefix".Measure(() => a = domElement.Prefix);
                "TagName".Measure(() => a = domElement.TagName);
                "HasAttribute".Measure(() => a = domElement.HasAttribute("Name"));
                /*// a = domElement.Parent;
                */
            });

            foreach (var child in domElement.ChildNodes)
            {
                SetupStyleInfo(child, styleSheet, styleUpdateInfos, switchableTreeNodeProvider, dependencyPropertyService, styleChanged);
            }
        }

        private static void GenerateStyles(StyleSheet styleSheet,
            IDictionary<TDependencyObject, StyleUpdateInfo> styleMatchInfos,
            IStyleResourcesService applicationResourcesService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService,
            CssTypeHelper<TDependencyObject, TUIElement, TDependencyProperty, TStyle> cssTypeHelper)
        {
            applicationResourcesService.EnsureResources();

            foreach (var styleMatchInfoKeyValue in styleMatchInfos)
            {
                var styleMatchInfo = styleMatchInfoKeyValue.Value;

                var matchedElementType = styleMatchInfo.MatchedType;

                foreach (var resourceKey in styleMatchInfo.CurrentMatchedSelectors)
                {
                    if (applicationResourcesService.Contains(resourceKey))
                    {
                        continue;
                    }

                    var s = resourceKey.Split('{')[1];

                    var rule = styleMatchInfo.CurrentStyleSheet.Rules.Where(x => x.SelectorString == s).First();
                    // // Debug.WriteLine("Generate Style " + resourceKey);

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
                            Debug.WriteLine($@" ERROR (normal) in Selector ""{rule.SelectorString}"": {error}");
                            styleSheet.AddError($@"ERROR in Selector ""{rule.SelectorString}"": {error}");
                        }

                        var nativeTriggers = $"CreateTriggers ({rule.DeclarationBlock.Triggers.Count})".Measure(() => rule.DeclarationBlock.Triggers
                            .Select(x => nativeStyleService.CreateTrigger(styleSheet, x, styleMatchInfo.MatchedType, (TDependencyObject)styleSheet.AttachedTo))
                            .ToList());


                        var style = "Create Style".Measure(() => nativeStyleService.CreateFrom(propertyStyleValues, nativeTriggers, matchedElementType));

                        applicationResourcesService.SetResource(resourceKey, style);

                        // // Debug.WriteLine("Finished generate Style " + resourceKey);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($@" ERROR (exception) in Selector ""{rule.SelectorString}"": {e.Message}");
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
            CssTypeHelper<TDependencyObject, TUIElement, TDependencyProperty, TStyle> cssTypeHelper)
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
    }
}

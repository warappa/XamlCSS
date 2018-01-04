using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class StyleUpdateInfo
    {
        public StyleSheet OldStyleSheet { get; set; }
        public string[] OldMatchedSelectors { get; set; }
        public StyleSheet CurrentStyleSheet { get; set; }
        public List<string> CurrentMatchedSelectors { get; set; } = new List<string>();
        public Type MatchedType { get; internal set; }
        public bool Reevaluate { get; internal set; }
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
                .Where(x => x.StartsWith(nativeStyleService.BaseStyleResourceKey + "_" + styleSheet.Id, StringComparison.Ordinal))
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
            var stopwatch = new Stopwatch();
            applicationResourcesService.BeginUpdate();

            var cylce = Guid.NewGuid();

            stopwatch.Restart();

            RemoveOldStyleObjects(copy, nativeStyleService, applicationResourcesService);
            stopwatch.Stop();
            Debug.WriteLine($"RemoveOldStyleObjects: {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Restart();
            SetAttachedToToNull(copy);

            stopwatch.Stop();
            Debug.WriteLine($"SetAttachedToToNull: {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Restart();
            SetAttachedToToNewStyleSheet(copy);

            stopwatch.Stop();
            Debug.WriteLine($"SetAttachedToToNewStyleSheet: {stopwatch.ElapsedMilliseconds}ms");

            var styleUpdateInfos = new Dictionary<TDependencyObject, StyleUpdateInfo>();

            var newOrUpdatedStyleHolders = copy
                //.Where(x => x.RenderTargetKind == RenderTargetKind.Stylesheet)
                .Where(x =>
                    x.ChangeKind == ChangeKind.New ||
                    x.ChangeKind == ChangeKind.Update ||
                    x.ChangeKind == ChangeKind.Remove)
                .Select(x => new { x.StartFrom, x.StyleSheet, x.StyleSheetHolder })
                .Distinct()
                .ToList();

           

            switchableTreeNodeProvider.Switch(SelectorType.LogicalTree);
            stopwatch.Restart();

            foreach (var item in newOrUpdatedStyleHolders)
            {
                var start = item.StartFrom ?? item.StyleSheetHolder;
                if (!switchableTreeNodeProvider.IsInTree(start))
                {
                    continue;
                }
                var domElement = switchableTreeNodeProvider.GetDomElement(start);
                SetupStyleInfo(domElement, item.StyleSheet, styleUpdateInfos, switchableTreeNodeProvider, dependencyPropertyService, false);

                EnsureParents(domElement, switchableTreeNodeProvider);
            }
            stopwatch.Stop();
            Debug.WriteLine($"Get full logical tree: {stopwatch.ElapsedMilliseconds}ms");

            switchableTreeNodeProvider.Switch(SelectorType.VisualTree);
            stopwatch.Restart();
            foreach (var item in newOrUpdatedStyleHolders)
            {
                var start = item.StartFrom ?? item.StyleSheetHolder;
                if (!switchableTreeNodeProvider.IsInTree(start))
                {
                    continue;
                }

                var domElement = switchableTreeNodeProvider.GetDomElement(start);
                SetupStyleInfo(domElement, item.StyleSheet, styleUpdateInfos, switchableTreeNodeProvider, dependencyPropertyService, true);

                EnsureParents(domElement, switchableTreeNodeProvider);
            }
            stopwatch.Stop();
            Debug.WriteLine($"Get full visual tree: {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Restart();
            var tasks = new List<Task<IList<IDomElement<TDependencyObject>>>>();
            foreach (var item in copy.Select(x => new { x.StartFrom, x.StyleSheetHolder, x.StyleSheet }).Distinct())
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
                

                //var task = Task.Run(() =>
                //{
                tasks.Add(Task.FromResult(UpdateMatchingStyles(item.StyleSheet, logical, visual, styleUpdateInfos,
                    dependencyPropertyService, nativeStyleService)));

                //});
                //tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();
            Debug.WriteLine($"Match all stylesheets: {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Restart();
            var allFound = tasks.SelectMany(x => x.Result).ToList();
            var allFoundElements = allFound.Select(x => x.Element).ToList();

            var allNotFoundKeys = styleUpdateInfos.Keys.Except(allFoundElements).ToList();

            foreach (var key in allNotFoundKeys)
            {
                var styleUpdateInfo = styleUpdateInfos[key];

                styleUpdateInfo.CurrentMatchedSelectors = new List<string>();
                styleUpdateInfo.OldMatchedSelectors = new string[0];
                styleUpdateInfo.Reevaluate = false;
                // remove style
                nativeStyleService.SetStyle(key, dependencyPropertyService.GetInitialStyle(key));
            }
            stopwatch.Stop();
            Debug.WriteLine($"handle allNotFound: {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Restart();
            foreach (var group in styleUpdateInfos.Where(x => allFoundElements.Contains(x.Key)).GroupBy(x => x.Value.CurrentStyleSheet).ToList())
            {
                GenerateStyles(
                    group.Key,
                    group.ToDictionary(x => x.Key, x => x.Value),
                    applicationResourcesService,
                    nativeStyleService,
                    cssTypeHelper);
            }

            stopwatch.Stop();
            Debug.WriteLine($"Generate styles: {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Restart();
            foreach (var item in copy.Select(x => new { x.StyleSheetHolder, x.StyleSheet }).Distinct())
            {
                switchableTreeNodeProvider.Switch(SelectorType.LogicalTree);
                var logical = switchableTreeNodeProvider.GetDomElement(item.StyleSheetHolder);

                switchableTreeNodeProvider.Switch(SelectorType.VisualTree);
                var visual = switchableTreeNodeProvider.GetDomElement(item.StyleSheetHolder);

                ApplyMatchingStyles(logical, item.StyleSheet, applicationResourcesService, nativeStyleService);
                ApplyMatchingStyles(visual, item.StyleSheet, applicationResourcesService, nativeStyleService);
            }

            stopwatch.Stop();
            Debug.WriteLine($"Apply styles: {stopwatch.ElapsedMilliseconds}ms");

            applicationResourcesService.EndUpdate();
        }

        private static void SetAttachedToToNewStyleSheet(List<RenderInfo<TDependencyObject, TUIElement>> copy)
        {
            var addedStyleSheets = copy
                            .Where(x => x.RenderTargetKind == RenderTargetKind.Stylesheet)
                            .Where(x =>
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
                            .Where(x => x.RenderTargetKind == RenderTargetKind.Stylesheet)
                            .Where(x =>
                                x.ChangeKind == ChangeKind.Remove)
                            .Select(x => new { x.StyleSheet, x.StyleSheetHolder })
                            .Distinct()
                            .ToList();

            foreach (var item in removedStyleSheets)
            {
                item.StyleSheet.AttachedTo = null;
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

        private static void EnsureParents(IDomElement<TDependencyObject> domElement, ISwitchableTreeNodeProvider<TDependencyObject> switchableTreeNodeProvider)
        {
            var current = domElement.Parent;
            while (current != null)
            {
                current = current.Parent;
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

        private static void ApplyMatchingStyles(IDomElement<TDependencyObject> domElement, StyleSheet styleSheet,
            IStyleResourcesService applicationResourcesService,
            INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService)
        {
            // Debug.WriteLine("ApplyMatchingStyles " + Utils.HierarchyDebugExtensions.GetPath(treeNodeProvider.GetDomElement(visualElement)));
            var visualElement = domElement.Element;

            var matchingStyles = domElement.StyleInfo.CurrentMatchedSelectors.ToArray();
            var appliedMatchingStyles = domElement.StyleInfo.OldMatchedSelectors;

            var styledBy = domElement.StyleInfo.CurrentStyleSheet;
            if (styledBy != null &&
                styledBy != styleSheet)
            {
                // Debug.WriteLine("    Another Stylesheet");
                return;
            }


            //dependencyPropertyService.SetStyledByStyleSheet(visualElement, styledBy);
            domElement.StyleInfo.CurrentStyleSheet = styleSheet;


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
                        nativeStyleService.SetStyle(visualElement, null);
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
                    else
                    {
                        nativeStyleService.SetStyle(visualElement, null);
                    }
                }

                //dependencyPropertyService.SetAppliedMatchingStyles(visualElement, matchingStyles);
                domElement.StyleInfo.OldMatchedSelectors = matchingStyles;
            }

            //dependencyPropertyService.SetHandledCss(visualElement, true);

            foreach (var child in domElement.ChildNodes)
            {
                ApplyMatchingStyles(child, styleSheet, applicationResourcesService, nativeStyleService);
            }
            // // Debug.WriteLine($"Applying: {string.Join(", ", dependencyPropertyService.GetMatchingStyles(visualElement) ?? new string[0])}");
        }

        private static IList<IDomElement<TDependencyObject>> UpdateMatchingStyles(
            //TDependencyObject styleResourceReferenceHolder,
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

            if (startFromVisual?.StyleInfo.Reevaluate == false)
            {
                return new List<IDomElement<TDependencyObject>>();
            }

            foreach (var rule in styleSheet.Rules)
            {
                // // Debug.WriteLine($"--- RULE {rule.SelectorString} ----");
                if (rule.SelectorType == SelectorType.VisualTree)
                {
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
                    if (logicalTree == null)
                    {
                        logicalTree = startFromLogical;
                        logicalTree?.XamlCssStyleSheets.Clear();
                        logicalTree?.XamlCssStyleSheets.Add(styleSheet);
                    }

                    root = logicalTree;
                }

                if(root == null)
                {
                    continue;
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

                    // dependencyPropertyService.SetStyledByStyleSheet(element, styleSheet);

                    found.Add(matchingNode);
                    matchingNode.StyleInfo.Reevaluate = false;

                    var resourceKey = nativeStyleService.GetStyleResourceKey(styleSheet.Id, element.GetType(), rule.SelectorString);

                    /*
                    var oldMatchingStyles = dependencyPropertyService.GetMatchingStyles(element) ?? new string[0];
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
                    */

                    if (!matchingNode.StyleInfo.CurrentMatchedSelectors.Contains(resourceKey))
                    {
                        matchingNode.StyleInfo.CurrentMatchedSelectors.Add(resourceKey);
                    }
                    /*
                    if (requiredStyleInfos.Any(x => x.Rule == rule && x.MatchedType == element.GetType()) == false)
                    {
                        requiredStyleInfos.Add(new StyleMatchInfo
                        {
                            Rule = rule,
                            MatchedType = element.GetType()
                        });
                    }*/
                }
            }

            found = found.Distinct().ToList();

            foreach (var f in found)
            {
                f.StyleInfo.Reevaluate = true;

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

            return found;
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
            bool updateCurrentAndOldStyleSheet)
        {
            var styleUpdateInfo = domElement.StyleInfo = styleUpdateInfos.ContainsKey(domElement.Element) ? styleUpdateInfos[domElement.Element] : new StyleUpdateInfo
            {
                MatchedType = domElement.Element.GetType()
            };

            if (styleUpdateInfo.Reevaluate)
            {
                return;
            }

            var styleSheetFromDom = dependencyPropertyService.GetStyleSheet(domElement.Element);
            if (styleSheetFromDom != null &&
                styleSheetFromDom != styleSheet)
            {
                // another stylesheet's domelement
                SetupStyleInfo(domElement, styleSheetFromDom, styleUpdateInfos, switchableTreeNodeProvider, dependencyPropertyService, updateCurrentAndOldStyleSheet);
                return;
            }

            styleUpdateInfo.CurrentStyleSheet = styleSheet;
            styleUpdateInfo.Reevaluate = true;
            styleUpdateInfo.OldMatchedSelectors = styleUpdateInfo.CurrentMatchedSelectors.ToArray();

            styleUpdateInfos[domElement.Element] = styleUpdateInfo;

            object a = domElement.ClassList;
            a = domElement.Id;
            a = domElement.LocalName;
            a = domElement.NamespaceUri;
            a = domElement.Prefix;
            a = domElement.TagName;
            a = domElement.HasAttribute("Name");
            /*// a = domElement.Parent;
            */
            foreach (var child in domElement.ChildNodes)
            {
                SetupStyleInfo(child, styleSheet, styleUpdateInfos, switchableTreeNodeProvider, dependencyPropertyService, updateCurrentAndOldStyleSheet);
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

                foreach (var selector in styleMatchInfo.CurrentMatchedSelectors)
                {
                    var s = selector.Split('{')[1];
                    var rule = styleMatchInfo.CurrentStyleSheet.Rules.Where(x => x.SelectorString == s).First();

                    var resourceKey = nativeStyleService.GetStyleResourceKey(styleSheet.Id, matchedElementType, rule.SelectorString);
                    
                    if (applicationResourcesService.Contains(resourceKey))
                    {
                        continue;
                    }

                    var stopwatch = new Stopwatch();
                    stopwatch.Restart();
                    // // Debug.WriteLine("Generate Style " + resourceKey);

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
                            styleSheet.AddError($@"ERROR in Selector ""{rule.SelectorString}"": {error}");
                        }

                        var nativeTriggers = rule.DeclarationBlock.Triggers
                            .Select(x => nativeStyleService.CreateTrigger(styleSheet, x, styleMatchInfo.MatchedType, (TDependencyObject)styleSheet.AttachedTo))
                            .ToList();

                        //if (!propertyStyleValues.Any() &&
                        //    !nativeTriggers.Any())
                        //{
                        //    // // Debug.WriteLine("no values found -> continue");
                        //    continue;
                        //}

                        var style = nativeStyleService.CreateFrom(propertyStyleValues, nativeTriggers, matchedElementType);

                        applicationResourcesService.SetResource(resourceKey, style);

                        // // Debug.WriteLine("Finished generate Style " + resourceKey);
                    }
                    catch (Exception e)
                    {
                        styleSheet.AddError($@"ERROR in Selector ""{rule.SelectorString}"": {e.Message}");
                    }

                    stopwatch.Stop();
                    Debug.WriteLine($"    {stopwatch.ElapsedMilliseconds}ms for {s}");
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

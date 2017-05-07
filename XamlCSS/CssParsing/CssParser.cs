using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace XamlCSS.CssParsing
{
    public class CssParser
    {
        internal static string defaultCssNamespace;
        internal static ICssFileProvider cssFileProvider;

        public static void Initialize(string defaultCssNamespace, ICssFileProvider cssFileProvider)
        {
            CssParser.defaultCssNamespace = defaultCssNamespace;
            CssParser.cssFileProvider = cssFileProvider;
        }

        public static StyleSheet Parse(string cssDocument, string defaultCssNamespace = null)
        {
            var result = new AstGenerator().GetAst(cssDocument);
            var ast = result.Root;

            var styleSheet = new StyleSheet();

            if (result.Errors.Any() ||
                result.Warnings.Any())
            {
                foreach (var error in result.Errors)
                {
                    styleSheet.Errors.Add(error.Message);
                }

                foreach (var warning in result.Warnings)
                {
                    styleSheet.Warnings.Add(warning.Message);
                }
            }

            var localNamespaces = ast.Children.Where(x => x.Type == CssNodeType.NamespaceDeclaration)
                    .Select(x => new CssNamespace(
                        x.Children.First(y => y.Type == CssNodeType.NamespaceAlias).Text,
                        x.Children.First(y => y.Type == CssNodeType.NamespaceValue).Text.Trim('"')))
                    .ToList();

            styleSheet.LocalNamespaces.AddRange(localNamespaces);

            if (string.IsNullOrEmpty(defaultCssNamespace))
            {
                defaultCssNamespace = CssParser.defaultCssNamespace;
            }

            if (!styleSheet.LocalNamespaces.Any(x => x.Alias == "") &&
                !string.IsNullOrEmpty(defaultCssNamespace))
            {
                styleSheet.LocalNamespaces.Add(new CssNamespace("", defaultCssNamespace));
            }

            var styleRules = ast.Children
                .Where(x => x.Type == CssNodeType.StyleRule)
                .ToList();

            foreach (var astRule in styleRules)
            {
                GetStyleRules(styleSheet, astRule);
            }

            var splitAndOrderedRules = styleSheet.LocalRules
                .SelectMany(rule =>
                {
                    return rule.Selectors.Select(selector =>
                    {
                        return new StyleRule
                        {
                            Selectors = new List<Selector>(new[] { selector }),
                            DeclarationBlock = rule.DeclarationBlock,
                            SelectorString = selector.Value,
                            SelectorType = rule.SelectorType
                        };
                    });
                })
                .OrderBy(x => x.Selectors[0].IdSpecificity)
                .ThenBy(x => x.Selectors[0].ClassSpecificity)
                .ThenBy(x => x.Selectors[0].SimpleSpecificity)
                .ToList();

            styleSheet.LocalRules.Clear();
            styleSheet.LocalRules.AddRange(splitAndOrderedRules);

            return styleSheet;
        }

        private static List<string> GetAllRuleSelectors(List<List<string>> allSelectorLayers)
        {
            return GetAllRuleSelectorsSub(null, allSelectorLayers);
        }

        private static List<string> GetAllRuleSelectorsSub(string baseSelector, List<List<string>> remainingSelectorLayers)
        {
            if (remainingSelectorLayers.Count == 1)
            {
                return remainingSelectorLayers[0]
                    .Select(x => CombineSelectors(baseSelector, x))
                    .ToList();
            }

            var newRemainingSelectorLayers = remainingSelectorLayers.Skip(1).ToList();

            var currentLayerSelectors = remainingSelectorLayers.First();

            var ruleSelectors = new List<string>();
            foreach (var currentLayerSelector in currentLayerSelectors)
            {
                var selector = currentLayerSelector.StartsWith("&") ? currentLayerSelector.Substring(1) : currentLayerSelector;

                ruleSelectors.AddRange(GetAllRuleSelectorsSub(CombineSelectors(baseSelector, selector), newRemainingSelectorLayers));
            }

            return ruleSelectors;
        }

        private static string CombineSelectors(string baseSelector, string currentSelector)
        {
            var isConcatSelector = currentSelector.StartsWith("&");
            var hasBaseSelector = baseSelector != null;
            return $"{(!hasBaseSelector ? "" : baseSelector)}{(!isConcatSelector && hasBaseSelector ? " " : "")}{(isConcatSelector ? "" + currentSelector.Substring(1) : currentSelector)}";
        }

        private static string GetVariableValue(CssNode variableReferenceAst, Dictionary<string, string> parameterValues)
        {
            var variableName = variableReferenceAst.Text;

            if (parameterValues != null &&
                parameterValues.ContainsKey(variableName))
            {
                return parameterValues[variableName];
            }

            var current = variableReferenceAst.Parent;
            CssNode foundDeclaration = null;
            while (current != null)
            {
                if (current.Type == CssNodeType.StyleDeclarationBlock ||
                    current.Type == CssNodeType.Document)
                {
                    foundDeclaration = current.Children
                        .LastOrDefault(x =>
                            x.Type == CssNodeType.VariableDeclaration &&
                            x.Children.Any(y => y.Type == CssNodeType.VariableName && y.Text == variableName));
                }

                if (foundDeclaration != null)
                {
                    return foundDeclaration.Children.First(y => y.Type == CssNodeType.VariableValue).Text;
                }
                current = current.Parent;
            }

            throw new InvalidOperationException($"Variable {variableName} not found!");
        }

        private static List<TriggerAction> GetActionDeclarationsFromBlock(CssNode astStyleDeclarationBlock, Dictionary<string, string> parameterValues)
        {
            return astStyleDeclarationBlock.Children
                .Where(x => x.Type == CssNodeType.ActionDeclaration)
                .Select(x =>
                {
                    var actionAst = x.Children
                             .Single(y => y.Type == CssNodeType.Key);
                    var actionBlockAst = x.Children
                             .Single(y => y.Type == CssNodeType.ActionParameterBlock);

                    var parameters = actionBlockAst.Children
                        .Select(actionParameter =>
                        {
                            var valueAst = actionParameter.Children.Where(c => c.Type == CssNodeType.Value).Single();
                            var val = GetValueFromValueAst(valueAst, parameterValues);

                            return new ActionParameter
                            {
                                Property = actionParameter.Children.Where(c => c.Type == CssNodeType.Key).Single().Text,
                                Value = val
                            };
                        });
                    return new TriggerAction
                    {
                        Action = actionAst.Text,
                        Parameters = parameters.ToList()
                    };
                })
                .ToList();
        }

        private static string GetValueFromValueAst(CssNode valueAst, Dictionary<string, string> parameterValues)
        {
            if (valueAst.Text != "")
            {
                return valueAst.Text;
            }
            var variable = valueAst.Children
                    .FirstOrDefault(y => y.Type == CssNodeType.VariableReference);
            if (variable == null)
            {
                return "";
            }

            return GetVariableValue(variable, parameterValues);
        }

        private static List<StyleDeclaration> GetStyleDeclarationsFromBlock(CssNode astStyleDeclarationBlock, Dictionary<string, string> parameterValues)
        {
            var mixinIncludes = GetMixinIncludes(astStyleDeclarationBlock);

            return mixinIncludes
                .Concat(astStyleDeclarationBlock.Children
                .Where(x => x.Type == CssNodeType.StyleDeclaration)
                .Select(x =>
                {
                    var keyAst = x.Children
                             .Single(y => y.Type == CssNodeType.Key);
                    var valueAst = x.Children
                             .Single(y => y.Type == CssNodeType.Value);
                    return new StyleDeclaration
                    {
                        Property = keyAst.Text,
                        Value = GetValueFromValueAst(valueAst, parameterValues)
                    };
                }))
                .GroupBy(x => x.Property, x => x.Value)
                .Select(x => new StyleDeclaration
                {
                    Property = x.Key,
                    Value = x.Last()
                })
                .ToList();
        }

        private static void GetStyleRules(StyleSheet styleSheet, CssNode astRule)
        {
            var astStyleDeclarationBlock = astRule.Children
                   .Single(x => x.Type == CssNodeType.StyleDeclarationBlock);

            var styleDeclarations = GetStyleDeclarationsFromBlock(astStyleDeclarationBlock, null);

            var propertyTriggers = GetPropertyTriggers(astStyleDeclarationBlock, null);
            var dataTriggers = GetDataTriggers(astStyleDeclarationBlock, null);
            var eventTriggers = GetEventTriggers(astStyleDeclarationBlock, null);

            var triggers = propertyTriggers.Concat(dataTriggers).Concat(eventTriggers).ToList();

            var parentSelectorList = GetParentsSelectorAsts(astRule);
            var parentSelectors = (parentSelectorList?.Select(x => GetSelectorStringsFromSelectorsCssNode(x)) ?? new List<List<string>>()).ToList();


            var currentLevelSelectors = astRule.Children
               .Single(x => x.Type == CssNodeType.Selectors);

            // add current level to parentlevels
            var allSelectorLayers = parentSelectors.Concat(new[] { GetSelectorStringsFromSelectorsCssNode(currentLevelSelectors) })
                .ToList();

            var allSelectorsToUse = GetAllRuleSelectors(allSelectorLayers);

            foreach (var ruleSelectorToUse in allSelectorsToUse)
            {
                var rule = new StyleRule();

                rule.SelectorType = SelectorType.LogicalTree;

                rule.Selectors = new List<Selector>(new[] { new Selector() { Value = ruleSelectorToUse } });

                rule.SelectorString = string.Join(",", rule.Selectors.Select(x => x.Value));

                rule.DeclarationBlock.AddRange(styleDeclarations);
                rule.DeclarationBlock.Triggers = triggers;

                styleSheet.LocalRules.Add(rule);
            }

            ResolveSubRules(styleSheet, astStyleDeclarationBlock);
        }

        private static List<ITrigger> GetPropertyTriggers(CssNode astStyleDeclarationBlock, Dictionary<string, string> parameterValues)
        {
            return astStyleDeclarationBlock.Children
                            .Where(x => x.Type == CssNodeType.PropertyTrigger)
                            .Select(x =>
                            {
                                var propertyAst = x.Children
                                         .Single(y => y.Type == CssNodeType.PropertyTriggerProperty);
                                var valueAst = x.Children
                                         .Single(y => y.Type == CssNodeType.PropertyTriggerValue);

                                var astTriggerStyleDeclarationBlock = x.Children
                                         .Single(y => y.Type == CssNodeType.StyleDeclarationBlock);

                                var enterActions = GetTriggerActions(astTriggerStyleDeclarationBlock, CssNodeType.EnterAction, parameterValues);

                                var exitActions = GetTriggerActions(astTriggerStyleDeclarationBlock, CssNodeType.ExitAction, parameterValues);

                                return new Trigger
                                {
                                    Property = propertyAst.Text,
                                    Value = GetValueFromValueAst(valueAst, parameterValues),
                                    StyleDeclarationBlock = new StyleDeclarationBlock(GetStyleDeclarationsFromBlock(astTriggerStyleDeclarationBlock, null)),
                                    EnterActions = enterActions,
                                    ExitActions = exitActions
                                };
                            })
                            .ToList<ITrigger>();
        }

        private static List<TriggerAction> GetTriggerActions(CssNode astTriggerStyleDeclarationBlock, CssNodeType type, Dictionary<string, string> parameterValues)
        {
            if (type != CssNodeType.EnterAction &&
                type != CssNodeType.ExitAction)
            {
                throw new InvalidOperationException("Type must be either EnterAction or ExitAction");
            }

            var actionDeclarationBlockAst = astTriggerStyleDeclarationBlock.Children
                .Where(y => y.Type == type)
                .SelectMany(y => y.Children)
                .Where(y => y.Type == CssNodeType.ActionDeclarationBlock)
                .SingleOrDefault();

            if (actionDeclarationBlockAst == null)
            {
                return new List<TriggerAction>();
            }

            return GetActionDeclarationsFromBlock(actionDeclarationBlockAst, parameterValues);
        }

        private static List<ITrigger> GetDataTriggers(CssNode astStyleDeclarationBlock, Dictionary<string, string> parameterValues)
        {
            return astStyleDeclarationBlock.Children
                            .Where(x => x.Type == CssNodeType.DataTrigger)
                            .Select(x =>
                            {
                                var bindingAst = x.Children
                                         .Single(y => y.Type == CssNodeType.DataTriggerBinding);
                                var valueAst = x.Children
                                         .Single(y => y.Type == CssNodeType.DataTriggerValue);

                                var astTriggerStyleDeclarationBlock = x.Children
                                         .Single(y => y.Type == CssNodeType.StyleDeclarationBlock);

                                var enterActions = GetTriggerActions(astTriggerStyleDeclarationBlock, CssNodeType.EnterAction, parameterValues);

                                var exitActions = GetTriggerActions(astTriggerStyleDeclarationBlock, CssNodeType.ExitAction, parameterValues);

                                return new DataTrigger
                                {
                                    Binding = bindingAst.Text,
                                    Value = GetValueFromValueAst(valueAst, parameterValues),
                                    StyleDeclarationBlock = new StyleDeclarationBlock(GetStyleDeclarationsFromBlock(astTriggerStyleDeclarationBlock, null)),
                                    EnterActions = enterActions,
                                    ExitActions = exitActions
                                };
                            })
                            .ToList<ITrigger>();
        }

        private static List<ITrigger> GetEventTriggers(CssNode astStyleDeclarationBlock, Dictionary<string, string> parameterValues)
        {
            return astStyleDeclarationBlock.Children
                            .Where(x => x.Type == CssNodeType.EventTrigger)
                            .Select(x =>
                            {
                                var eventAst = x.Children
                                         .Single(y => y.Type == CssNodeType.EventTriggerEvent);

                                var astTriggerActionDeclarationBlock = x.Children
                                         .Single(y => y.Type == CssNodeType.ActionDeclarationBlock);

                                return new EventTrigger
                                {
                                    Event = eventAst.Text,
                                    Actions = new List<TriggerAction>(GetActionDeclarationsFromBlock(astTriggerActionDeclarationBlock, parameterValues))
                                };
                            })
                            .ToList<ITrigger>();
        }

        private static List<StyleDeclaration> GetMixinIncludes(CssNode astStyleDeclarationBlock)
        {
            return astStyleDeclarationBlock.Children
                            .Where(x => x.Type == CssNodeType.MixinInclude)
                            .SelectMany(x =>
                            {
                                var name = x.Text;

                                var astMixinParameters = x.Children
                                         .SingleOrDefault(y => y.Type == CssNodeType.MixinIncludeParameters)
                                         ?.Children
                                         .Select(y => y.Text)
                                         .ToList() ?? new List<string>();

                                return GetMixinStyleDefinitions(astStyleDeclarationBlock, name, astMixinParameters);
                            })
                            .ToList();
        }

        private static List<StyleDeclaration> GetMixinStyleDefinitions(CssNode astStyleDeclarationBlock, string name, List<string> parameterValues)
        {
            var declaration = GetMixinDeclaration(astStyleDeclarationBlock, name);

            if (declaration == null)
                return new List<StyleDeclaration>();

            var parameterDict = new Dictionary<string, string>();
            var parameterAsts = declaration.Children.First(x => x.Type == CssNodeType.MixinParameters).Children;
            for (var i = 0; i < parameterAsts.Count; i++)
            {
                var parameterAst = parameterAsts[i];
                if (i < parameterValues.Count)
                {
                    parameterDict.Add(parameterAst.Text, parameterValues[i]);
                }
                else
                {
                    var defaultValueAst = parameterAst.Children.FirstOrDefault(x => x.Type == CssNodeType.MixinParameterDefaultValue);
                    if (defaultValueAst == null)
                    {
                        throw new InvalidOperationException($"Parameter missing for parameter '{parameterAst.Text}' which has no default value!");
                    }

                    parameterDict.Add(parameterAst.Text, defaultValueAst.Text);
                }
            }

            return GetStyleDeclarationsFromBlock(declaration.Children.First(x => x.Type == CssNodeType.StyleDeclarationBlock), parameterDict);
        }

        private static CssNode GetMixinDeclaration(CssNode astStyleDeclarationBlock, string name)
        {
            var current = astStyleDeclarationBlock.Parent;

            while (current != null)
            {
                var mixinDeclaration = current
                    .Children
                    .LastOrDefault(x =>
                        x.Type == CssNodeType.MixinDeclaration &&
                        x.Text == name);
                if (mixinDeclaration != null)
                {
                    return mixinDeclaration;
                }
                current = current.Parent;
            }
            return null;
        }

        private static List<string> GetSelectorStringsFromSelectorsCssNode(CssNode selectors)
        {
            return selectors.Children
                            .Select(x => string.Join(" ", x.Children /* selector-fragment */.Select(y => y.Text)))
                            .ToList();
        }

        private static List<Selector> GetSelectorFromSelectorsCssNode(CssNode selectors)
        {
            return selectors.Children
                            .Select(x =>
                            {
                                return new Selector
                                {
                                    Value = string.Join(" ", x.Children /* selector-fragment */.Select(y => y.Text))
                                };
                            })
                            .ToList();
        }

        private static void ResolveSubRules(StyleSheet styleSheet, CssNode astStyleDeclarationBlock)
        {
            var subRuleAsts = astStyleDeclarationBlock.Children
                .Where(x => x.Type == CssNodeType.StyleRule)
                .ToList();

            foreach (var subRuleAst in subRuleAsts)
            {
                GetStyleRules(styleSheet, subRuleAst);
            }
        }

        private static IEnumerable<CssNode> GetParentsSelectorAsts(CssNode astRule)
        {
            var list = new List<CssNode>();

            if (astRule.Type == CssNodeType.StyleDeclarationBlock)
            {
                astRule = astRule.Parent;
            }
            astRule = astRule.Parent;
            while (astRule != null &&
                astRule.Type != CssNodeType.StyleRule)
            {
                astRule = astRule.Parent;
            }

            if (astRule == null)
            {
                return new List<CssNode>();
            }

            var selectors = astRule.Children
                .Single(x => x.Type == CssNodeType.Selectors);

            var fromParents = GetParentsSelectorAsts(astRule);

            return fromParents.Concat(new[] { selectors });
        }

        private static bool IsNextTokenOfType(List<CssToken> tokens, int index, CssTokenType type, bool ignoreWhitespace = true)
        {
            return IsNextTokenOfTypes(tokens, index, new[] { type }, ignoreWhitespace);
        }

        private static bool IsNextTokenOfTypes(List<CssToken> tokens, int index, CssTokenType[] types, bool ignoreWhitespace = true)
        {
            int typesIndex = 0;
            index++;

            while (index < tokens.Count)
            {
                if (ignoreWhitespace &&
                    tokens[index].Type == CssTokenType.Whitespace)
                {
                    index++;
                    continue;
                }

                if (tokens[index].Type == types[typesIndex])
                {
                    typesIndex++;
                    if (typesIndex == types.Length)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }

                index++;
            }

            return false;
        }


    }
}

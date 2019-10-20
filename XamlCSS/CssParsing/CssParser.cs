﻿using System;
using System.Collections.Generic;
using System.Linq;
using XamlCSS.Dom;

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

        public static StyleSheet Parse(string document, string defaultCssNamespace = null, IDictionary<string, string> variables = null)
        {
            var result = new AstGenerator().GetAst(document);

            return Parse(result, defaultCssNamespace, variables);
        }

        public static StyleSheet Parse(CssNode document, string defaultCssNamespace = null, IDictionary<string, string> variables = null)
        {
            return Parse(new GeneratorResult
            {
                Errors = new List<LineInfo>(),
                Warnings = new List<LineInfo>(),
                Root = document
            }, defaultCssNamespace, variables);
        }

        public static StyleSheet Parse(GeneratorResult result, string defaultCssNamespace = null, IDictionary<string, string> variables = null)
        {
            var ast = result.Root;

            var styleSheet = new StyleSheet();

            variables = GetVariablesOfBlock(result.Root, variables);

            if (result.Errors.Any() ||
                result.Warnings.Any())
            {
                foreach (var error in result.Errors)
                {
                    styleSheet.Errors.Add(error.Message + $" ({error.Text})");
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

            foreach (var localNamespace in localNamespaces)
            {
                styleSheet.LocalNamespaces.Add(localNamespace);
            }

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
                GetStyleRules(styleSheet, astRule, variables);
            }

            var splitAndOrderedRules = styleSheet.LocalRules
                .SelectMany(rule =>
                {
                    return rule.Selectors.Select(selector =>
                    {
                        return new StyleRule
                        {
                            Selectors = new SelectorCollection(new[] { selector }),
                            DeclarationBlock = new StyleDeclarationBlock(rule.DeclarationBlock, rule.DeclarationBlock.Triggers)
                        };
                    });
                })
                .GroupBy(x => x.SelectorString)
                .Select(x =>
                {
                    var combinedStyleDeclaration = x.First();

                    foreach (var styleDeclarationBlockToImport in x.Skip(1).Select(y => y.DeclarationBlock))
                    {
                        foreach (var styleDeclarationToImport in styleDeclarationBlockToImport)
                        {
                            var existing = combinedStyleDeclaration.DeclarationBlock.FirstOrDefault(y => y.Property == styleDeclarationToImport.Property);
                            if (existing != null)
                            {
                                existing.Value = styleDeclarationToImport.Value;
                            }
                            else
                            {
                                combinedStyleDeclaration.DeclarationBlock.Add(styleDeclarationToImport);
                            }
                        }

                        foreach (var trigger in styleDeclarationBlockToImport.Triggers)
                        {
                            if (trigger is Trigger propertyTrigger)
                            {
                                var existing = combinedStyleDeclaration.DeclarationBlock.Triggers
                                    .OfType<Trigger>()
                                    .FirstOrDefault(y =>
                                        y.Property == propertyTrigger.Property &&
                                        y.Value == propertyTrigger.Value);

                                if (existing != null)
                                {
                                    existing.EnterActions = propertyTrigger.EnterActions;
                                    existing.ExitActions = propertyTrigger.ExitActions;
                                    existing.StyleDeclarationBlock = propertyTrigger.StyleDeclarationBlock;
                                }
                                else
                                {
                                    combinedStyleDeclaration.DeclarationBlock.Triggers.Add(propertyTrigger);
                                }
                            }
                            else if (trigger is EventTrigger eventTrigger)
                            {
                                var existing = combinedStyleDeclaration.DeclarationBlock.Triggers
                                    .OfType<EventTrigger>()
                                    .FirstOrDefault(y => y.Event == eventTrigger.Event);

                                if (existing != null)
                                {
                                    existing.Actions = eventTrigger.Actions;
                                }
                                else
                                {
                                    combinedStyleDeclaration.DeclarationBlock.Triggers.Add(eventTrigger);
                                }
                            }
                            else if (trigger is DataTrigger dataTrigger)
                            {
                                var existing = combinedStyleDeclaration.DeclarationBlock.Triggers
                                    .OfType<DataTrigger>()
                                    .FirstOrDefault(y =>
                                        y.Binding == dataTrigger.Binding &&
                                        y.Value == dataTrigger.Value);

                                if (existing != null)
                                {
                                    existing.EnterActions = dataTrigger.EnterActions;
                                    existing.ExitActions = dataTrigger.ExitActions;
                                    existing.StyleDeclarationBlock = dataTrigger.StyleDeclarationBlock;
                                }
                                else
                                {
                                    combinedStyleDeclaration.DeclarationBlock.Triggers.Add(dataTrigger);
                                }
                            }
                        }
                    }

                    return combinedStyleDeclaration;
                })
                .OrderBy(x => x.Selectors[0].IdSpecificity)
                .ThenBy(x => x.Selectors[0].ClassSpecificity)
                .ThenBy(x => x.Selectors[0].SimpleSpecificity)
                .ToList();

            styleSheet.LocalRules.Clear();
            styleSheet.LocalRules.AddRange(splitAndOrderedRules);

            styleSheet.Variables.Clear();
            foreach (var variable in GetVariables(result.Root))
            {
                styleSheet.Variables.Add(variable);
            }

            return styleSheet;
        }

        private static IDictionary<string, string> GetVariablesOfBlock(CssNode astNode, IDictionary<string, string> variables)
        {
            variables = new Dictionary<string, string>(variables ?? new Dictionary<string, string>());
            foreach (var variable in GetVariables(astNode))
            {
                variables[variable.Key] = variable.Value;
            }

            return variables;
        }

        private static IEnumerable<KeyValuePair<string, string>> GetVariables(CssNode root)
        {
            var dict = new Dictionary<string, string>();

            foreach (var node in root.Children)
            {
                if (node.Type == CssNodeType.VariableDeclaration)
                {
                    var name = node.Children.First(x => x.Type == CssNodeType.VariableName).Text;
                    var variableValue = node.Children.FirstOrDefault(x => x.Type == CssNodeType.VariableValue);
                    var defaultModifier = variableValue.Children.FirstOrDefault(x => x.Type == CssNodeType.VariableDefaultModifier);

                    if (defaultModifier != null &&
                        dict.ContainsKey(name))
                    {
                        continue;
                    }

                    var variableReference = variableValue.Children.FirstOrDefault(x => x.Type == CssNodeType.VariableReference);
                    if (variableReference != null)
                    {
                        dict[name] = GetVariableValue(variableReference, dict);
                    }
                    else
                    {
                        dict[name] = variableValue.Text;
                    }
                }
            }

            return dict;
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
                ruleSelectors.AddRange(GetAllRuleSelectorsSub(CombineSelectors(baseSelector, currentLayerSelector), newRemainingSelectorLayers));
            }

            return ruleSelectors;
        }

        private static string CombineSelectors(string baseSelector, string currentSelector)
        {
            var isConcatSelector = currentSelector.Contains("&");
            var hasBaseSelector = baseSelector != null;

            if (isConcatSelector)
            {
                if (hasBaseSelector)
                {
                    return currentSelector.Replace("&", baseSelector);
                }
            }

            return (hasBaseSelector ? baseSelector + " " : "") + currentSelector;
        }

        private static string GetVariableValue(CssNode variableReferenceAst, IDictionary<string, string> parameterValues)
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
                    foundDeclaration = current.GetVariableDeclaration(variableName);

                    if (foundDeclaration != null)
                    {
                        return foundDeclaration.Children.First(y => y.Type == CssNodeType.VariableValue).Text;
                    }
                }

                current = current.Parent;
            }

            //throw new InvalidOperationException($"Variable {variableName} not found!");

            return null;
        }

        private static List<TriggerAction> GetActionDeclarationsFromBlock(CssNode astStyleDeclarationBlock, IDictionary<string, string> parameterValues)
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

        private static string GetValueFromValueAst(CssNode valueAst, IDictionary<string, string> parameterValues)
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

        private static List<StyleDeclaration> GetStyleDeclarationsFromBlock(CssNode astStyleDeclarationBlock, IDictionary<string, string> parameterValues)
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

        private static IEnumerable<KeyValuePair<string, string>> GetExtends(CssNode astStyleDeclarationBlock)
        {
            var currentSelectors = astStyleDeclarationBlock.Parent.Children
                .First(x => x.Type == CssNodeType.Selectors)
                .Children
                .Where(x => x.Type == CssNodeType.Selector)
                .Select(x => string.Join(" ", x.Children
                    .Where(y => y.Type == CssNodeType.SimpleSelectorSequence)
                    .Select(z => string.Join("", z.Children.Select(c => c.Text))))
                )
                .ToList();

            var selectorsToExtend = astStyleDeclarationBlock
                .Children.Where(x => x.Type == CssNodeType.Extend)
                .Select(x => x.Text)
                .ToList();

            return currentSelectors
                .SelectMany(current => selectorsToExtend
                    .Select(extend => new KeyValuePair<string, string>(current, extend)))
                .ToList();
        }

        private static void GetStyleRules(StyleSheet styleSheet, CssNode astRule, IDictionary<string, string> variables)
        {
            var astStyleDeclarationBlock = astRule.Children
                   .Single(x => x.Type == CssNodeType.StyleDeclarationBlock);

            variables = GetVariablesOfBlock(astStyleDeclarationBlock, variables);

            var styleDeclarations = GetStyleDeclarationsFromBlock(astStyleDeclarationBlock, variables);
            HandleExtends(styleSheet, astStyleDeclarationBlock);

            var propertyTriggers = GetPropertyTriggers(astStyleDeclarationBlock, variables);
            var dataTriggers = GetDataTriggers(astStyleDeclarationBlock, variables);
            var eventTriggers = GetEventTriggers(astStyleDeclarationBlock, variables);

            var triggers = propertyTriggers.Concat(dataTriggers).Concat(eventTriggers).ToList();

            var parentSelectorList = GetParentsSelectorAsts(astRule);
            var parentSelectors = (parentSelectorList.Select(x => GetSelectorStringsFromSelectorsCssNode(x)) ?? new List<List<string>>()).ToList();

            var currentLevelSelectors = astRule.Children
               .Single(x => x.Type == CssNodeType.Selectors);

            // add current level to parentlevels
            var allSelectorLayers = parentSelectors.Concat(new[] { GetSelectorStringsFromSelectorsCssNode(currentLevelSelectors) })
                .ToList();

            var allSelectorsToUse = GetAllRuleSelectors(allSelectorLayers);

            foreach (var ruleSelectorToUse in allSelectorsToUse)
            {
                var rule = new StyleRule();

                rule.Selectors = new SelectorCollection(new[] { CachedSelectorProvider.Instance.GetOrAdd(ruleSelectorToUse) });

                rule.DeclarationBlock.AddRange(styleDeclarations);
                rule.DeclarationBlock.Triggers = triggers;

                styleSheet.LocalRules.Add(rule);
            }

            ResolveSubRules(styleSheet, astStyleDeclarationBlock, variables);
        }

        private static void HandleExtends(StyleSheet styleSheet, CssNode astStyleDeclarationBlock)
        {
            var extends = GetExtends(astStyleDeclarationBlock);

            foreach (var extend in extends)
            {
                var rules = styleSheet.LocalRules
                    .Where(rule => rule.Selectors
                        .Where(selector => selector.Value == extend.Value).Any())
                    .ToList();

                foreach (var rule in rules)
                {
                    rule.Selectors.Add(new Selector
                    {
                        Value = extend.Key
                    });
                }
            }
        }

        private static List<ITrigger> GetPropertyTriggers(CssNode astStyleDeclarationBlock, IDictionary<string, string> parameterValues)
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
                                    Property = GetValueFromValueAst(propertyAst, parameterValues),
                                    Value = GetValueFromValueAst(valueAst, parameterValues),
                                    StyleDeclarationBlock = new StyleDeclarationBlock(GetStyleDeclarationsFromBlock(astTriggerStyleDeclarationBlock, parameterValues)),
                                    EnterActions = enterActions,
                                    ExitActions = exitActions
                                };
                            })
                            .ToList<ITrigger>();
        }

        private static List<TriggerAction> GetTriggerActions(CssNode astTriggerStyleDeclarationBlock, CssNodeType type, IDictionary<string, string> parameterValues)
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

        private static List<ITrigger> GetDataTriggers(CssNode astStyleDeclarationBlock, IDictionary<string, string> parameterValues)
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
                                    Binding = GetValueFromValueAst(bindingAst, parameterValues),
                                    Value = GetValueFromValueAst(valueAst, parameterValues),
                                    StyleDeclarationBlock = new StyleDeclarationBlock(GetStyleDeclarationsFromBlock(astTriggerStyleDeclarationBlock, null)),
                                    EnterActions = enterActions,
                                    ExitActions = exitActions
                                };
                            })
                            .ToList<ITrigger>();
        }

        private static List<ITrigger> GetEventTriggers(CssNode astStyleDeclarationBlock, IDictionary<string, string> parameterValues)
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
            var i = 0;
            foreach (var parameterAst in parameterAsts)
            {
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

                i++;
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
                            .Select(x =>
                            {
                                var list = new List<string>();

                                foreach (var child in x.Children)
                                {
                                    if (child.Type == CssNodeType.SimpleSelectorSequence)
                                    {
                                        list.Add(string.Join("", child.Children.Select(y => y.Text)));
                                    }
                                    else if (child.Type == CssNodeType.GeneralDescendantCombinator ||
                                        child.Type == CssNodeType.DirectDescendantCombinator ||
                                        child.Type == CssNodeType.GeneralSiblingCombinator ||
                                        child.Type == CssNodeType.DirectSiblingCombinator ||
                                        child.Type == CssNodeType.ParentSelector)
                                    {
                                        list.Add(child.Text);
                                    }
                                }
                                return string.Join("", list);
                            })
                            .ToList();
        }

        private static List<Selector> GetSelectorFromSelectorsCssNode(CssNode selectors)
        {
            return selectors.Children
                            .Select(x =>
                            {
                                return new Selector(string.Join(" ", x.Children /* selector-fragment */.Select(y => y.Text)), x);
                            })
                            .ToList();
        }

        private static void ResolveSubRules(StyleSheet styleSheet, CssNode astStyleDeclarationBlock, IDictionary<string, string> variables)
        {
            var subRuleAsts = astStyleDeclarationBlock.Children
                .Where(x => x.Type == CssNodeType.StyleRule)
                .ToList();

            foreach (var subRuleAst in subRuleAsts)
            {
                GetStyleRules(styleSheet, subRuleAst, variables);
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

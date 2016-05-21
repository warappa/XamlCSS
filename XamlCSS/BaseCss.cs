using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
		private string defaultCssNamespace;

		public BaseCss(IDependencyPropertyService<TDependencyObject, TUIElement, TStyle, TDependencyProperty> dependencyPropertyService,
			ITreeNodeProvider<TDependencyObject> treeNodeProvider,
			IStyleResourcesService applicationResourcesService,
			INativeStyleService<TStyle, TDependencyObject, TDependencyProperty> nativeStyleService,
			string defaultCssNamespace)
		{
			this.dependencyPropertyService = dependencyPropertyService;
			this.treeNodeProvider = treeNodeProvider;
			this.applicationResourcesService = applicationResourcesService;
			this.nativeStyleService = nativeStyleService;
			this.defaultCssNamespace = defaultCssNamespace;

			CssParser.Initialize(defaultCssNamespace);
		}

		public void EnqueueRenderStyleSheet(TUIElement styleSheetHolder, StyleSheet styleSheet, TUIElement startFrom)
		{
			GenerateStyleResources(styleSheetHolder, styleSheet, startFrom);
		}

		protected void GenerateStyleResources(TUIElement styleResourceReferenceHolder, StyleSheet styleSheet, TUIElement startFrom)
		{
			if (styleResourceReferenceHolder == null ||
				styleSheet == null)
				return;

			IDomElement<TDependencyObject> root = null;

			IDomElement<TDependencyObject> visualTree = null;
			IDomElement<TDependencyObject> logicalTree = null;

			foreach (var rule in styleSheet.Rules)
			{
				if (rule.SelectorType == SelectorType.VisualTree)
				{
					if (visualTree == null)
					{
						visualTree = treeNodeProvider.GetVisualTree(startFrom ?? styleResourceReferenceHolder, startFrom != null ? treeNodeProvider.GetParent(startFrom) : null);
						visualTree.XamlCssStyleSheets.Add(styleSheet);
					}
					root = visualTree;
				}
				else
				{
					if (logicalTree == null)
					{
						logicalTree = treeNodeProvider.GetLogicalTree(startFrom ?? styleResourceReferenceHolder, startFrom != null ? treeNodeProvider.GetParent(startFrom) : null);
						logicalTree.XamlCssStyleSheets.Add(styleSheet);
					}
					root = logicalTree;
				}

				// apply our selector
				var matchingNodes = root.QuerySelectorAllWithSelf(rule.Selector)
					.Cast<IDomElement<TDependencyObject>>()
					.ToArray();

				var matchingTypes = matchingNodes
					.Select(x => x.Element.GetType())
					.Distinct()
					.ToArray();

				applicationResourcesService.EnsureResources();

				foreach (var type in matchingTypes)
				{
					var resourceKey = nativeStyleService.GetStyleResourceKey(type, rule.Selector);

					if (applicationResourcesService.Contains(resourceKey))
						continue;

					var dict = new Dictionary<TDependencyProperty, object>();

					foreach (var i in rule.DeclarationBlock)
					{
						TDependencyProperty property = null;
						if (i.Property.Contains("."))
						{
							string typename = null;
							string propertyName = null;

							if (i.Property.Contains("|"))
							{
								var strs = i.Property.Split('|', '.');
								var alias = strs[0];
								var namespaceFragments = styleSheet.Namespaces.First(x => x.Alias == alias).Namespace.Split(',');
								typename = $"{namespaceFragments[0]}.{strs[1]}, {string.Join(",", namespaceFragments.Skip(1))}";
								propertyName = strs[2];
							}
							else
							{
								var strs = i.Property.Split('.');
								var namespaceFragments = styleSheet.Namespaces.First(x => x.Alias == "").Namespace.Split(',');
								typename = $"{namespaceFragments[0]}.{strs[0]}, {string.Join(",", namespaceFragments.Skip(1))}";
								propertyName = strs[1];
							}
							property = dependencyPropertyService.GetBindableProperty(Type.GetType(typename), propertyName);
						}
						else
						{
							property = dependencyPropertyService.GetBindableProperty(type, i.Property);
						}

						if (property == null)
						{
							continue;
						}
						var propertyValue = dependencyPropertyService.GetBindablePropertyValue(type, property, i.Value);

						dict[property] = propertyValue;
					}

					if (dict.Keys.Count == 0)
						continue;

					var style = nativeStyleService.CreateFrom(dict, type);
					applicationResourcesService.SetResource(resourceKey, style);
				}

				foreach (var n in matchingNodes)
				{
					var element = n.Element as TDependencyObject;

					var matchingStyles = dependencyPropertyService.GetMatchingStyles(element) ?? new string[0];

					var key = nativeStyleService.GetStyleResourceKey(element.GetType(), rule.Selector);
					if (applicationResourcesService.Contains(key))
					{
						dependencyPropertyService.SetMatchingStyles(element, matchingStyles.Concat(new[] { key }).Distinct().ToArray());
					}
				}
			}

			ApplyMatchingStyles(startFrom ?? styleResourceReferenceHolder);
		}

		public void RemoveStyleResources(TUIElement styleResourceReferenceHolder, StyleSheet styleSheet)
		{
			UnapplyMatchingStyles(styleResourceReferenceHolder);

			var resourceKeys = applicationResourcesService.GetKeys().Cast<object>()
				.OfType<string>()
				.Where(x => x.StartsWith(nativeStyleService.BaseStyleResourceKey, StringComparison.Ordinal))
				.ToArray();

			foreach (var key in resourceKeys)
			{
				applicationResourcesService.RemoveResource(key);
			}
		}

		private void ApplyMatchingStyles(TUIElement visualElement)
		{
			if (visualElement == null)
				return;

			foreach (var child in treeNodeProvider.GetChildren(visualElement).ToArray())
			{
				ApplyMatchingStyles(child as TUIElement);
			}

			var matchingStyles = dependencyPropertyService.GetMatchingStyles(visualElement);
			var appliedMatchingStyles = dependencyPropertyService.GetAppliedMatchingStyles(visualElement);

			if (matchingStyles == appliedMatchingStyles ||
				(matchingStyles != null && appliedMatchingStyles != null && matchingStyles.SequenceEqual(appliedMatchingStyles)))
				return;

			if (matchingStyles?.Length == 1)
			{
				object s = null;
				if (applicationResourcesService.Contains(matchingStyles[0]) == true)
				{
					s = applicationResourcesService.GetResource(matchingStyles[0]);
				}
				nativeStyleService.SetStyle(visualElement, (TStyle)s);
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
					nativeStyleService.SetStyle(visualElement, nativeStyleService.CreateFrom(dict, visualElement.GetType()));
				}
				else
				{
					nativeStyleService.SetStyle(visualElement, null);
				}
			}

			dependencyPropertyService.SetAppliedMatchingStyles(visualElement, matchingStyles);
		}

		public void UnapplyMatchingStyles(TDependencyObject bindableObject)
		{
			if (bindableObject == null)
				return;

			foreach (var child in treeNodeProvider.GetChildren(bindableObject).ToArray())
			{
				UnapplyMatchingStyles(child);
			}

			dependencyPropertyService.SetMatchingStyles(bindableObject, null);
			dependencyPropertyService.SetAppliedMatchingStyles(bindableObject, null);
			nativeStyleService.SetStyle(bindableObject, null);
		}

		public void UpdateElement(TDependencyObject sender)
		{
			EnqueueRenderStyleSheet(
				GetStyleSheetParent(sender as TDependencyObject) as TUIElement,
				GetStyleSheetParentStyleSheet(sender as TUIElement),
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

		public StyleSheet GetStyleSheetParentStyleSheet(TDependencyObject obj)
		{
			var parent = GetStyleSheetParent(obj);
			if (parent == null)
				return null;

			return dependencyPropertyService.GetStyleSheet(parent);
		}
	}
}

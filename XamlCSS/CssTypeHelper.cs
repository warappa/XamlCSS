using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using XamlCSS.CssParsing;

namespace XamlCSS
{
    public class CssTypeHelper<TDependencyObject, TUIElement, TDependencyProperty, TStyle>
        where TDependencyObject : class
        where TUIElement : class, TDependencyObject
        where TStyle : class
        where TDependencyProperty : class
    {
        private IMarkupExtensionParser markupExpressionParser;
        private IDependencyPropertyService<TDependencyObject, TUIElement, TStyle, TDependencyProperty> dependencyPropertyService;

        public CssTypeHelper(IMarkupExtensionParser markupExpressionParser,
            IDependencyPropertyService<TDependencyObject, TUIElement, TStyle, TDependencyProperty> dependencyPropertyService)
        {
            this.markupExpressionParser = markupExpressionParser;
            this.dependencyPropertyService = dependencyPropertyService;
        }

        public bool IsMarkupExtension(string valueExpression)
        {
            if (valueExpression != null &&
                ((valueExpression.StartsWith("#", StringComparison.Ordinal) && !IsHexColorValue(valueExpression)) ||
                valueExpression.StartsWith("{", StringComparison.Ordinal))) // color
            {
                return true;
            }
            return false;
        }

        public object GetMarkupExtensionValue(TDependencyObject targetObject, string valueExpression)
        {
            object propertyValue = null;
            if (IsMarkupExtension(valueExpression))
            {
                if (valueExpression.StartsWith("#"))
                {
                    valueExpression = "{" + valueExpression.Substring(1) + "}";
                }

                propertyValue = markupExpressionParser.ProvideValue(valueExpression, targetObject);
            }

            return propertyValue;
        }

        public object GetPropertyValue(Type matchedType, TDependencyObject targetObject, string valueExpression, TDependencyProperty property)
        {
            object propertyValue;
            if (valueExpression != null &&
                ((valueExpression.StartsWith("#", StringComparison.Ordinal) && !IsHexColorValue(valueExpression)) ||
                valueExpression.StartsWith("{", StringComparison.Ordinal))) // color
            {
                if (valueExpression.StartsWith("#"))
                {
                    valueExpression = "{" + valueExpression.Substring(1) + "}";
                }

                propertyValue = markupExpressionParser.ProvideValue(valueExpression, targetObject);
            }
            else
            {
                propertyValue = dependencyPropertyService.GetBindablePropertyValue(matchedType, property, valueExpression);
            }

            return propertyValue;
        }

        public TDependencyProperty GetDependencyProperty(List<CssNamespace> namespaces, Type matchedType, string propertyExpression)
        {
            TDependencyProperty property;

            var typeAndProperyName = ResolveFullTypeNameAndPropertyName(namespaces, propertyExpression, matchedType);

            property = dependencyPropertyService.GetBindableProperty(Type.GetType(typeAndProperyName.Item1), typeAndProperyName.Item2);

            return property;
        }

        public object GetClrPropertyValue(List<CssNamespace> namespaces, object obj, string propertyExpression)
        {
            var typeAndProperyName = ResolveFullTypeNameAndPropertyName(namespaces, propertyExpression, obj.GetType());

            var type = Type.GetType(typeAndProperyName.Item1);
            return type.GetRuntimeProperty(typeAndProperyName.Item2).GetValue(obj);
        }

        public Type GetClrPropertyType(List<CssNamespace> namespaces, object obj, string propertyExpression)
        {
            var typeAndProperyName = ResolveFullTypeNameAndPropertyName(namespaces, propertyExpression, obj.GetType());

            var type = Type.GetType(typeAndProperyName.Item1);
            return type.GetRuntimeProperty(typeAndProperyName.Item2).PropertyType;
        }

        public Tuple<string, string> ResolveFullTypeNameAndPropertyName(List<CssNamespace> namespaces, string cssPropertyExpression, Type matchedType)
        {
            string typename, propertyName;

            if (cssPropertyExpression.Contains("|"))
            {
                var strs = cssPropertyExpression.Split('|', '.');
                var alias = strs[0];
                var namespaceFragments = namespaces
                    .First(x => x.Alias == alias)
                    .Namespace
                    .Split(',');

                typename = $"{namespaceFragments[0]}.{strs[1]}, {string.Join(",", namespaceFragments.Skip(1))}";
                propertyName = strs[2];
            }
            else if (cssPropertyExpression.Contains("."))
            {
                var strs = cssPropertyExpression.Split('.');
                var namespaceFragments = namespaces
                    .First(x => x.Alias == "")
                    .Namespace
                    .Split(',');

                typename = $"{namespaceFragments[0]}.{strs[0]}, {string.Join(",", namespaceFragments.Skip(1))}";
                propertyName = strs[1];
            }
            else
            {
                typename = matchedType.AssemblyQualifiedName;
                propertyName = cssPropertyExpression;
            }

            return new Tuple<string, string>(typename, propertyName);
        }

        public string ResolveFullTypeName(List<CssNamespace> namespaces, string cssTypeExpression)
        {
            string typename;

            if (cssTypeExpression.Contains("|"))
            {
                var strs = cssTypeExpression.Split('|');
                var alias = strs[0];
                var namespaceFragments = namespaces
                    .FirstOrDefault(x => x.Alias == alias)
                    ?.Namespace
                    .Split(',');

                if (namespaceFragments == null)
                {
                    throw new Exception($@"Namespace ""{alias}"" not found!");
                }

                typename = $"{namespaceFragments[0]}.{strs[1]}, {string.Join(",", namespaceFragments.Skip(1))}";
            }
            else
            {
                var strs = cssTypeExpression.Split('.');
                var namespaceFragments = namespaces
                    .First(x => x.Alias == "")
                    .Namespace
                    .Split(',');

                typename = $"{namespaceFragments[0]}.{strs[0]}, {string.Join(",", namespaceFragments.Skip(1))}";
            }

            return typename;
        }

        private bool IsHexColorValue(string value)
        {
            int dummy;
            return int.TryParse(value.Substring(1), NumberStyles.HexNumber, CultureInfo.CurrentUICulture, out dummy);
        }
    }
}

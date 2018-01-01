using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using XamlCSS.CssParsing;
using XamlCSS.Utils;

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

        public string CreateMarkupExtensionExpression(string valueExpression)
        {
            if (valueExpression.StartsWith("{"))
            {
                return valueExpression;
            }
            else
            {
                return "{" + valueExpression + "}";
            }
        }

        public object GetMarkupExtensionValue(TDependencyObject targetObject, string valueExpression, IEnumerable<CssNamespace> namespaces)
        {
            object propertyValue = null;
            if (IsMarkupExtension(valueExpression))
            {
                if (valueExpression.StartsWith("#"))
                {
                    valueExpression = "{" + valueExpression.Substring(1) + "}";
                }

                propertyValue = markupExpressionParser.ProvideValue(valueExpression, targetObject, namespaces);
            }

            return propertyValue;
        }

        public object GetPropertyValue(Type matchedType, TDependencyObject targetObject, string propertyName, string valueExpression, TDependencyProperty property, IEnumerable<CssNamespace> namespaces)
        {
            object propertyValue;

            if (valueExpression == "inherit")
            {
                valueExpression = $"{{Binding RelativeSource={{RelativeSource AncestorType={{x:Type FrameworkElement}}}}, Path=({matchedType.Name}.{propertyName})}}"; // DependencyProperty.UnsetValue;
            }

            if (valueExpression != null &&
                ((valueExpression.StartsWith("#", StringComparison.Ordinal) && !IsHexColorValue(valueExpression)) ||
                valueExpression.StartsWith("{", StringComparison.Ordinal))) // color
            {
                if (valueExpression.StartsWith("#"))
                {
                    valueExpression = "{" + valueExpression.Substring(1) + "}";
                }

                propertyValue = markupExpressionParser.ProvideValue(valueExpression, targetObject, namespaces);
            }
            else
            {
                propertyValue = dependencyPropertyService.GetBindablePropertyValue(matchedType, propertyName, property, valueExpression);
            }

            return propertyValue;
        }

        public DependencyPropertyInfo<TDependencyProperty> GetDependencyPropertyInfo(IList<CssNamespace> namespaces, Type matchedType, string propertyExpression)
        {
            var typeAndProperyName = ResolveFullTypeNameAndPropertyName(namespaces, propertyExpression, matchedType);

            var declaringType = Type.GetType(typeAndProperyName.Item1);

            return TypeHelpers.DeclaredDependencyProperties<TDependencyProperty>(declaringType)
                .FirstOrDefault(x => x.ShortName == typeAndProperyName.Item2);
        }

        public object GetClrPropertyValue(List<CssNamespace> namespaces, object obj, string propertyExpression)
        {
            var typeAndProperyName = ResolveFullTypeNameAndPropertyName(namespaces, propertyExpression, obj.GetType());

            var type = Type.GetType(typeAndProperyName.Item1);
            return type.GetRuntimeProperty(typeAndProperyName.Item2).GetValue(obj);
        }

        public Type GetClrPropertyType(IList<CssNamespace> namespaces, object obj, string propertyExpression)
        {
            var typeAndProperyName = ResolveFullTypeNameAndPropertyName(namespaces, propertyExpression, obj.GetType());

            var type = Type.GetType(typeAndProperyName.Item1);
            return type.GetRuntimeProperty(typeAndProperyName.Item2).PropertyType;
        }

        public Tuple<string, string> ResolveFullTypeNameAndPropertyName(IList<CssNamespace> namespaces, string cssPropertyExpression, Type matchedType)
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

        public string ResolveFullTypeName(IList<CssNamespace> namespaces, string cssTypeExpression)
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
            return int.TryParse(value.Substring(1), NumberStyles.HexNumber, CultureInfo.CurrentUICulture, out int dummy);
        }
    }
}

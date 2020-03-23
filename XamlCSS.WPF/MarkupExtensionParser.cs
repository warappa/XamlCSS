using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using XamlCSS.Utils;
using XamlCSS.WPF.Dom;

namespace XamlCSS.WPF
{
    public class MarkupExtensionParserSlow : IMarkupExtensionParser
    {
        public const string MarkupParserHelperId = "__markupParserHelper";

        static MarkupExtensionParserSlow()
        {
            
        }

        private static Regex dynamicOrStaticResource = new Regex(@"{\s*(?<ext>StaticResource|DynamicResource)\s*(?<key>[a-zA-Z_0-9]+)\s*}", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        public object Parse(string expression, DependencyObject obj, IEnumerable<CssNamespace> namespaces)
        {
            FrameworkElement textBlock = null;
            object localValue = null;

            var match = dynamicOrStaticResource.Match(expression);
            if (match.Success)
            {
                var ext = match.Groups[1].Value;
                if (ext == "StaticResource")
                    return GetDynamicResourceValue(match.Groups[2].Value, obj);
                return new DynamicResourceExtension(match.Groups[2].Value).ProvideValue(null);
            }

            var xmlnamespaces = string.Join(" ", namespaces.Where(x => x.Alias != "")
                .Select(x =>
                {
                    var strs = x.Namespace.Split(',');
                    if (strs.Length >= 2)
                    {
                        return "xmlns:" + x.Alias + "=\"clr-namespace:" + strs[0] + ";assembly=" + strs[1] + "\"";
                    }
                    else
                    {
                        return "xmlns:" + x.Alias + "=\"" + x.Namespace + "\"";
                    }
                }));

            var test = $@"<FrameworkElement xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" {xmlnamespaces} x:Name=""{MarkupParserHelperId}"" Tag=""{expression}"" />";

            try
            {
                textBlock = (FrameworkElement)XamlReader.Parse(test);
            }
            catch (Exception e)
            {
                throw new Exception($@"Cannot evaluate markup-expression ""{expression}""!");
            }

            try
            {
                localValue = textBlock.ReadLocalValue(FrameworkElement.TagProperty);
            }
            finally
            {
            }

            if (localValue is BindingExpression)
            {
                return ((BindingExpression)localValue).ParentBinding;
            }
            else if ((textBlock.GetValue(FrameworkElement.TagProperty)) == DependencyProperty.UnsetValue)
            {
                return localValue;
            }

            return localValue;
        }

        public object ProvideValue(string expression, object obj, IEnumerable<CssNamespace> namespaces, bool unwrap = true)
        {
            object parseResult = Parse(expression, (DependencyObject)obj, namespaces);

            if (parseResult is Binding binding)
            {
                parseResult = binding.ProvideValue(null);
            }
            else if (unwrap == true &&
                parseResult?.GetType().Name == "ResourceReferenceExpression")
            {
                var resourceKey = TypeHelpers.GetPropertyValue(parseResult, "ResourceKey");

                parseResult = new DynamicResourceExtension(resourceKey);
            }

            return parseResult;
        }

        internal static object GetDynamicResourceValue(object resourceKey, object element)
        {
            if (element is ApplicationDependencyObject)
            {
                Application.Current.TryFindResource(resourceKey);
            }
            else if (element is FrameworkElement)
            {
                return ((FrameworkElement)element).TryFindResource(resourceKey);
            }
            else if (element is FrameworkContentElement)
            {
                return ((FrameworkContentElement)element).TryFindResource(resourceKey);
            }

            return null;
        }
    }
}

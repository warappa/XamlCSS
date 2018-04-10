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

namespace XamlCSS.WPF
{
    public class MarkupExtensionParser : IMarkupExtensionParser
    {
        private static MethodInfo addLogicalChild;
        private static MethodInfo removeLogicalChild;
        public const string MarkupParserHelperId = "__markupParserHelper";

        static MarkupExtensionParser()
        {
            addLogicalChild = typeof(LogicalTreeHelper).GetMethods(BindingFlags.Static | BindingFlags.NonPublic |
                 BindingFlags.FlattenHierarchy)
                .Where(x => x.Name == "AddLogicalChild" && x.GetParameters().Length == 2)
                .First();

            removeLogicalChild = typeof(LogicalTreeHelper).GetMethods(BindingFlags.Static | BindingFlags.NonPublic |
                 BindingFlags.FlattenHierarchy)
                .Where(x => x.Name == "RemoveLogicalChild" && x.GetParameters().Length == 2)
                .First();

            dynamicOrStaticResource.Match("{StaticResource abc}");
        }

        private void AddLogicalChild(DependencyObject parent, object child)
        {
            addLogicalChild.Invoke(null, new object[] { parent, child });
        }

        private void RemoveLogicalChild(DependencyObject parent, object child)
        {
            removeLogicalChild.Invoke(null, new object[] { parent, child });
        }

        private static Regex dynamicOrStaticResource = new Regex(@"{\s*(?<ext>StaticResource|DynamicResource)\s*(?<key>[a-zA-Z_]+)\s*}", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        public object Parse(string expression, FrameworkElement obj, IEnumerable<CssNamespace> namespaces)
        {
            FrameworkElement textBlock = null;
            object localValue = null;

            //var match = $"regex match {expression}".Measure(() => dynamicOrStaticResource.Match(expression));
            //if (match.Success)
            //{
            //    // return GetDynamicResourceValue(match.Groups["key"].Value, obj);
            //    var ext = match.Groups[1].Value;
            //    if(ext == "StaticResource")
            //        return GetDynamicResourceValue(match.Groups[2].Value, obj);
            //    return new DynamicResourceExtension(match.Groups[2].Value);
            //    "GetDynamicResourceValue".Measure(() => GetDynamicResourceValue(match.Groups[1].Value, obj));
            //}

            var xmlnamespaces = "get xaml namespaces".Measure(() => string.Join(" ", namespaces.Where(x => x.Alias != "")
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
                })));
            $"Parse {expression}".Measure(() =>
            {

                //                var test = $@"
                //<DataTemplate
                //xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                //xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                //{xmlnamespaces}><FrameworkElement x:Name=""{MarkupParserHelperId}"" Tag=""{expression}"" /></DataTemplate>";

                var test = $@"<FrameworkElement xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" {xmlnamespaces} x:Name=""{MarkupParserHelperId}"" Tag=""{expression}"" />";

                /*
                var pc = new ParserContext();
                pc.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                pc.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");*/
                //var dataTemplate = (XamlReader.Parse(test) as DataTemplate);


                try
                {
                    //textBlock = (FrameworkElement)dataTemplate.LoadContent();
                    textBlock = (FrameworkElement)XamlReader.Parse(test);
                }
                catch (Exception e)
                {
                    throw new Exception($@"Cannot evaluate markup-expression ""{expression}""!");
                }
                // AddLogicalChild(obj, textBlock);



                try
                {
                    localValue = textBlock.ReadLocalValue(FrameworkElement.TagProperty);
                }
                finally
                {
                    // RemoveLogicalChild(obj, textBlock);
                }

            });

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

        public object ProvideValue(string expression, object obj, IEnumerable<CssNamespace> namespaces)
        {
            object parseResult = Parse(expression, (FrameworkElement)obj, namespaces);

            if (parseResult is Binding binding)
            {
                parseResult = binding.ProvideValue(null);
            }
            else if (parseResult?.GetType().Name == "ResourceReferenceExpression")
            {
                var resourceKey = TypeHelpers.GetPropertyValue(parseResult, "ResourceKey");

                parseResult = new DynamicResourceExtension(resourceKey);
            }

            return parseResult;
        }

        internal static object GetDynamicResourceValue(object resourceKey, object element)
        {
            return "GetDynamicResourceValue".Measure(() =>
            {
                if (element is FrameworkElement)
                {
                    return ((FrameworkElement)element).TryFindResource(resourceKey);
                }
                else if (element is FrameworkContentElement)
                {
                    return ((FrameworkContentElement)element).TryFindResource(resourceKey);
                }

                return null;
            });
        }
    }
}

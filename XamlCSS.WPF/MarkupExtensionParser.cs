using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml.Linq;
using XamlCSS.CssParsing;

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
        }

        private void AddLogicalChild(DependencyObject parent, object child)
        {
            addLogicalChild.Invoke(null, new object[] { parent, child });
        }

        private void RemoveLogicalChild(DependencyObject parent, object child)
        {
            removeLogicalChild.Invoke(null, new object[] { parent, child });
        }

        public object Parse(string expression, FrameworkElement obj, IEnumerable<CssNamespace> namespaces)
        {
            var wasStaticResourceExtension = expression.Replace(" ", "").StartsWith("{StaticResource");
            if (wasStaticResourceExtension)
            {
                expression = expression.Replace("StaticResource", "DynamicResource");
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

            var test = $@"
<TextBlock 
xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
{xmlnamespaces}
	 x:Name=""{MarkupParserHelperId}"" Tag=""{expression}"">
</TextBlock>";
            /*
            var pc = new ParserContext();
            pc.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            pc.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");*/
            //var dataTemplate = (XamlReader.Parse(test) as DataTemplate);

            TextBlock textBlock;
            try
            {
                //textBlock = (TextBlock)dataTemplate.LoadContent();
                textBlock = (TextBlock)XamlReader.Parse(test);
            }
            catch (Exception e)
            {
                throw new Exception($@"Cannot evaluate markup-expression ""{expression}""!");
            }
            AddLogicalChild(obj, textBlock);

            object localValue;
            object resolvedValue;

            try
            {
                localValue = textBlock.ReadLocalValue(FrameworkElement.TagProperty);
                resolvedValue = textBlock.GetValue(FrameworkElement.TagProperty);
            }
            finally
            {
                RemoveLogicalChild(obj, textBlock);
            }

            if (localValue is BindingExpression)
            {
                return ((BindingExpression)localValue).ParentBinding;
            }
            else if (wasStaticResourceExtension)
            {
                return resolvedValue;
            }
            else if (resolvedValue == DependencyProperty.UnsetValue)
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
                return binding.ProvideValue(null);
            }

            if (parseResult?.GetType().Name == "ResourceReferenceExpression")
            {
                var resourceKeyProperty = parseResult.GetType().GetProperty("ResourceKey");

                return new DynamicResourceExtension(resourceKeyProperty.GetValue(parseResult));
            }

            return parseResult;
        }
    }
}

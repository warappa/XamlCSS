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

namespace XamlCSS.WPF
{
    public class MarkupExtensionParser : IMarkupExtensionParser
    {
        private static MethodInfo addLogicalChild;
        private static MethodInfo removeLogicalChild;

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

        public object Parse(string expression)
        {
            string myBindingExpression = expression;
            var test = "<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Tag=\"" + myBindingExpression + "\" />";

            var result = XamlReader.Parse(test) as TextBlock;

            var bindingExpression = result.ReadLocalValue(FrameworkElement.TagProperty);
            var binding = bindingExpression;

            if (binding is BindingExpression)
            {
                binding = ((BindingExpression)binding).ParentBinding;
            }

            return binding;
        }
        public object Parse(string expression, FrameworkElement obj)
        {
            var wasStaticResourceExtension = expression.Replace(" ", "").StartsWith("{StaticResource");
            if (wasStaticResourceExtension)
            {
                expression = expression.Replace("StaticResource", "DynamicResource");
            }

            var test = $@"
<DataTemplate DataType=""{{x:Type x:String}}"">
	<TextBlock x:Name=""aaa"" Tag=""{expression}"" />
</DataTemplate>";

            var pc = new ParserContext();
            pc.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            pc.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");

            var dataTemplate = (XamlReader.Parse(test, pc) as DataTemplate);

            var textBlock = (TextBlock)dataTemplate.LoadContent();
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

        public object ProvideValue(string expression, object obj)
        {
            object parseResult = Parse(expression, (FrameworkElement)obj);

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

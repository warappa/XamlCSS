using System.IO;
using System.Linq;
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
        public object Parse(string expression)
        {
            string myBindingExpression = expression;
            var test = "<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" DataContext=\"" + myBindingExpression + "\" />";

            var result = XamlReader.Parse(test) as TextBlock;

            var bindingExpression = result.ReadLocalValue(TextBlock.DataContextProperty);
            var binding = bindingExpression;

            if (binding is BindingExpression)
            {
                binding = ((BindingExpression)binding).ParentBinding;
            }

            return binding;
        }
        public object Parse(string expression, ResourceDictionary resourceDictionary)
        {
            string myBindingExpression = expression;

            var dummyResourceDict = new ResourceDictionary();
            foreach (var key in resourceDictionary.Keys)
            {
                dummyResourceDict.Add(key, key);
            }

            var resDictString = resourceDictionary != null ? XamlWriter.Save(dummyResourceDict) : "";
            string inner = null;
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(resDictString)))
            {
                var doc = XDocument.Parse(resDictString);

                inner = doc.Descendants().First()?.ToString();
            }

            var test = $@"
<StackPanel xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
	<StackPanel.Resources>
		{ inner ?? "" }
	</StackPanel.Resources>
	<TextBlock DataContext=""{myBindingExpression}"" />
</StackPanel>";

            var result = (XamlReader.Parse(test) as StackPanel).Children[0];
            var bindingExpression = result.ReadLocalValue(TextBlock.DataContextProperty);

            var binding = bindingExpression;

            if (binding is BindingExpression)
            {
                binding = ((BindingExpression)binding).ParentBinding;
            }
            else if (resourceDictionary.Keys.OfType<object>().Contains(binding))
            {
                binding = resourceDictionary[binding];
            }

            return binding;
        }

        private void GetAllResourceDictionaries(object obj, ResourceDictionary dict)
        {
            if (obj == null)
            {
                return;
            }
            ResourceDictionary resDict = null;

            if (obj is FrameworkElement)
            {
                GetAllResourceDictionaries((obj as FrameworkElement).Parent, dict);
                resDict = (obj as FrameworkElement).Resources;
            }
            else if (obj is FrameworkContentElement)
            {
                GetAllResourceDictionaries((obj as FrameworkContentElement).Parent, dict);
                resDict = (obj as FrameworkContentElement).Resources;
            }

            foreach (var key in resDict.Keys)
            {
                dict[key] = resDict[key];
            }
        }

        public object ProvideValue(string expression, object obj)
        {
            ResourceDictionary resDict = new ResourceDictionary();

            GetAllResourceDictionaries(obj, resDict);

            object binding = null;
            if (expression.Contains("Binding "))
            {
                binding = Parse(expression);
            }
            else
            {
                binding = Parse(expression, resDict);
            }

            if (binding is Binding)
            {
                return (binding as Binding).ProvideValue(null);
            }

            if (binding.GetType().Name == "ResourceReferenceExpression")
            {
                var a = binding.GetType().GetProperty("ResourceKey");

                return new DynamicResourceExtension(a.GetValue(binding));
            }

            return binding;
        }
    }
}

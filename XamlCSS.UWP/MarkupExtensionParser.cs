using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace XamlCSS.UWP
{
	public class MarkupExtensionParser : IMarkupExtensionParser
	{
		public object Parse(string expression)
		{
			string myBindingExpression = expression;
			var test = "<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" DataContext=\"" + myBindingExpression + "\" />";

			var result = XamlReader.Load(test) as TextBlock;
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
			var list = new List<string>(resourceDictionary.Keys.Select(x =>
				$@"<x:String x:Key=""{x.ToString()}"">{x.ToString()}</x:String>"));

			string inner = string.Join(" ", list);

			var test = $@"
<StackPanel xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
	<StackPanel.Resources>
		{ inner }
	</StackPanel.Resources>
	<TextBlock DataContext=""{myBindingExpression}"" />
</StackPanel>";

			var result = (XamlReader.Load(test) as StackPanel).Children[0];

			var bindingExpression = result.ReadLocalValue(TextBlock.DataContextProperty);

			var binding = bindingExpression;

			if (binding is BindingExpression)
			{
				binding = ((BindingExpression)binding).ParentBinding;
			}
			else if (
				binding is string &&
				resourceDictionary.Keys.Contains(binding as string))
			{
				binding = resourceDictionary[binding as string];
			}

			return binding;
		}

		protected IEnumerable<FrameworkElement> GetParents(FrameworkElement obj)
		{
			var parent = obj;
			while (parent != null)
			{
				yield return parent;
				parent = ((dynamic)parent).Parent;
			}
		}

		public object ProvideValue(string expression, object obj)
		{
			var resDict = new ResourceDictionary();
			foreach (var parent in GetParents(obj as FrameworkElement))
			{
				foreach (var i in parent.Resources)
				{
					resDict.Add(i);
				}
			}

			var binding = Parse(expression);

			if (binding is Binding)
			{
				return (binding as Binding);
			}

			return binding;
		}
	}
}

using System.Collections.Generic;
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
            var test = $"<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Tag=\"{ expression }\" />";
            
            var result = XamlReader.Load(test) as TextBlock;
			var bindingExpression = result.ReadLocalValue(TextBlock.TagProperty);

			var binding = bindingExpression;

			if (binding is BindingExpression)
			{
				binding = ((BindingExpression)binding).ParentBinding;
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
			var binding = Parse(expression);

			if (binding is Binding)
			{
				return (binding as Binding);
			}

			return binding;
		}
	}
}

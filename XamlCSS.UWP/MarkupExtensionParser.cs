using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using XamlCSS.CssParsing;

namespace XamlCSS.UWP
{
    public class MarkupExtensionParser : IMarkupExtensionParser
    {
        public const string MarkupParserHelperId = "__markupParserHelper";

        private object AddLogicalChild(FrameworkElement parent, object child)
        {
            if (parent == null)
            {
                return null;
            }

            var panel = parent as Panel;
            if (panel != null)
            {
                panel.Children.Add(child as UIElement);
                return null;
            }

            var contentControl = parent as ContentControl;
            if (contentControl != null)
            {
                var oldContent = contentControl.Content;
                contentControl.Content = child;
                return oldContent;
            }

            if (parent.Parent != null)
            {
                return AddLogicalChild((FrameworkElement)parent.Parent, child);
            }

            return null;
        }

        private void RemoveLogicalChild(FrameworkElement parent, object child, object oldContent)
        {
            if (parent == null)
            {
                return;
            }

            var panel = parent as Panel;
            if (panel != null)
            {
                panel.Children.Remove(child as UIElement);
                return;
            }

            var contentControl = parent as ContentControl;
            if (contentControl != null)
            {
                contentControl.Content = oldContent;
                return;
            }

            RemoveLogicalChild((FrameworkElement)parent.Parent, child, oldContent);
        }

        private object Parse(string expression, FrameworkElement obj, IEnumerable<CssNamespace> namespaces)
        {
            var test = $@"
<DataTemplate 
xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
{string.Join(" ", namespaces.Where(x => x.Alias != "").Select(x => "xmlns:" + x.Alias + "=\"using:" + x.Namespace.Split(',')[0] + "\""))}
>
	<TextBlock x:Name=""{MarkupParserHelperId}"" Tag=""{expression}"" />
</DataTemplate>";

            TextBlock textBlock;
            try
            {
                var dataTemplate = (XamlReader.Load(test) as DataTemplate);
                textBlock = (TextBlock)dataTemplate.LoadContent();
            }
            catch (Exception e)
            {
                throw new Exception($@"Cannot evaluate markup-expression ""{expression}""!");
            }

            var oldContent = AddLogicalChild(obj, textBlock);

            object localValue;
            object resolvedValue;

            try
            {
                localValue = textBlock.ReadLocalValue(FrameworkElement.TagProperty);
                resolvedValue = textBlock.GetValue(FrameworkElement.TagProperty);
            }
            finally
            {
                RemoveLogicalChild(obj, textBlock, oldContent);
            }

            if (localValue is BindingExpression)
            {
                return ((BindingExpression)localValue).ParentBinding;
            }
            else if (resolvedValue == DependencyProperty.UnsetValue)
            {
                return localValue;
            }

            return localValue;
        }

        public object ProvideValue(string expression, object obj, IEnumerable<CssNamespace> namespaces, bool unwrap = true)
        {
            return Parse(expression, (FrameworkElement)obj, namespaces);
        }
    }
}

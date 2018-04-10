using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using XamlCSS.CssParsing;
using XamlCSS.Utils;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class StyleSheetTests
    {
        [Test]
        public void Can_parse_rule_to_ast()
        {
            var mapping = new Dictionary<string, List<string>>
            {
                {
                    "http://schemas.microsoft.com/winfx/2006/xaml/presentation",
                    new List<string>
                    {
                        typeof(System.Windows.Data.Binding).AssemblyQualifiedName.Replace(".Binding,", ","),
                        typeof(System.Windows.Navigation.NavigationWindow).AssemblyQualifiedName.Replace(".NavigationWindow,", ","),
                        typeof(System.Windows.Shapes.Rectangle).AssemblyQualifiedName.Replace(".Rectangle,", ","),
                        typeof(System.Windows.Controls.Button).AssemblyQualifiedName.Replace(".Button,", ","),
                        typeof(System.Windows.FrameworkElement).AssemblyQualifiedName.Replace(".FrameworkElement,", ","),
                        typeof(System.Windows.Documents.Run).AssemblyQualifiedName.Replace(".Run,", ",")
                    }
                }
            };

            TypeHelpers.Initialze(mapping);

            CssParser.Initialize(typeof(System.Windows.Controls.Button).AssemblyQualifiedName.Replace(".Button,", ","), null);

            var stylesheetBase = new StyleSheet();
            stylesheetBase.Content = @"@namespace ""http://schemas.microsoft.com/winfx/2006/xaml/presentation"";";

            stylesheetBase.GetNamespaceUri("", "FlowDocument").Should().Be("System.Windows.Documents, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            var dependent = new StyleSheet();
            dependent.SingleBaseStyleSheet = stylesheetBase;

            // inherits defaultnamespace from stylesheetBase
            dependent.GetNamespaceUri("", "FlowDocument").Should().Be("System.Windows.Documents, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            var dependent2 = new StyleSheet();
            dependent2.SingleBaseStyleSheet = stylesheetBase;
            // overwrite inherited defaultnamespace from stylesheetBase
            dependent2.Content = $@"@namespace ""{typeof(System.Windows.Controls.Button).AssemblyQualifiedName.Replace(".Button,", ",")}"";";

            dependent.GetNamespaceUri("", "FlowDocument").Should().Be("System.Windows.Documents, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            dependent2.GetNamespaceUri("", "FlowDocument").Should().Be("System.Windows.Controls, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            stylesheetBase.Content = @"";
            
            dependent.GetNamespaceUri("", "FlowDocument").Should().Be("System.Windows.Controls, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            dependent2.GetNamespaceUri("", "FlowDocument").Should().Be("System.Windows.Controls, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            stylesheetBase.Content = @"@namespace ""http://schemas.microsoft.com/winfx/2006/xaml/presentation"";";

            // inherited namespace doesn't matter because it is overwritten in dependent stylesheets
            dependent.GetNamespaceUri("", "FlowDocument").Should().Be("System.Windows.Controls, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            dependent2.GetNamespaceUri("", "FlowDocument").Should().Be("System.Windows.Controls, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        }
    }
}

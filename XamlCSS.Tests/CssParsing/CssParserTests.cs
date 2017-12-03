using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class CssParserTests
    {
        string css = @"
@namespace xamlcss ""XamlCss"";
.main .sub>div xamlcss|Button {
	background-color: red;
	background: #00ff00, solid, url('aaa');
	Grid.Row: 1;
}
";

        [Test]
        public void TestParseCss()
        {
            var styleSheet = CssParser.Parse(css);

            Assert.AreEqual(1, styleSheet.Rules.Count);
        }

        [Test]
        public void TestParseCssWithoutSpaces()
        {
            var styleSheet = CssParser.Parse("Button{Foreground:Red;}");

            Assert.AreEqual(1, styleSheet.Rules.Count);
            Assert.AreEqual("Button", styleSheet.Rules[0].SelectorString);
            Assert.AreEqual("Foreground", styleSheet.Rules[0].DeclarationBlock[0].Property);
        }

        [Test]
        public void Test_can_set_attached_property()
        {
            var styleSheet = CssParser.Parse("Button{Grid.Row:1;}");

            Assert.AreEqual(1, styleSheet.Rules.Count);
            Assert.AreEqual("Button", styleSheet.Rules[0].SelectorString);
            Assert.AreEqual("Grid.Row", styleSheet.Rules[0].DeclarationBlock[0].Property);
        }

        [Test]
        public void Test_can_parse_namespace()
        {
            var styleSheet = CssParser.Parse(@"@namespace ui ""System.Windows.Controls"";");

            Assert.AreEqual(1, styleSheet.Namespaces.Count());
            Assert.AreEqual("ui", styleSheet.Namespaces[0].Alias);
            Assert.AreEqual("System.Windows.Controls", styleSheet.Namespaces[0].Namespace);
        }

        [Test]
        public void Test_can_parse_namespace2()
        {
            var styleSheet = CssParser.Parse(@"@namespace ui ""System.Windows.Controls, PresentationFramework, Version = 4.0.0.0, Culture = neutral, PublicKeyToken = 31bf3856ad364e35"";");

            Assert.AreEqual(1, styleSheet.Namespaces.Count());
            Assert.AreEqual("ui", styleSheet.Namespaces[0].Alias);
            Assert.AreEqual("System.Windows.Controls, PresentationFramework, Version = 4.0.0.0, Culture = neutral, PublicKeyToken = 31bf3856ad364e35", styleSheet.Namespaces[0].Namespace);
        }

        [Test]
        public void Test_can_parse_namespace3()
        {
            var styleSheet = CssParser.Parse(@"
@namespace ""default"";
@namespace ui ""System.Windows.Controls, PresentationFramework, Version = 4.0.0.0, Culture = neutral, PublicKeyToken = 31bf3856ad364e35"";
.test
{
	ui|Grid.Row: 0;
	ui|Grid.Column: 1;
}");

            Assert.AreEqual(2, styleSheet.Namespaces.Count());
            Assert.AreEqual("", styleSheet.Namespaces[0].Alias);
            Assert.AreEqual("ui", styleSheet.Namespaces[1].Alias);
            Assert.AreEqual("default", styleSheet.Namespaces[0].Namespace);
            Assert.AreEqual("System.Windows.Controls, PresentationFramework, Version = 4.0.0.0, Culture = neutral, PublicKeyToken = 31bf3856ad364e35", styleSheet.Namespaces[1].Namespace);
            Assert.AreEqual("ui|Grid.Row", styleSheet.Rules[0].DeclarationBlock[0].Property);
            Assert.AreEqual("ui|Grid.Column", styleSheet.Rules[0].DeclarationBlock[1].Property);
        }

        [Test]
        public void Test_can_parse_markupExtensions()
        {
            var styleSheet = CssParser.Parse(@"
.test
{
	Text: #Binding testValue;
	Background: Green;
    Foreground: #ff00ff;
}");

            Assert.AreEqual(3, styleSheet.Rules[0].DeclarationBlock.Count);
            Assert.AreEqual("#Binding testValue", styleSheet.Rules[0].DeclarationBlock[0].Value);
            Assert.AreEqual("Green", styleSheet.Rules[0].DeclarationBlock[1].Value);
            Assert.AreEqual("#ff00ff", styleSheet.Rules[0].DeclarationBlock[2].Value);
        }

        [Test]
        public void Test_can_parse_markupExtensions_xaml_style()
        {
            var styleSheet = CssParser.Parse(@"
.test
{
	Text: ""{Binding testValue}"";
	Background: Green;
    Foreground: #ff00ff;
}");

            Assert.AreEqual(3, styleSheet.Rules[0].DeclarationBlock.Count);
            Assert.AreEqual("{Binding testValue}", styleSheet.Rules[0].DeclarationBlock[0].Value);
            Assert.AreEqual("Green", styleSheet.Rules[0].DeclarationBlock[1].Value);
            Assert.AreEqual("#ff00ff", styleSheet.Rules[0].DeclarationBlock[2].Value);
        }

        [Test]
        public void Rules_ordered_by_specificity()
        {
            var styleSheet = CssParser.Parse(@"

.warning#some-element,
#an-element,
.warning
{
}
.important
{   
}
Button
{

}
");

            Assert.AreEqual(5, styleSheet.Rules.Count);
            Assert.AreEqual("Button", styleSheet.Rules[0].Selectors[0].Value);
            Assert.AreEqual("1", styleSheet.Rules[0].Selectors[0].Specificity);

            Assert.AreEqual(".warning", styleSheet.Rules[1].Selectors[0].Value);
            Assert.AreEqual("1,0", styleSheet.Rules[1].Selectors[0].Specificity);

            Assert.AreEqual(".important", styleSheet.Rules[2].Selectors[0].Value);
            Assert.AreEqual("1,0", styleSheet.Rules[2].Selectors[0].Specificity);

            Assert.AreEqual("#an-element", styleSheet.Rules[3].Selectors[0].Value);
            Assert.AreEqual("1,0,0", styleSheet.Rules[3].Selectors[0].Specificity);

            Assert.AreEqual(".warning#some-element", styleSheet.Rules[4].Selectors[0].Value);
            Assert.AreEqual("1,1,0", styleSheet.Rules[4].Selectors[0].Specificity);

            var s = new Selector { Value = ".important-button-container>Button" };
            Assert.AreEqual("1,1", s.Specificity);
        }

        [Test]
        public void Test_can_parse_empty_string()
        {
            var styleSheet = CssParser.Parse(@"
.test
{
	Text: """";
    Width: 0;
}");

            Assert.AreEqual(2, styleSheet.Rules[0].DeclarationBlock.Count);
            Assert.AreEqual(@"", styleSheet.Rules[0].DeclarationBlock[0].Value);
            Assert.AreEqual("0", styleSheet.Rules[0].DeclarationBlock[1].Value);
        }

        [Test]
        public void Can_parse_singlequote_string_literal()
        {
            var styleSheet = CssParser.Parse(@"
.test
{
	Text: 'pack://application,,,/Assets/Fonts/#FontAwesome';
    Width: 0;
}");

            Assert.AreEqual(2, styleSheet.Rules[0].DeclarationBlock.Count);
            Assert.AreEqual(@"pack://application,,,/Assets/Fonts/#FontAwesome", styleSheet.Rules[0].DeclarationBlock[0].Value);
            Assert.AreEqual("0", styleSheet.Rules[0].DeclarationBlock[1].Value);
        }

        [Test]
        public void Can_parse_singlequote_string_literal_with_escaped_singlequotes()
        {
            var styleSheet = CssParser.Parse(@"
.test
{
	Text: 'pack://application,,,/Assets/Fonts/#\'FontAwesome';
    Width: 0;
}");

            Assert.AreEqual(2, styleSheet.Rules[0].DeclarationBlock.Count);
            Assert.AreEqual(@"pack://application,,,/Assets/Fonts/#'FontAwesome", styleSheet.Rules[0].DeclarationBlock[0].Value);
            Assert.AreEqual("0", styleSheet.Rules[0].DeclarationBlock[1].Value);
        }

        [Test]
        public void Can_parse_doublequote_string_literal()
        {
            var styleSheet = CssParser.Parse(@"
.test
{
	Text: ""pack://application,,,/Assets/Fonts/#FontAwesome"";
    Width: 0;
}");

            Assert.AreEqual(2, styleSheet.Rules[0].DeclarationBlock.Count);
            Assert.AreEqual(@"pack://application,,,/Assets/Fonts/#FontAwesome", styleSheet.Rules[0].DeclarationBlock[0].Value);
            Assert.AreEqual("0", styleSheet.Rules[0].DeclarationBlock[1].Value);
        }

        [Test]
        public void Can_parse_doublequote_string_literal_with_escaped_doublequotes()
        {
            var styleSheet = CssParser.Parse(@"
.test
{
	Text: ""pack://application,,,/Assets/Fonts/#\""FontAwesome"";
    Width: 0;
}");

            Assert.AreEqual(2, styleSheet.Rules[0].DeclarationBlock.Count);
            Assert.AreEqual(@"pack://application,,,/Assets/Fonts/#""FontAwesome", styleSheet.Rules[0].DeclarationBlock[0].Value);
            Assert.AreEqual("0", styleSheet.Rules[0].DeclarationBlock[1].Value);
        }

        [Test]
        public void Can_parse_double()
        {
            var styleSheet = CssParser.Parse(@"
.test
{
	Opacity: 0.5;
    Width: 0;
}");

            Assert.AreEqual(2, styleSheet.Rules[0].DeclarationBlock.Count);
            Assert.AreEqual(@"0.5", styleSheet.Rules[0].DeclarationBlock[0].Value);
            Assert.AreEqual("0", styleSheet.Rules[0].DeclarationBlock[1].Value);
        }

        [Test]
        public void Can_parse_color()
        {
            var styleSheet = CssParser.Parse(@"
.test
{
	Color: #ff00ff;
}");

            Assert.AreEqual(1, styleSheet.Rules[0].DeclarationBlock.Count);
            Assert.AreEqual(@"#ff00ff", styleSheet.Rules[0].DeclarationBlock[0].Value);
        }

        [Test]
        public void Can_parse_square_brackets_in_selector()
        {
            var styleSheet = CssParser.Parse(@"
.test[Text=""hallo""],
.test[Text='hallo'],
.test[Text=hallo]
{
	Color: #ff00ff;
}");

            Assert.AreEqual(1, styleSheet.Rules[0].DeclarationBlock.Count);
            styleSheet.Rules[0].Selectors[0].Value.Should().Be(@".test[Text=""hallo""]");
            styleSheet.Rules[1].Selectors[0].Value.Should().Be(@".test[Text='hallo']");
            styleSheet.Rules[2].Selectors[0].Value.Should().Be(@".test[Text=hallo]");
            Assert.AreEqual(@"#ff00ff", styleSheet.Rules[0].DeclarationBlock[0].Value);
        }

        [Test]
        public void Issue_50_Parser_should_not_throw_if_document_only_contains_at_character()
        {
            StyleSheet styleSheet = null;
            Action action = () => styleSheet = CssParser.Parse(@"@");

            action.ShouldNotThrow();
            styleSheet.Errors.Count.Should().Be(1);
            styleSheet.Errors.First().Contains("Next:");
        }

        [Test]
        public void Issue_50_Parser_should_not_hang_if_document_only_contains_at_character()
        {
            StyleSheet styleSheet = null;
            Action action = () => styleSheet = CssParser.Parse(@"@a");

            action.ShouldNotThrow();
            styleSheet.Errors.Count.Should().Be(2);
            styleSheet.Errors[0].Should().Contain("unexpected token");
            styleSheet.Errors[1].Should().Contain("Reached end of tokens");
        }
    }
}

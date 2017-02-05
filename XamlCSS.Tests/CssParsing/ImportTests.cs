using FluentAssertions;
using NUnit.Framework;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class ImportTests
    {
        [Test]
        public void Can_import_css_file()
        {
            CssParser.Initialize(null, new TestCssContentProvider());
            var css = @"
@import 'CssParsing\\TestData\\ImportCss.scss';

SomeElement {
    @include InputElement();

    TextColor: $textColor;
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(2);

            styleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("Grid.Column");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("1");
            styleSheet.Rules[0].DeclarationBlock[1].Property.Should().Be("TextColor");
            styleSheet.Rules[0].DeclarationBlock[1].Value.Should().Be("Red");
        }

        [Test]
        public void Can_override_imported_css_file()
        {
            CssParser.Initialize(null, new TestCssContentProvider());
            var css = @"
@import 'CssParsing\\TestData\\ImportCss.scss';

$textColor: Pink;

@mixin InputElement()
{
    Background: Orange;
}

SomeElement
{
    @include InputElement();

    TextColor: $textColor;
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(2);

            styleSheet.Rules[0].SelectorString.Should().Be("SomeElement");
            styleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("Background");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("Orange");
            styleSheet.Rules[0].DeclarationBlock[1].Property.Should().Be("TextColor");
            styleSheet.Rules[0].DeclarationBlock[1].Value.Should().Be("Pink");

            styleSheet.Rules[1].SelectorString.Should().Be(".header");
            styleSheet.Rules[1].DeclarationBlock[0].Property.Should().Be("TextColor");
            styleSheet.Rules[1].DeclarationBlock[0].Value.Should().Be("Red");
            styleSheet.Rules[1].DeclarationBlock[1].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[1].DeclarationBlock[1].Value.Should().Be("White");
        }
    }
}

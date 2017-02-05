using FluentAssertions;
using NUnit.Framework;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class MixinTests
    {
        [Test]
        public void Can_parse_simple_mixin()
        {
            var css = @"
@mixin RedBackground()
{
    BackgroundColor: Red;
}

.header {
    @include RedBackground();

    TextColor: Green;
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(1);

            styleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("Red");
            styleSheet.Rules[0].DeclarationBlock[1].Property.Should().Be("TextColor");
            styleSheet.Rules[0].DeclarationBlock[1].Value.Should().Be("Green");
        }

        [Test]
        public void Can_parse_simple_mixin_with_parameters()
        {
            var css = @"
@mixin Colored($textColor, $backgroundColor)
{
    TextColor: $textColor;
    BackgroundColor: $backgroundColor;
}

.header {
    @include Colored(Red, Green);

    HeightRequest: 200;
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(1);

            styleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("TextColor");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("Red");
            styleSheet.Rules[0].DeclarationBlock[1].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[0].DeclarationBlock[1].Value.Should().Be("Green");
            styleSheet.Rules[0].DeclarationBlock[2].Property.Should().Be("HeightRequest");
            styleSheet.Rules[0].DeclarationBlock[2].Value.Should().Be("200");
        }

        [Test]
        public void Can_parse_nested_mixin_with_parameters()
        {
            var css = @"
@mixin Colored($textColor, $backgroundColor)
{
    @include Nested(24);

    TextColor: $textColor;
    BackgroundColor: $backgroundColor;
}

@mixin Nested($fontSize)
{
    FontSize: $fontSize;
}

.header {
    @include Colored(Red, Green);

    HeightRequest: 200;
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(1);

            styleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("FontSize");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("24");
            styleSheet.Rules[0].DeclarationBlock[1].Property.Should().Be("TextColor");
            styleSheet.Rules[0].DeclarationBlock[1].Value.Should().Be("Red");
            styleSheet.Rules[0].DeclarationBlock[2].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[0].DeclarationBlock[2].Value.Should().Be("Green");
            styleSheet.Rules[0].DeclarationBlock[3].Property.Should().Be("HeightRequest");
            styleSheet.Rules[0].DeclarationBlock[3].Value.Should().Be("200");
        }

        [Test]
        public void Can_parse_mixin_with_default_parameters()
        {
            var css = @"
@mixin Colored($textColor, $backgroundColor:""Yellow"")
{
    TextColor: $textColor;
    BackgroundColor: $backgroundColor;
}

.header {
    @include Colored(Red);

    HeightRequest: 200;
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(1);

            styleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("TextColor");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("Red");
            styleSheet.Rules[0].DeclarationBlock[1].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[0].DeclarationBlock[1].Value.Should().Be("Yellow");
            styleSheet.Rules[0].DeclarationBlock[2].Property.Should().Be("HeightRequest");
            styleSheet.Rules[0].DeclarationBlock[2].Value.Should().Be("200");
        }

        [Test]
        public void Can_parse_mixin_with_missing_parameter_without_default_value_should_throw_exception()
        {
            var css = @"
@mixin Colored($textColor, $backgroundColor:""Yellow"")
{
    TextColor: $textColor;
    BackgroundColor: $backgroundColor;
}

.header {
    @include Colored();

    HeightRequest: 200;
}
";
            Assert.That(() => CssParser.Parse(css), Throws.InvalidOperationException.With.Message.Contains("$textColor"));
        }
    }
}

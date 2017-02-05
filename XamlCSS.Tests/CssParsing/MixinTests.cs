using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}

using FluentAssertions;
using NUnit.Framework;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture] 
    public class SpecificyTests
    {
        [Test]
        public void Asterisk_should_be_0()
        {
            var target = new Selector();
            target.Value = "*";

            target.Specificity.Should().Be("0");
        }

        [Test]
        public void Complex_selector_should_be_121()
        {
            var target = new Selector();
            target.Value = "#nav .selected > a:hover";

            target.Specificity.Should().Be("1,2,1");
        }

        [Test]
        public void Complex_selector_with_attribute_should_be_131()
        {
            var target = new Selector();
            target.Value = "#nav .selected[text=abc] > a:hover";

            target.Specificity.Should().Be("1,3,1");
        }
    }
}

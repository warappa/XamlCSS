using FluentAssertions;
using NUnit.Framework;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class CssNamespaceTests
    {
        [Test]
        public void Equals_if_alias_and_namespace_matches()
        {
            var ns1 = new CssNamespace("", "defaultnamespace");
            var ns2 = new CssNamespace("", "defaultnamespace");
            var ns3 = new CssNamespace("otherAlias", "defaultnamespace");
            var ns4 = new CssNamespace("", "otherDefaultnamespace");

            ns1.Equals(ns2).Should().Be(true);
            ns1.Equals(ns3).Should().Be(false);
            ns1.Equals(ns4).Should().Be(false);

            ns1.Equals(null).Should().Be(false);
        }

        [Test]
        public void GetHashCode_should_be_equal_if_alias_and_namespace_matches()
        {
            var ns1 = new CssNamespace("", "defaultnamespace");
            var ns2 = new CssNamespace("", "defaultnamespace");
            var ns3 = new CssNamespace("otherAlias", "defaultnamespace");
            var ns4 = new CssNamespace("", "otherDefaultnamespace");

            ns1.GetHashCode().Should().Be(ns2.GetHashCode());
            ns1.GetHashCode().Should().NotBe(ns3.GetHashCode());
            ns1.GetHashCode().Should().NotBe(ns4.GetHashCode());
        }
    }
}

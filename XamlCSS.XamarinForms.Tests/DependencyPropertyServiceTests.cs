using FluentAssertions;
using NUnit.Framework;
using System;
using Xamarin.Forms;

namespace XamlCSS.XamarinForms.Tests
{
    [TestFixture]
    public class DependencyPropertyServiceTests
    {
        private DependencyPropertyService target;

        [Flags]
        public enum TestEnum
        {
            One = 1,
            Two = 2,
            All = 3
        }

        [SetUp]
        public void Initialize()
        {
            target = new DependencyPropertyService();
        }

        [Test]
        public void Can_parse_simple_enum()
        {
            var res = (TestEnum)target.GetClrValue(typeof(TestEnum), "Two");

            res.Should().Be(TestEnum.Two);
        }

        [Test]
        public void Can_parse_multiple_flags()
        {
            var res = (TestEnum)target.GetClrValue(typeof(TestEnum), "One,Two");

            res.Should().Be(TestEnum.One | TestEnum.Two);
        }

        [Test]
        public void Can_parse_rectangle_with_4_parameters()
        {
            var res = (Rectangle)target.GetClrValue(typeof(Rectangle), "1,2,3,4");

            res.Should().Be(new Rectangle(1, 2, 3, 4));
        }

        [Test]
        public void Can_parse_Thickness_with_4_parameters()
        {
            var res = (Thickness)target.GetClrValue(typeof(Thickness), "1,2,3,4");

            res.Should().Be(new Thickness(1, 2, 3, 4));
        }

        [Test]
        public void Can_parse_Thickness_with_2_parameters()
        {
            var res = (Thickness)target.GetClrValue(typeof(Thickness), "1,2");

            res.Should().Be(new Thickness(1, 2, 1, 2));
        }
    }
}

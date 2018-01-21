using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using XamlCSS.Utils;

namespace XamlCSS.WPF.Tests
{
    [TestFixture]
    public class ResolveTypeTests
    {
        private List<CssNamespace> namespaces;
        private Dictionary<string, List<string>> mapping;

        [TestFixtureSetUp]
        public void Setup()
        {
            namespaces = new List<CssNamespace>
            {
                new CssNamespace("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation")
            };

            mapping = new Dictionary<string, List<string>>
            {
                {
                    "http://schemas.microsoft.com/winfx/2006/xaml/presentation",
                    new List<string>
                    {
                        typeof(Button).AssemblyQualifiedName.Replace(".Button", "")
                    }
                }
            };

            TypeHelpers.Initialze(mapping, true);
        }

        [Test]
        public void Can_map_namespaceUri_to_assemblyqualifiedtypename()
        {
            var type = TypeHelpers.ResolveFullTypeName(namespaces, "Button");

            type.Should().Be(typeof(Button).AssemblyQualifiedName);
        }
    }
}

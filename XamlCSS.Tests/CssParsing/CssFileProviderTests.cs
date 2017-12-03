using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using XamlCSS.CssParsing;
using System.Collections.Generic;
using System.Reflection;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class CssFileProviderTests
    {
        private ConcreteCssFileProvider target;

        [SetUp]
        public void Initialize()
        {
            target = new ConcreteCssFileProvider(new[] { typeof(CssFileProviderTests).Assembly });
        }

        [Test]
        public void Can_load_from_embedded_resource()
        {
            var result = target.LoadFrom("CssParsing.TestData.ImportCssEmbedded.scss");

            result.Should().NotBeNull();
        }

        [Test]
        public void Can_load_from_file()
        {
            var result = target.LoadFrom("CssParsing\\TestData\\ImportCss.scss");

            result.Should().NotBeNull();
        }

        [Test]
        public void Nonexisting_source_returns_null()
        {
            var result = target.LoadFrom("nonexisting");

            result.Should().BeNull();
        }

        public class ConcreteCssFileProvider : CssFileProviderBase
        {
            public ConcreteCssFileProvider(IEnumerable<Assembly> assemblies) : base(assemblies)
            {

            }

            protected override Stream TryGetFromFile(string source)
            {
                try
                {
                    return File.OpenRead(source);
                }
                catch
                {
                    return null;
                }
            }

            protected override Stream TryLoadFromStaticApplicationResource(string source)
            {
                return null;
            }
        }
    }
}

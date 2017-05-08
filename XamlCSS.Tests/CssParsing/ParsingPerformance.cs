using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class ParsingPerformance
    {
        private string css;
        private int iterations = 1;

        [SetUp]
        public void Setup()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XamlCSS.Tests.CssParsing.TestData.BigCss.scss"))
            using (var reader = new StreamReader(stream))
            {
                css = reader.ReadToEnd();
            }
        }

        [Test]
        public void Test_parse_performance()
        {
            for (var i = 0; i < iterations; i++)
            {
                ParseIteration();
            }
        }

        [Test]
        public void Test_ast_generation_performance()
        {
            for (var i = 0; i < iterations; i++)
            {
                AstGenerationIteration();
            }
        }

        [Test]
        public void Test_tokenzie_performance()
        {
            for (var i = 0; i < iterations; i++)
            {
                TokenizeGenerationIteration();
            }
        }
        Stopwatch stopwatch = new Stopwatch();
        private void ParseIteration()
        {
            stopwatch.Start();

            var stylesheet = CssParser.Parse(css);
            stopwatch.Stop();

            Debug.WriteLine($"Parsing big css: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Reset();
        }

        private void AstGenerationIteration()
        {
            stopwatch.Start();

            var stylesheet = new AstGenerator().GetAst(css);
            stopwatch.Stop();

            Debug.WriteLine($"Ast big css: {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Reset();
        }

        private void TokenizeGenerationIteration()
        {
            stopwatch.Start();

            var stylesheet = Tokenizer.Tokenize(css);
            stopwatch.Stop();

            Debug.WriteLine($"Tokenize big css: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Reset();
        }
    }
}

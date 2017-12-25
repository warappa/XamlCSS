using NUnit.Framework;
using System.Linq;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.Dom
{
    [TestFixture]
    public class CssSelectionTests
    {
        private CssParser selectorEngine;
        string test1 = @"
@namespace ""System.Windows.Controls"";
@namespace xamlcss ""XamlCSS"";
@namespace ui ""System.Windows.Controls"";

.main .sub>div xamlcss|Button {
	background-color: red;
	background: #00ff00;
}
ui|Grid
{
	Background: red;
}
";
        private TestNode dom;
        private TestNode body;
        private TestNode div;
        private TestNode label1;
        private TestNode label2;
        private TestNode label3;
        private TestNode grid;
        private TestNode section;
        private TestNode gridSpecial;

        [TestFixtureSetUp]
        public void Setup()
        {
            this.selectorEngine = new CssParser();
        }

        [SetUp]
        public void TestSetup()
        {
            TestNamespaceProvider.Instance.prefixToNamespaceUri["special"] = "A.Namespace";
            //TestNamespaceProvider.Instance.prefixToNamespaceUri["ui"] = typeof(TestNode).Namespace;
            TestNamespaceProvider.Instance.prefixToNamespaceUri[""] = typeof(TestNode).Namespace;

            dom = (body = new TestNode(null, "body",
                new[] {(section = new TestNode(null, "section", new[]
                {
                (div = new TestNode(null, "div", new[]
                {
                    (label1 = new TestNode(null, "label")),
                    (label2 = new TestNode(null, "label", null,null, "label2")),
                    (label3 = new TestNode(null, "label", null,null, null, "required")),
                    (grid = new TestNode(null, "Grid", null,null, null)),
                    (gridSpecial = new TestNode(null, "special|Grid", null,null, null))
                }))
                }))
            }));

            Assert.AreEqual("label", label2.TagName);
            Assert.AreEqual("label2", label2.Id);
            Assert.AreEqual("required", label3.ClassName);

            Assert.AreEqual(div, label2.Parent);
        }

        [Test]
        public void Select_on_root_node_descending_nodes_by_tag()
        {
            var styleSheet = CssParser.Parse(test1);

            var nodes = dom.QuerySelectorAllWithSelf("body label");

            Assert.AreEqual(3, nodes.Count());
        }

        [Test]
        public void Select_on_root_node_descending_node_with_tag_and_class()
        {
            var styleSheet = CssParser.Parse(test1);

            var nodes = dom.QuerySelectorAllWithSelf("body label.required");

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(label3, nodes.First());
        }

        [Test]
        public void Select_on_sub_node_descending_node_with_tag_and_class()
        {
            var styleSheet = CssParser.Parse(test1);

            var nodes = div.QuerySelectorAllWithSelf("div label.required");

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(label3, nodes.First());
        }

        [Test]
        public void Select_by_ascending_node_with_tag_and_class2()
        {
            var styleSheet = CssParser.Parse(test1);

            var nodes = div.QuerySelectorAllWithSelf("label.required");

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(label3, nodes.First());
        }

        [Test]
        public void Select_ascending_node_with_tag_and_class3()
        {
            var styleSheet = CssParser.Parse(test1);

            var nodes = div.QuerySelectorAllWithSelf("body label.required");

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(label3, nodes.First());
        }

        [Test]
        public void Select_ascending_node_with_tag_and_class4()
        {
            var nodes = label3.QuerySelectorAllWithSelf("body label.required");

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(label3, nodes.First());
        }

        [Test]
        public void Select_ascending_node_with_nth_child()
        {
            var nodes = label2.QuerySelectorAllWithSelf("label:nth-of-type(2)");

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(label2, nodes.First());
        }

        [Test]
        public void Parser_loads_namespaces()
        {
            var styleSheet = CssParser.Parse(test1);

            Assert.AreEqual(3, styleSheet.Namespaces.Count());

            Assert.AreEqual("", styleSheet.Namespaces[0].Alias);
            Assert.AreEqual("System.Windows.Controls", styleSheet.Namespaces[0].Namespace);

            Assert.AreEqual("xamlcss", styleSheet.Namespaces[1].Alias);
            Assert.AreEqual("XamlCSS", styleSheet.Namespaces[1].Namespace);

            Assert.AreEqual("ui", styleSheet.Namespaces[2].Alias);
            Assert.AreEqual("System.Windows.Controls", styleSheet.Namespaces[2].Namespace);
        }

        [Test]
        public void Select_without_namespace()
        {
            var nodes = dom.QuerySelectorAllWithSelf("Grid");

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(grid, nodes.First());
        }

        [Test]
        public void Select_with_namespace()
        {
            var nodes = dom.QuerySelectorAllWithSelf("special|Grid");

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(gridSpecial, nodes.First());
        }
    }
}

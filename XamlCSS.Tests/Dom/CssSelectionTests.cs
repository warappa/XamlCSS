using AngleSharp.Dom;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

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

        [TestFixtureSetUp]
        public void Setup()
        {
            this.selectorEngine = new CssParser();
        }

        [SetUp]
        public void TestSetup()
        {
            dom = (body = new TestNode(null, "body",
                new[] {(section = new TestNode(null, "section", new[]
                {
                (div = new TestNode(null, "div", new[]
                {
                    (label1 = new TestNode(null, "label")),
                    (label2 = new TestNode(null, "label", null,null, "label2")),
                    (label3 = new TestNode(null, "label", null,null, null, "required")),
                    (grid = new TestNode(null, "Grid", null,null, null))
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
            var nodes = dom.QuerySelectorAllWithSelf("ui|Grid");

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(grid, nodes.First());
        }
    }

    [TestFixture]
    public class SelectorTests
    {
        [Test]
        public void Test()
        {
            var selector = new Selector(".button a> #bbb");

            selector.Fragments.Select(x => x.Text).ShouldBeEquivalentTo(new[] { ".button", " ", "a", ">", "#bbb" }.ToList());
        }

        [Test]
        public void Can_match_id()
        {
            var selector = new Selector("#bbb");

            var tag = GetDomElement("button", "bbb");
            selector.Match(tag).Should().Be(true);
        }

        [Test]
        public void Can_match_tagname()
        {
            var selector = new Selector("button");

            var tag = GetDomElement("button", "bbb");
            selector.Match(tag).Should().Be(true);
        }

        [Test]
        public void Can_match_class()
        {
            var selector = new Selector(".important");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            selector.Match(tag).Should().Be(true);
        }

        [Test]
        public void Can_match_tagname_with_class()
        {
            var selector = new Selector("button.important");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            selector.Match(tag).Should().Be(true);
        }

        [Test]
        public void Can_match_id_with_class()
        {
            var selector = new Selector("#bbb.important");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            selector.Match(tag).Should().Be(true);
        }

        public IDomElement<object> GetDomElement(string tagname, string id, string classes = "")
        {
            return new TestDomElement(tagname, id, classes);
        }
    }

    public class TestDomElement : DomElementBase<object, object>
    {
        public TestDomElement(string tagname, string id, string classes = "")
            : base(new object(), A.Fake<ITreeNodeProvider<object>>())
        {
            this.TagName = tagname;
            Id = id;


            var c = new TokenList();
            c.AddRange(classes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            classList = c;
        }

        protected override IHtmlCollection<IElement> CreateCollection(IEnumerable<IElement> list)
        {
            throw new System.NotImplementedException();
        }

        protected override INamedNodeMap CreateNamedNodeMap(object dependencyObject)
        {
            throw new System.NotImplementedException();
        }

        protected override INodeList CreateNodeList(IEnumerable<INode> nodes)
        {
            throw new System.NotImplementedException();
        }

        protected override IHtmlCollection<IElement> GetChildElements(object dependencyObject)
        {
            throw new System.NotImplementedException();
        }

        protected override INodeList GetChildNodes(object dependencyObject)
        {
            throw new System.NotImplementedException();
        }

        protected override ITokenList GetClassList(object dependencyObject)
        {
            return new TokenList();
        }

        protected override string GetId(object dependencyObject)
        {
            return id;
        }
    }
}

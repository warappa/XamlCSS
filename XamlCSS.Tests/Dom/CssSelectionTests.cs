using NUnit.Framework;
using System.Linq;
using System.Reflection;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS.Tests.Dom
{
    [TestFixture]
    public class CssSelectionTests
    {
        private CssParser selectorEngine;
        string test1 = $@"
@namespace ""{TestNode.GetAssemblyQualifiedNamespaceName(typeof(TestNode))}"";
@namespace xamlcss ""{TestNode.GetAssemblyQualifiedNamespaceName(typeof(XamlCSS.CssNamespace))}"";
@namespace ui ""{TestNode.GetAssemblyQualifiedNamespaceName(typeof(TestNode))}"";
@namespace special ""{TestNode.GetAssemblyQualifiedNamespaceName(typeof(TestNode))}"";

.main .sub>div xamlcss|Button {{
	background-color: red;
	background: #00ff00;
}}
ui|Grid
{{
	Background: red;
}}
";
        private StyleSheet defaultStyleSheet;
        private TestNode dom;
        private TestNode body;
        private TestNode div;
        private TestNode div1;
        private TestNode label1;
        private TestNode label2;
        private TestNode label3;
        private TestNode grid;
        private TestNode section;
        private TestNode gridSpecial;

        [OneTimeSetUp]
        public void Setup()
        {
            this.selectorEngine = new CssParser();
        }

        [SetUp]
        public void TestSetup()
        {
            CssParser.defaultCssNamespace = "XamlCSS.Tests.Dom, XamlCSS.Tests, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null";
            defaultStyleSheet = CssParser.Parse(test1);
            
            dom = (body = new TestNode(new UIElement(), null, "body",
                new[] {(section = new TestNode(new UIElement(), null, "section", new[]
                {
                    (div1 = new TestNode(new UIElement(), null, "div", new[]{
                (div = new TestNode(new UIElement(), null, "div", new[]
                {
                    (label1 = new TestNode(new UIElement(), null, "label")),
                    (label2 = new TestNode(new UIElement(), null, "label", null,null, "label2")),
                    (label3 = new TestNode(new UIElement(), null, "label", null,null, null, "required")),
                    (grid = new TestNode(new UIElement(), null, "Grid", null,null, null)),
                    (gridSpecial = new TestNode(new UIElement(), null, "special|Grid", null,null, null))
                })) }))
                }))
            }));

            SetCurrentStyleSheet(dom, defaultStyleSheet);

            Assert.AreEqual("label", label2.TagName);
            Assert.AreEqual("label2", label2.Id);
            Assert.AreEqual("required", label3.ClassName);

            Assert.AreEqual(div, label2.Parent);
        }

        private void SetCurrentStyleSheet(IDomElement<UIElement, PropertyInfo> domElement, StyleSheet styleSheet)
        {
            domElement.StyleInfo.CurrentStyleSheet = styleSheet;
            domElement.StyleInfo.DoMatchCheck = SelectorType.LogicalTree | SelectorType.VisualTree;

            foreach(var child in domElement.ChildNodes)
            {
                SetCurrentStyleSheet(child, styleSheet);
            }
        }

        [Test]
        public void Select_on_root_node_descending_nodes_by_tag()
        {
            var styleSheet = CssParser.Parse(test1);
            SetCurrentStyleSheet(dom, styleSheet);

            var nodes = dom.QuerySelectorAllWithSelf(styleSheet, new Selector("body label"), SelectorType.LogicalTree);

            Assert.AreEqual(3, nodes.Count());
        }


        [Test]
        public void Select_on_root_node_with_repeating_general_parents2()
        {
            var styleSheet = CssParser.Parse(test1);
            SetCurrentStyleSheet(dom, styleSheet);

            var nodes = div.QuerySelectorAllWithSelf(styleSheet, new Selector("div div label.required"), SelectorType.LogicalTree);

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(label3, nodes.First());
        }


        [Test]
        public void Select_on_root_node_with_repeating_general_parents()
        {
            var styleSheet = CssParser.Parse(test1);
            SetCurrentStyleSheet(dom, styleSheet);

            var nodes = dom.QuerySelectorAllWithSelf(styleSheet, new Selector("div div label.required"), SelectorType.LogicalTree);

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(label3, nodes.First());
        }

        [Test]
        public void Select_on_root_node_descending_node_with_tag_and_class()
        {
            var styleSheet = CssParser.Parse(test1);
            SetCurrentStyleSheet(dom, styleSheet);

            var nodes = dom.QuerySelectorAllWithSelf(styleSheet, new Selector("body label.required"), SelectorType.LogicalTree);

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(label3, nodes.First());
        }

        [Test]
        public void Select_on_root_node_intermediate_node_and_descending_node_with_tag_and_class()
        {
            var styleSheet = CssParser.Parse(test1);
            SetCurrentStyleSheet(dom, styleSheet);

            var nodes = dom.QuerySelectorAllWithSelf(styleSheet, new Selector("body div label.required"), SelectorType.LogicalTree);

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(label3, nodes.First());
        }

        [Test]
        public void Select_on_sub_node_descending_node_with_tag_and_class()
        {
            var styleSheet = CssParser.Parse(test1);
            SetCurrentStyleSheet(dom, styleSheet);

            var nodes = div.QuerySelectorAllWithSelf(styleSheet, new Selector("div label.required"), SelectorType.LogicalTree);

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(label3, nodes.First());
        }

        [Test]
        public void Select_by_ascending_node_with_tag_and_class2()
        {
            var styleSheet = CssParser.Parse(test1);
            SetCurrentStyleSheet(dom, styleSheet);

            var nodes = div.QuerySelectorAllWithSelf(styleSheet, new Selector("label.required"), SelectorType.LogicalTree);

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(label3, nodes.First());
        }

        [Test]
        public void Select_ascending_node_with_tag_and_class3()
        {
            var styleSheet = CssParser.Parse(test1);
            SetCurrentStyleSheet(dom, styleSheet);

            var nodes = div.QuerySelectorAllWithSelf(styleSheet, new Selector("body label.required"), SelectorType.LogicalTree);

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(label3, nodes.First());
        }

        [Test]
        public void Select_ascending_node_with_tag_and_class4()
        {
            var nodes = label3.QuerySelectorAllWithSelf(defaultStyleSheet, new Selector("body label.required"), SelectorType.LogicalTree);

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(label3, nodes.First());
        }

        [Test]
        public void Select_ascending_node_with_nth_child()
        {
            var selector = new Selector("label:nth-of-type(2)");
            var nodes = label2.QuerySelectorAllWithSelf(defaultStyleSheet, selector, SelectorType.LogicalTree);

            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(label2, nodes.First());
        }

        [Test]
        public void Parser_loads_namespaces()
        {
            var styleSheet = CssParser.Parse(test1);

            Assert.AreEqual(4, styleSheet.Namespaces.Count());

            Assert.AreEqual("", styleSheet.Namespaces[0].Alias);
            Assert.AreEqual(TestNode.GetAssemblyQualifiedNamespaceName(typeof(TestNode)), styleSheet.Namespaces[0].Namespace);

            Assert.AreEqual("xamlcss", styleSheet.Namespaces[1].Alias);
            Assert.AreEqual(TestNode.GetAssemblyQualifiedNamespaceName(typeof(CssNamespace)), styleSheet.Namespaces[1].Namespace);

            Assert.AreEqual("ui", styleSheet.Namespaces[2].Alias);
            Assert.AreEqual(TestNode.GetAssemblyQualifiedNamespaceName(typeof(TestNode)), styleSheet.Namespaces[2].Namespace);

            Assert.AreEqual("special", styleSheet.Namespaces[3].Alias);
            Assert.AreEqual(TestNode.GetAssemblyQualifiedNamespaceName(typeof(TestNode)), styleSheet.Namespaces[3].Namespace);
        }

        [Test]
        public void Select_without_namespace()
        {
            var nodes = dom.QuerySelectorAllWithSelf(defaultStyleSheet, new Selector("Grid"), SelectorType.LogicalTree);

            Assert.AreEqual(2, nodes.Count());
            Assert.AreEqual(grid, nodes[0]);
            Assert.AreEqual(gridSpecial, nodes[1]);
        }

        [Test]
        public void Select_with_namespace()
        {
            var nodes = dom.QuerySelectorAllWithSelf(defaultStyleSheet, new Selector("special|Grid"), SelectorType.LogicalTree);

            Assert.AreEqual(2, nodes.Count());
            Assert.AreEqual(grid, nodes[0]);
            Assert.AreEqual(gridSpecial, nodes[1]);
        }
    }
}

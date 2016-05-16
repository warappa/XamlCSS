using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS.Tests.Dom
{
	[TestFixture]
	public class CssSelectionTests
	{
		private CssParser selectorEngine;
		string test1 = @"
@namespace xamlcss ""XamlCss"";
.main .sub>div xamlcss|Button {
	background-color: red;
	background: #00ff00;
}
";
		private TestNode dom;
		private TestNode body;
		private TestNode div;
		private TestNode label1;
		private TestNode label2;
		private TestNode label3;
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
					(label3 = new TestNode(null, "label", null,null, null, "required"))
				}))
				}))
			}));

			Assert.AreEqual("label", label2.TagName);
			Assert.AreEqual("label2", label2.Id);
			Assert.AreEqual("required", label3.ClassName);

			Assert.AreEqual(div, label2.Parent);
		}

		[Test]
		public void Test_select_on_root_node_descending_nodes_by_tag()
		{
			var styleSheet = CssParser.Parse(test1);
			
			var nodes = dom.QuerySelectorAllWithSelf("body label");

			Assert.AreEqual(3, nodes.Count());
		}

		[Test]
		public void Test_select_on_root_node_descending_node_with_tag_and_class()
		{
			var styleSheet = CssParser.Parse(test1);
			
			var nodes = dom.QuerySelectorAllWithSelf("body label.required");

			Assert.AreEqual(1, nodes.Count());
			Assert.AreEqual(label3, nodes.First());
		}

		[Test]
		public void Test_select_on_sub_node_descending_node_with_tag_and_class()
		{
			var styleSheet = CssParser.Parse(test1);
			
			var nodes = div.QuerySelectorAllWithSelf("div label.required");
			
			Assert.AreEqual(1, nodes.Count());
			Assert.AreEqual(label3, nodes.First());
		}

		[Test]
		public void Test_select_by_ascending_node_with_tag_and_class2()
		{
			var styleSheet = CssParser.Parse(test1);
			
			var nodes = div.QuerySelectorAllWithSelf("label.required");

			Assert.AreEqual(1, nodes.Count());
			Assert.AreEqual(label3, nodes.First());
		}

		[Test]
		public void Test_select_ascending_node_with_tag_and_class3()
		{
			var styleSheet = CssParser.Parse(test1);
			
			var nodes = div.QuerySelectorAllWithSelf("body label.required");

			Assert.AreEqual(1, nodes.Count());
			Assert.AreEqual(label3, nodes.First());
		}

		[Test]
		public void Test_select_ascending_node_with_tag_and_class4()
		{
			var styleSheet = CssParser.Parse(test1);
			
			var nodes = label3.QuerySelectorAllWithSelf("body label.required");

			Assert.AreEqual(1, nodes.Count());
			Assert.AreEqual(label3, nodes.First());
		}

		[Test]
		public void TestSelectAscendingNodeWithNth_child()
		{
			var styleSheet = CssParser.Parse(test1);

			var nodes = label2.QuerySelectorAllWithSelf("label:nth-of-type(2)");

			Assert.AreEqual(1, nodes.Count());
			Assert.AreEqual(label2, nodes.First());
		}
	}
}

using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using XamlCSS.Dom;

namespace XamlCSS.Tests.Dom
{
    [TestFixture]
    public class SelectorTests
    {
        public SelectorTests()
        {
            TestNamespaceProvider.Instance["ui"] = "XamlCSS.Dom.Tests";
        }
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
        public void Can_match_class_multiple()
        {
            var selector = new Selector(".important.some");

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

        [Test]
        public void Can_match_direct_sibling()
        {
            var selector = new Selector("a+#bbb");
        
            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("a", null);

            var parent = GetDomElement("parent", null, "", new[] { sibling, tag });
            
            selector.Match(tag).Should().Be(true);
        }

        [Test]
        public void Can_match_direct_sibling_more_levels()
        {
            var selector = new Selector("a+b+#bbb");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("a", null);
            var sibling2 = GetDomElement("b", null);

            var parent = GetDomElement("parent", null, "", new[] { sibling, sibling2, tag });

            selector.Match(tag).Should().Be(true);
        }

        [Test]
        public void Can_match_general_sibling()
        {
            var selector = new Selector("a~#bbb");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("a", null);
            var sibling2 = GetDomElement("b", null);

            var parent = GetDomElement("parent", null, "", new[] { sibling, sibling2, tag });

            selector.Match(tag).Should().Be(true);
        }

        [Test]
        public void Can_match_general_sibling_more_levels()
        {
            var selector = new Selector("c~d a~#bbb");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("a", null);
            var sibling2 = GetDomElement("b", null);

            var parent = GetDomElement("d", null, "", new[] { sibling, sibling2, tag });
            var parentSibling = GetDomElement("c", null, "", null);
            var parentSibling2 = GetDomElement("y", null, "", null);

            var parentRoot = GetDomElement("parent", null, "", new[] { parentSibling, parentSibling2, parent});

            selector.Match(tag).Should().Be(true);
        }

        [Test]
        public void Can_match_direct_descendant()
        {
            var selector = new Selector("a>#bbb");

            var tag = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { tag });

            selector.Match(tag).Should().Be(true);
        }

        [Test]
        public void Can_match_direct_descendant_more_levels()
        {
            var selector = new Selector("b>a>#bbb");

            var tag = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { tag });
            var parentRoot = GetDomElement("b", null, "", new[] { parent });

            selector.Match(tag).Should().Be(true);
        }

        [Test]
        public void Can_match_general_descendant()
        {
            var selector = new Selector("a #bbb");

            var tag = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { tag });
            var parentRoot = GetDomElement("a", null, "", new[] { parent });

            selector.Match(tag).Should().Be(true);
        }

        [Test]
        public void Can_match_general_descendant_more_levels()
        {
            var selector = new Selector("b a #bbb");

            var tag = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { tag });
            var parentRoot = GetDomElement("b", null, "", new[] { parent });

            selector.Match(tag).Should().Be(true);
        }

        [Test]
        public void Can_match_first_child()
        {
            var selector = new Selector("button:first-child");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { tag, sibling });

            selector.Match(tag).Should().Be(true);
        }

        [Test]
        public void Can_match_last_child()
        {
            var selector = new Selector("button:last-child");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, tag });

            selector.Match(tag).Should().Be(true);
        }

        [Test]
        public void Can_match_nth_child()
        {
            var selector = new Selector("button:nth-child(2)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, tag });

            selector.Match(tag).Should().Be(true);
        }

        [Test]
        public void Can_match_nth_of_type()
        {
            var selector = new Selector("button:nth-of-type(2)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, tag });

            selector.Match(tag).Should().Be(true);
        }

        [Test]
        public void Can_match_universal()
        {
            var selector = new Selector("*");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, tag });

            selector.Match(tag).Should().Be(true);
            selector.Match(parent).Should().Be(true);
            selector.Match(sibling).Should().Be(true);
            selector.Match(sibling2).Should().Be(true);
        }

        [Test]
        public void Can_match_universal_namespaced()
        {
            var selector = new Selector("ui|*");

            var tag = GetDomElement("ui|button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, tag });

            selector.Match(tag).Should().Be(true);
            selector.Match(parent).Should().Be(false);
            selector.Match(sibling).Should().Be(false);
            selector.Match(sibling2).Should().Be(false);
        }

        public TestNode GetDomElement(string tagname, string id, string classes = "", IEnumerable<IDomElement<UIElement>> children = null)
        {
            return new TestNode(GetUiElement(), tagname, children, null, id, classes);
        }

        public UIElement GetUiElement()
        {
            return new UIElement();
        }
    }
}

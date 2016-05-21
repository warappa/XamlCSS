using AngleSharp.Dom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS.Tests.Dom
{
	public class UIElement
	{
		public string Id { get; set; }
		public string Class { get; set; }
		public List<UIElement> Children { get; set; } = new List<UIElement>();
	}
	public class TestElementAttribute : ElementAttributeBase<UIElement, IDictionary<object, object>>
	{
		public TestElementAttribute(UIElement dependencyObject, IDictionary<object, object> property)
			: base(dependencyObject, property)
		{

		}

		public override string Value
		{
			get
			{
				object val = null;
				if (property.TryGetValue(dependencyObject, out val) == true)
					return (val ?? "").ToString();
				return null;
			}

			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public class TestElementCollection : ElementCollectionBase<UIElement>
	{
		public TestElementCollection(IDomElement<UIElement> node)
			: base(node)
		{

		}

		public TestElementCollection(IEnumerable<IElement> elements)
			: base(elements)
		{

		}

		protected override IElement CreateElement(UIElement dependencyObject, IDomElement<UIElement> parentNode)
		{
			throw new NotImplementedException();
		}
		protected override IEnumerable<UIElement> GetChildren(UIElement dependencyObject)
		{
			return dependencyObject.Children;
		}
		protected override string GetId(UIElement dependencyObject)
		{
			return dependencyObject.Id;
		}
	}

	public class TestNamedNodeList : NamedNodeListBase<UIElement, IDictionary<object, object>>
	{
		public TestNamedNodeList(DomElementBase<UIElement, IDictionary<object, object>> node)
			: base(node)
		{

		}

		public TestNamedNodeList(IEnumerable<INode> nodes)
			: base(nodes)
		{

		}

		protected override INode CreateNode(UIElement dependencyObject, IDomElement<UIElement> parentNode)
		{
			throw new NotImplementedException();
		}
		protected override IEnumerable<UIElement> GetChildren(UIElement dependencyObject)
		{
			return dependencyObject.Children;
		}
	}

	public class TestNamedNodeMap : NamedNodeMapBase<UIElement, IDictionary<object, object>>
	{
		public TestNamedNodeMap(UIElement dependencyObject)
			: base(dependencyObject)
		{

		}
		public TestNamedNodeMap(IDictionary<string, string> dependencyObject)
			: base(dependencyObject.Select(x => new TestElementAttribute(null, new Dictionary<object, object>() { { x.Key, x.Value } })))
		{

		}
		protected override IAttr CreateAttribute(UIElement dependencyObject, IDictionary<object, object> property)
		{
			return new TestElementAttribute(dependencyObject, property);
		}
	}

	[DebuggerDisplay("{TagName} #{Id} .{ClassName}")]
	public class TestNode : DomElementBase<UIElement, IDictionary<object, object>>
	{
		public TestNode(UIElement dependencyObject, string tagname, IEnumerable<IElement> children = null,
			IDictionary<string, string> attributes = null, string id = null, string @class = null)
			: base(dependencyObject ?? new UIElement(), null)
		{
			this.ChildNodes = CreateNodeList(children ?? new List<IElement>());
			foreach (TestNode c in ChildNodes)
				c.Parent = c.ParentElement = this;
			this.ClassList = new TokenList();
			this.ClassList.Add((@class ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			this.Id = id;
			this.LocalName = this.NodeName = this.TagName = tagname;
			this.prefix = "ui";
			this.Attributes = new TestNamedNodeMap(attributes ?? new Dictionary<string, string>());
		}
		protected override IHtmlCollection<IElement> CreateCollection(IEnumerable<IElement> list)
		{
			return new TestElementCollection(list);
		}
		protected override INamedNodeMap CreateNamedNodeMap(UIElement dependencyObject)
		{
			return new TestNamedNodeMap(dependencyObject);
		}

		protected override IHtmlCollection<IElement> GetChildElements(UIElement dependencyObject)
		{
			return new TestElementCollection(this);
		}
		protected override INodeList GetChildNodes(UIElement dependencyObject)
		{
			return new TestNamedNodeList(this);
		}
		protected override INodeList CreateNodeList(IEnumerable<INode> nodes)
		{
			return new TestNamedNodeList(nodes);
		}
		protected override ITokenList GetClassList(UIElement dependencyObject)
		{
			var list = new TokenList();
			var strs = (dependencyObject.Class ?? "").Split(' ');
			if (strs != null)
				list.AddRange(strs);
			return list;
		}
		protected override string GetId(UIElement dependencyObject)
		{
			return dependencyObject.Id;
		}
	}
}
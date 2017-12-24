using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            : base(node, null)
        {

        }

        public TestElementCollection(IEnumerable<IElement> elements)
            : base(elements, null)
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
            : base(node, null)
        {

        }

        public TestNamedNodeList(IEnumerable<INode> nodes)
            : base(nodes, null)
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
        protected override IAttr CreateAttribute(UIElement dependencyObject, DependencyPropertyInfo<IDictionary<object, object>> propertyInfo)
        {
            return new TestElementAttribute(dependencyObject, propertyInfo.Property);
        }
    }

    [DebuggerDisplay("{TagName} #{Id} .{ClassName}")]
    public class TestNode : DomElementBase<UIElement, IDictionary<object, object>>
    {
        public TestNode(UIElement dependencyObject, string tagname, IEnumerable<IDomElement<UIElement>> children = null,
            IDictionary<string, IDictionary<object, object>> attributes = null, string id = null, string @class = null)
            : base(dependencyObject ?? new UIElement(), null)
        {
            this.childNodes = children?.ToList() ?? new List<IDomElement<UIElement>>();
            foreach (TestNode c in ChildNodes)
            {
                c.parent = this;
            }

            this.classList = new List<string>();
            foreach (var item in (@class ?? "").Split(classSplitter, StringSplitOptions.RemoveEmptyEntries))
            {
                this.ClassList.Add(item);
            }

            this.Id = id;
            this.LocalName = this.NodeName = this.TagName = tagname;
            this.prefix = "ui";
            this.attributes = attributes ?? new Dictionary<string, IDictionary<object, object>>();
        }

        public string namespaceUri;
        public override string NamespaceUri
        {
            get
            {
                return namespaceUri;
            }

            protected set
            {
                namespaceUri = value;
            }
        }

        protected override IDictionary<string, IDictionary<object, object>> CreateNamedNodeMap(UIElement dependencyObject)
        {
            return (IDictionary<string, IDictionary<object, object>>)new Dictionary<string, Dictionary<object, object>>();
        }
        protected override IList<string> GetClassList(UIElement dependencyObject)
        {
            return dependencyObject.Class.Split(new[] { ' '}, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        IDomElement<UIElement> parent;
        public string ClassName => string.Join(" ", ClassList);

        public override IDomElement<UIElement> Parent
        {
            get
            {
                return parent;
            }
        }

        protected override string GetId(UIElement dependencyObject)
        {
            return dependencyObject.Id;
        }
    }
}
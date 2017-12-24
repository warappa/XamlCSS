using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XamlCSS.Dom;

namespace XamlCSS.Tests.Dom
{
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
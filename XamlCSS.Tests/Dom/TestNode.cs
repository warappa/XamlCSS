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
        public TestNode(UIElement dependencyObject, IDomElement<UIElement> parent, string tagname, IEnumerable<IDomElement<UIElement>> children = null,
            IDictionary<string, IDictionary<object, object>> attributes = null, string id = null, string @class = null)
            : base(dependencyObject ?? new UIElement(), parent, parent, TestTreeNodeProvider.Instance)
        {
            this.childNodes = this.logicalChildNodes = children?.ToList() ?? new List<IDomElement<UIElement>>();
            foreach (TestNode c in ChildNodes)
            {
                c.parent = this;
                c.logicalParent = this;
            }

            dependencyObject.Children = this.ChildNodes.Select(x => x.Element).ToList();

            this.classList = new List<string>();
            foreach (var item in (@class ?? "").Split(classSplitter, StringSplitOptions.RemoveEmptyEntries))
            {
                this.ClassList.Add(item);
            }

            this.Id = id;
            var namespaceSeparatorIndex = tagname.IndexOf('|');
            if (namespaceSeparatorIndex > -1)
            {
                this.TagName = tagname.Substring(namespaceSeparatorIndex + 1);
                this.assemblyQualifiedNamespaceName = TestNode.GetAssemblyQualifiedNamespaceName(GetType());
            }
            else
            {
                this.TagName = tagname;
                this.assemblyQualifiedNamespaceName = TestNode.GetAssemblyQualifiedNamespaceName(GetType());
            }

            this.attributes = attributes ?? new Dictionary<string, IDictionary<object, object>>();

            this.StyleInfo = new StyleUpdateInfo
            {
                MatchedType = dependencyObject.GetType()
            };
        }

        public override void UpdateIsReady()
        {
            IsReady = true;
        }

        protected override IList<IDomElement<UIElement>> GetChildNodes(SelectorType type)
        {
            return childNodes;
        }

        protected override IDictionary<string, IDictionary<object, object>> CreateNamedNodeMap(UIElement dependencyObject)
        {
            return (IDictionary<string, IDictionary<object, object>>)new Dictionary<string, Dictionary<object, object>>();
        }
        protected override IList<string> GetClassList(UIElement dependencyObject)
        {
            return dependencyObject.Class.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

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

        public override bool Equals(object obj)
        {
            var other = obj as TestNode;
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return ReferenceEquals(other.Element, Element);
        }
        public override int GetHashCode()
        {
            return Element.GetHashCode();
        }
    }
}
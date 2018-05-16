using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using XamlCSS.Dom;
using XamlCSS.Utils;

namespace XamlCSS.Tests.Dom
{
    [DebuggerDisplay("{TagName} #{Id} .{ClassName}")]
    public class TestNode : DomElementBase<UIElement, PropertyInfo>
    {
        public TestNode(UIElement dependencyObject, IDomElement<UIElement, PropertyInfo> parent, string tagname, IEnumerable<IDomElement<UIElement, PropertyInfo>> children = null,
            IDictionary<string, PropertyInfo> attributes = null, string id = null, string @class = null)
            : base(dependencyObject ?? new UIElement(), parent, parent, TestTreeNodeProvider.Instance)
        {
            this.childNodes = this.logicalChildNodes = children?.ToList() ?? new List<IDomElement<UIElement, PropertyInfo>>();
            foreach (TestNode c in ChildNodes)
            {
                c.parent = this;
                c.logicalParent = this;
            }

            dependencyObject.Children = this.ChildNodes.Select(x => x.Element).ToList();

            this.classList = new HashSet<string>();
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

            this.attributes = attributes ?? new Dictionary<string, PropertyInfo>();

            this.StyleInfo = new StyleUpdateInfo
            {
                MatchedType = dependencyObject.GetType()
            };
        }

        public override bool ApplyStyleImmediately { get; }

        public override void EnsureAttributeWatcher(PropertyInfo dependencyProperty)
        {

        }
        public override void ClearAttributeWatcher()
        {

        }

        public override object GetAttributeValue(PropertyInfo dependencyProperty)
        {
            return dependencyProperty.GetValue(dependencyObject);
        }

        public override void UpdateIsReady()
        {
            IsReady = true;
        }

        protected override IList<IDomElement<UIElement, PropertyInfo>> GetChildNodes(SelectorType type)
        {
            return childNodes;
        }

        protected override IDictionary<string, PropertyInfo> CreateNamedNodeMap(UIElement dependencyObject)
        {
            return new Dictionary<string, PropertyInfo>();
        }
        protected override HashSet<string> GetClassList(UIElement dependencyObject)
        {
            return dependencyObject.Class.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        }

        public string ClassName => string.Join(" ", ClassList);

        public override IDomElement<UIElement, PropertyInfo> Parent
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
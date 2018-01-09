using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XamlCSS.Dom;

namespace XamlCSS.Tests.Dom
{
    public class TestNamespaceProvider : INamespaceProvider<UIElement>
    {
        public static TestNamespaceProvider Instance = new TestNamespaceProvider(new Dictionary<string, string>());

        public readonly IDictionary<string, string> prefixToNamespaceUri;

        public TestNamespaceProvider(IDictionary<string, string> prefixToNamespaceUri)
        {
            this.prefixToNamespaceUri = prefixToNamespaceUri;
        }
        public string LookupNamespaceUri(IDomElement<UIElement> dependencyObject, string prefix)
        {
            if (prefixToNamespaceUri.ContainsKey(prefix))
            {
                return prefixToNamespaceUri[prefix];
            }

            return null;
        }

        public string LookupPrefix(IDomElement<UIElement> dependencyObject, string namespaceUri)
        {
            if (prefixToNamespaceUri.Any(x => x.Value == namespaceUri))
            {
                return prefixToNamespaceUri.FirstOrDefault(x => x.Value == namespaceUri).Key;
            }

            return null;
        }

        public void Clear()
        {
            prefixToNamespaceUri.Clear();
        }

        public string this[string key]
        {
            get => prefixToNamespaceUri[key];
            set => prefixToNamespaceUri[key] = value;
        }
    }
    [DebuggerDisplay("{TagName} #{Id} .{ClassName}")]
    public class TestNode : DomElementBase<UIElement, IDictionary<object, object>>
    {
        public TestNode(UIElement dependencyObject, IDomElement<UIElement> parent, string tagname, IEnumerable<IDomElement<UIElement>> children = null,
            IDictionary<string, IDictionary<object, object>> attributes = null, string id = null, string @class = null, INamespaceProvider<UIElement> namespaceProvider = null)
            : base(dependencyObject ?? new UIElement(), parent, null, namespaceProvider ?? TestNamespaceProvider.Instance)
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

            var nsp = namespaceProvider ?? TestNamespaceProvider.Instance;

            this.Id = id;
            var namespaceSeparatorIndex = tagname.IndexOf('|');
            if (namespaceSeparatorIndex > -1)
            {
                this.LocalName = tagname.Substring(namespaceSeparatorIndex + 1);
                this.NodeName = this.TagName = tagname;
                this.prefix = tagname.Substring(0, namespaceSeparatorIndex);
                this.namespaceUri = nsp.LookupNamespaceUri(this, prefix);
            }
            else
            {
                this.LocalName = this.NodeName = this.TagName = tagname;
                this.prefix = "";
                this.namespaceUri = nsp.LookupNamespaceUri(this, prefix);
            }

            this.attributes = attributes ?? new Dictionary<string, IDictionary<object, object>>();

            this.StyleInfo = new StyleUpdateInfo
            {
                MatchedType = dependencyObject.GetType()
            };
        }

        protected override IDictionary<string, IDictionary<object, object>> CreateNamedNodeMap(UIElement dependencyObject)
        {
            return (IDictionary<string, IDictionary<object, object>>)new Dictionary<string, Dictionary<object, object>>();
        }
        protected override IList<string> GetClassList(UIElement dependencyObject)
        {
            return dependencyObject.Class.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
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
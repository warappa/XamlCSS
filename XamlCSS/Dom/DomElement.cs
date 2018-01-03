using System;
using System.Collections.Generic;
using System.Linq;

namespace XamlCSS.Dom
{
    public interface INamespaceProvider<TDependencyObject>
        where TDependencyObject : class
    {
        string LookupNamespaceUri(IDomElement<TDependencyObject> domElement, string prefix);
        string LookupPrefix(IDomElement<TDependencyObject> domElement, string namespaceUri);
    }
    public abstract class DomElementBase<TDependencyObject, TDependencyProperty> : IDomElement<TDependencyObject>
        where TDependencyObject : class
        where TDependencyProperty : class
    {
        private const string Undefined = "UNDEFINED";

        private static readonly CachedSelectorProvider cachedSelectorProvider = new CachedSelectorProvider();

        public IList<StyleSheet> XamlCssStyleSheets { get; protected set; } = new List<StyleSheet>();

        protected ITreeNodeProvider<TDependencyObject> treeNodeProvider;
        private readonly INamespaceProvider<TDependencyObject> namespaceProvider;
        protected readonly static char[] classSplitter = { ' ' };
        protected readonly TDependencyObject dependencyObject;
        protected string id;
        protected string namespaceUri = Undefined;
        protected IList<IDomElement<TDependencyObject>> childNodes = null;
        protected string prefix = Undefined;
        protected IDictionary<string, TDependencyProperty> attributes = null;
        protected IList<string> classList = null;

        public DomElementBase(
            TDependencyObject dependencyObject,
            IDomElement<TDependencyObject> parent,
            ITreeNodeProvider<TDependencyObject> treeNodeProvider,
            INamespaceProvider<TDependencyObject> namespaceProvider
            )
        {
            this.dependencyObject = dependencyObject;
            this.parent = parent;
            this.treeNodeProvider = treeNodeProvider;
            this.namespaceProvider = namespaceProvider;

            this.id = GetId(dependencyObject);
            this.NodeName = dependencyObject.GetType().Name;
            this.LocalName = dependencyObject.GetType().Name;
            this.namespaceUri = GetNamespaceUri(dependencyObject.GetType());
        }

        protected void ElementAdded(object sender, EventArgs e)
        {
            if (sender == Element)
            {
                // force reevaluation
                parent = treeNodeProvider.GetDomElement(treeNodeProvider.GetParent(Element));
            }

            if (treeNodeProvider.GetParent(sender as TDependencyObject) == dependencyObject)
            {
                if (childNodes != null)
                {
                    var node = treeNodeProvider.GetDomElement(sender as TDependencyObject);
                    if (childNodes.Any(x => x.Element == sender))
                    {

                    }
                    else
                    {
                        childNodes.Add(node);
                    }
                }
            }
        }

        protected void ElementRemoved(object sender, EventArgs e)
        {
            if (ChildNodes.Any(x => x.Element == sender))
            {
                if (childNodes != null)
                {
                    var node = childNodes.First(x => x.Element == sender as TDependencyObject);
                    childNodes.Remove(node);
                }
            }

            if (sender == Element)
            {
                parent = null;
            }
        }

        protected void ResetChildren()
        {
            this.childNodes = null;
        }

        public void ResetClassList()
        {
            classList = null;
        }

        public static string GetNamespaceUri(Type type)
        {
            return type.AssemblyQualifiedName.Replace($".{type.Name},", ",");
        }

        public TDependencyObject Element { get { return dependencyObject; } }

        abstract protected IDictionary<string, TDependencyProperty> CreateNamedNodeMap(TDependencyObject dependencyObject);

        // abstract protected INodeList CreateNodeList(IEnumerable<INode> nodes);

        protected IList<IDomElement<TDependencyObject>> GetChildNodes(TDependencyObject dependencyObject)
        {
            return treeNodeProvider
                .GetChildren(dependencyObject)
                .Select(x => treeNodeProvider.GetDomElement(x))
                .ToList();
        }

        abstract protected IList<string> GetClassList(TDependencyObject dependencyObject);

        abstract protected string GetId(TDependencyObject dependencyObject);

        public IDictionary<string, TDependencyProperty> Attributes => attributes ?? (attributes = CreateNamedNodeMap(dependencyObject));

        public IList<IDomElement<TDependencyObject>> ChildNodes => childNodes ?? (childNodes = GetChildNodes(dependencyObject));

        public IList<string> ClassList => classList ?? (classList = GetClassList(dependencyObject));

        public bool HasChildNodes { get { return ChildNodes.Any(); } }

        public string Id { get { return id; } set { id = value; } }

        public bool IsFocused { get { return false; } }

        public string LocalName { get; protected set; }

        public string NodeName { get; protected set; }

        public IDomElement<TDependencyObject> Owner
        {
            get
            {
                IDomElement<TDependencyObject> currentNode = this;
                while (true)
                {
                    if (currentNode.Parent == null)
                        break;

                    currentNode = currentNode.Parent;
                }

                return currentNode;
            }
        }

        IDomElement<TDependencyObject> parent = null;
        public virtual IDomElement<TDependencyObject> Parent
        {
            get
            {
                if (parent != null)
                {
                    return parent;
                }
                return null;
                //var parentElement = treeNodeProvider.GetParent(dependencyObject);
                //return parentElement == null ? null : (parent = treeNodeProvider.GetDomElement(parentElement));
            }
        }

        public string Prefix
        {
            get
            {
                if (prefix == Undefined)
                {
                    prefix = namespaceProvider.LookupPrefix(this, NamespaceUri) ?? Undefined;
                }

                return prefix != Undefined ? prefix : null;
            }
        }

        public string NamespaceUri
        {
            get
            {
                return namespaceUri;
            }
        }

        private string tagName = null;
        public string TagName
        {
            get
            {
                if (tagName != null)
                {
                    return tagName;
                }
                tagName = LookupPrefix(dependencyObject.GetType().Namespace) + "|" + dependencyObject.GetType().Name;
                return tagName;
            }
            protected set
            {
                tagName = value;
            }
        }

        public StyleUpdateInfo StyleInfo { get; set; }

        public bool Contains(IDomElement<TDependencyObject> otherNode)
        {
            return ChildNodes.Contains(otherNode);
        }

        public bool Equals(IDomElement<TDependencyObject> otherNode)
        {
            if (otherNode == null)
            {
                return false;
            }

            if (object.ReferenceEquals(this, otherNode))
            {
                return true;
            }

            return dependencyObject == otherNode.Element;
        }

        public bool HasAttribute(string name)
        {
            return Attributes.ContainsKey(name);
        }

        public bool IsDefaultNamespace(string namespaceUri)
        {
            return LookupPrefix(namespaceUri) == "";
        }

        public string LookupNamespaceUri(string prefix)
        {
            return namespaceProvider.LookupNamespaceUri(this, prefix);
        }

        public string LookupPrefix(string namespaceUri)
        {
            return namespaceProvider.LookupPrefix(this, namespaceUri);
        }

        public bool Matches(StyleSheet styleSheet, ISelector selector)
        {
            return selector.Match(styleSheet, this);
        }

        public IDomElement<TDependencyObject> QuerySelector(StyleSheet styleSheet, ISelector selector)
        {
            // var selector = cachedSelectorProvider.GetOrAdd(selectors);

            return ChildNodes.QuerySelector(styleSheet, selector);
        }

        public IDomElement<TDependencyObject> QuerySelectorWithSelf(StyleSheet styleSheet, ISelector selector)
        {
            if (this.Matches(styleSheet, selector))
            {
                return this;
            }

            //var selector = cachedSelectorProvider.GetOrAdd(selectors);

            return ChildNodes.QuerySelector(styleSheet, selector);
        }

        public IList<IDomElement<TDependencyObject>> QuerySelectorAll(StyleSheet styleSheet, ISelector selector)
        {
            // var selector = cachedSelectorProvider.GetOrAdd(selectors);

            return ChildNodes.QuerySelectorAll(styleSheet, selector);
        }

        public IList<IDomElement<TDependencyObject>> QuerySelectorAllWithSelf(StyleSheet styleSheet, ISelector selector)
        {
            // var selector = cachedSelectorProvider.GetOrAdd(selectors);

            if (StyleInfo.CurrentStyleSheet != styleSheet)
            {
                return new List<IDomElement<TDependencyObject>>();
            }

            var res = ChildNodes.QuerySelectorAll(styleSheet, selector);
            if (this.Matches(styleSheet, selector))
            {
                res.Add(this);
            }

            return res;
        }

        public bool HasFocus()
        {
            return false;
        }


        public override int GetHashCode()
        {
            return dependencyObject.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as DomElementBase<TDependencyObject, TDependencyProperty>;
            if (other == null)
                return false;
            return this.dependencyObject == other.dependencyObject;
        }

        public static bool operator ==(DomElementBase<TDependencyObject, TDependencyProperty> a, DomElementBase<TDependencyObject, TDependencyProperty> b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            return a?.Equals(b) == true;
        }
        public static bool operator !=(DomElementBase<TDependencyObject, TDependencyProperty> a, DomElementBase<TDependencyObject, TDependencyProperty> b)
        {
            return !(a == b);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DomElementBase() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public class NamespaceProvider<TDependencyObject, TUIElement, TStyle, TDependencyProperty> : INamespaceProvider<TDependencyObject>
        where TDependencyObject : class
        where TUIElement : class, TDependencyObject
        where TStyle : class
        where TDependencyProperty : class
    {
        private readonly IDependencyPropertyService<TDependencyObject, TUIElement, TStyle, TDependencyProperty> dependencyPropertyService;
        private readonly IDictionary<string, string> prefix2Namespace = new Dictionary<string, string>();

        public NamespaceProvider(IDependencyPropertyService<TDependencyObject, TUIElement, TStyle, TDependencyProperty> dependencyPropertyService)
        {
            this.dependencyPropertyService = dependencyPropertyService;
        }

        public string LookupNamespaceUri(IDomElement<TDependencyObject> domElement, string prefix)
        {
            if (prefix == null)
            {
                return null;
            }

            if (prefix2Namespace.ContainsKey(prefix))
            {
                return prefix2Namespace[prefix];
            }

            var namespaceUri = GetStyleSheet(domElement)?.Namespaces
                .Where(x => x.Alias == prefix)
                .Select(x => x.Namespace)
                .FirstOrDefault();

            if (namespaceUri != null)
            {
                prefix2Namespace[prefix] = namespaceUri;
            }

            return namespaceUri;
        }

        public string LookupPrefix(IDomElement<TDependencyObject> domElement, string namespaceUri)
        {
            if (namespaceUri == null)
            {
                return null;
            }

            var prefix = prefix2Namespace.Where(x => x.Value == namespaceUri)
                .Select(x => x.Key)
                .FirstOrDefault();

            if (prefix != null)
            {
                return prefix;
            }

            prefix = GetStyleSheet(domElement)?.Namespaces
                .Where(x => x.Namespace == namespaceUri)
                .Select(x => x.Alias)
                .FirstOrDefault();

            if (prefix != null)
            {
                prefix2Namespace[prefix] = namespaceUri;
            }

            return prefix;
        }

        private StyleSheet GetStyleSheet(IDomElement<TDependencyObject> domElement)
        {
            var element = GetStyleSheetHolder(domElement)?.Element;
            if (element == null)
            {
                return null;
            }

            return dependencyPropertyService.GetStyleSheet(element);
        }

        private IDomElement<TDependencyObject> GetStyleSheetHolder(IDomElement<TDependencyObject> domElement)
        {
            var current = domElement;
            while (current != null &&
                dependencyPropertyService.GetStyleSheet(current.Element) == null)
            {
                current = current.Parent;
            }

            return current;
        }
    }
}

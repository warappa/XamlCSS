using System;
using System.Collections.Generic;
using System.Linq;

namespace XamlCSS.Dom
{
    public abstract class DomElementBase<TDependencyObject, TDependencyProperty> : IDomElement<TDependencyObject>
        where TDependencyObject : class
        where TDependencyProperty : class
    {
        private const string Undefined = "UNDEFINED";

        private static readonly CachedSelectorProvider cachedSelectorProvider = new CachedSelectorProvider();

        public IList<StyleSheet> XamlCssStyleSheets { get; protected set; } = new List<StyleSheet>();

        protected ITreeNodeProvider<TDependencyObject> treeNodeProvider;
        protected readonly static char[] classSplitter = { ' ' };
        protected readonly TDependencyObject dependencyObject;
        protected string id;
        protected IList<IDomElement<TDependencyObject>> childNodes = null;
        protected string prefix = "UNDEFINED";
        protected IDictionary<string, TDependencyProperty> attributes = null;
        protected IList<string> classList = null;
        
        public DomElementBase(
            TDependencyObject dependencyObject,
            ITreeNodeProvider<TDependencyObject> treeNodeProvider
            )
        {
            this.treeNodeProvider = treeNodeProvider;
            this.dependencyObject = dependencyObject;
            this.id = GetId(dependencyObject);
            this.LocalName = dependencyObject.GetType().Name;
            this.NamespaceUri = dependencyObject.GetType().Namespace;
            this.NodeName = dependencyObject.GetType().Name;
            this.TagName = dependencyObject.GetType().Name;
        }


        protected void DomElementAdded(object sender, EventArgs e)
        {
            if (treeNodeProvider.GetParent(sender as TDependencyObject) == dependencyObject)
            {
                if (childNodes != null)
                {
                    var node = treeNodeProvider.GetDomElement(sender as TDependencyObject);
                    childNodes.Add(node);
                }
            }
        }

        protected void DomElementRemoved(object sender, EventArgs e)
        {
            if (ChildNodes.Any(x => x.Element == sender))
            {
                if (childNodes != null)
                {
                    var node = childNodes.First(x => x.Element == sender as TDependencyObject);
                    childNodes.Remove(node);
                }
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

        public static string GetPrefix(Type type)
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

        public virtual string NamespaceUri { get; protected set; }

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
        
        public virtual IDomElement<TDependencyObject> Parent
        {
            get
            {
                var parent = treeNodeProvider.GetParent(dependencyObject);
                return parent == null ? null : treeNodeProvider.GetDomElement(parent);
            }
        }

        public string Prefix
        {
            get
            {
                if (prefix == Undefined)
                {
                    prefix = Owner.LookupPrefix(GetPrefix(dependencyObject.GetType()));
                }

                return prefix;
            }
        }
        
        public string TagName { get; protected set; }
        
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
            return XamlCssStyleSheets
                .SelectMany(x => x.Namespaces)
                .Where(x => x.Alias == prefix)
                .Select(x => x.Namespace)
                .FirstOrDefault();
        }

        public string LookupPrefix(string namespaceUri)
        {
            return XamlCssStyleSheets
                .SelectMany(x => x.Namespaces)
                .Where(x => x.Namespace == namespaceUri)
                .Select(x => x.Alias)
                .FirstOrDefault();
        }

        public bool Matches(string selectors)
        {
            return cachedSelectorProvider.GetOrAdd(selectors).Match(this);
        }

        public IDomElement<TDependencyObject> QuerySelector(string selectors)
        {
            var selector = cachedSelectorProvider.GetOrAdd(selectors);

            return ChildNodes.QuerySelector(selector);
        }

        public IDomElement<TDependencyObject> QuerySelectorWithSelf(string selectors)
        {
            if (this.Matches(selectors))
            {
                return this;
            }

            var selector = cachedSelectorProvider.GetOrAdd(selectors);

            return ChildNodes.QuerySelector(selector);
        }

        public IList<IDomElement<TDependencyObject>> QuerySelectorAll(string selectors)
        {
            var selector = cachedSelectorProvider.GetOrAdd(selectors);

            return ChildNodes.QuerySelectorAll(selector);
        }

        public IList<IDomElement<TDependencyObject>> QuerySelectorAllWithSelf(string selectors)
        {
            var selector = cachedSelectorProvider.GetOrAdd(selectors);

            var res = ChildNodes.QuerySelectorAll(selector);
            if (this.Matches(selectors))
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
}

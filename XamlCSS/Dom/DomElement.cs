using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XamlCSS.Utils;

namespace XamlCSS.Dom
{
    [DebuggerDisplay("{dependencyObject.GetType().Name} Id={Id} Class={string.Join(\", \", ClassList)}")]
    public abstract class DomElementBase<TDependencyObject, TDependencyProperty> : IDomElement<TDependencyObject, TDependencyProperty>
        where TDependencyObject : class
        where TDependencyProperty : class
    {
        private static readonly CachedSelectorProvider cachedSelectorProvider = new CachedSelectorProvider();
        private static readonly IDictionary<Type, string> cachedAssemblyQualifiedNamespaces = new Dictionary<Type, string>();

        public IList<StyleSheet> XamlCssStyleSheets { get; protected set; } = new List<StyleSheet>();

        public bool? IsInLogicalTree { get; private set; }
        public bool? IsInVisualTree { get; private set; }

        protected readonly static char[] classSplitter = { ' ' };
        protected readonly TDependencyObject dependencyObject;
        protected ITreeNodeProvider<TDependencyObject, TDependencyProperty> treeNodeProvider;

        protected string id;
        protected string assemblyQualifiedNamespaceName;
        protected IList<IDomElement<TDependencyObject, TDependencyProperty>> childNodes = null;
        protected IList<IDomElement<TDependencyObject, TDependencyProperty>> logicalChildNodes = null;
        protected IDictionary<string, TDependencyProperty> attributes = null;
        protected HashSet<string> classList = null;

        public DomElementBase(
            TDependencyObject dependencyObject,
            IDomElement<TDependencyObject, TDependencyProperty> parent,
            IDomElement<TDependencyObject, TDependencyProperty> logicalParent,
            ITreeNodeProvider<TDependencyObject, TDependencyProperty> treeNodeProvider
            )
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            this.dependencyObject = dependencyObject;
            this.parent = parent;
            this.logicalParent = logicalParent;
            this.treeNodeProvider = treeNodeProvider;

            this.id = GetId(dependencyObject);
            this.TagName = dependencyObject.GetType().Name;
            this.classList = GetClassList(dependencyObject);
            this.assemblyQualifiedNamespaceName = GetAssemblyQualifiedNamespaceName(dependencyObject.GetType());

            UpdateTreeAssociation();
            UpdateIsReady();

            AddIfNotAdded();
        }

        protected void InvalidateIsInTree()
        {
            IsInLogicalTree = IsInVisualTree = null;
        }

        protected void ElementLoaded(object element)
        {
            if (!ReferenceEquals(element, dependencyObject))
            {
                return;
            }

            // got parent
            ReevaluateParent();

            AddIfNotAdded();
        }

        protected void ElementUnloaded(TDependencyObject element)
        {
            if (!ReferenceEquals(element, dependencyObject))
            {
                return;
            }

            if (IsInLogicalTree == true &&
                logicalParent != null)
            {
                if (((DomElementBase<TDependencyObject, TDependencyProperty>)logicalParent)?.logicalChildNodes?.Remove(this) == false)
                {
                    // should not happen - bug
                }
            }

            if (IsInVisualTree == true &&
                parent != null)
            {
                if (((DomElementBase<TDependencyObject, TDependencyProperty>)parent)?.childNodes?.Remove(this) == false)
                {
                    // should not happen - bug
                }
            }

            // got parent
            ReevaluateParent();
        }

        private void AddIfNotAdded()
        {
            if (IsInLogicalTree == true &&
                logicalParent != null)
            {
                if ((logicalParent as DomElementBase<TDependencyObject, TDependencyProperty>).logicalChildNodes?.Contains(this) == false)
                    (logicalParent as DomElementBase<TDependencyObject, TDependencyProperty>).logicalChildNodes?.Add(this);
            }

            if (IsInVisualTree == true &&
                parent != null)
            {
                if ((parent as DomElementBase<TDependencyObject, TDependencyProperty>).childNodes?.Contains(this) == false)
                    (parent as DomElementBase<TDependencyObject, TDependencyProperty>).childNodes?.Add(this);
            }
        }

        private void UpdateTreeAssociation()
        {
            this.IsInLogicalTree = treeNodeProvider.IsInTree(dependencyObject, SelectorType.LogicalTree);
            this.IsInVisualTree = treeNodeProvider.IsInTree(dependencyObject, SelectorType.VisualTree);
        }

        protected void ReevaluateParent()
        {
            // force reevaluation
            parent = treeNodeProvider.GetDomElement(treeNodeProvider.GetParent(Element, SelectorType.VisualTree));
            logicalParent = treeNodeProvider.GetDomElement(treeNodeProvider.GetParent(Element, SelectorType.LogicalTree));

            UpdateTreeAssociation();
        }

        public void ResetClassList()
        {
            classList = null;
        }

        public TDependencyObject Element { get { return dependencyObject; } }

        abstract public void EnsureAttributeWatcher(TDependencyProperty dependencyProperty);
        abstract public void ClearAttributeWatcher();
        abstract protected IDictionary<string, TDependencyProperty> CreateNamedNodeMap(TDependencyObject dependencyObject);
        abstract public bool ApplyStyleImmediately { get; }

        protected virtual IList<IDomElement<TDependencyObject, TDependencyProperty>> GetChildNodes(SelectorType type)
        {
            return treeNodeProvider
                .GetChildren(dependencyObject, type)
                .Select(x => treeNodeProvider.GetDomElement(x))
                .ToList();
        }

        abstract protected HashSet<string> GetClassList(TDependencyObject dependencyObject);

        abstract protected string GetId(TDependencyObject dependencyObject);

        abstract public void UpdateIsReady();

        public IDictionary<string, TDependencyProperty> Attributes => attributes ?? (attributes = CreateNamedNodeMap(dependencyObject));
        public abstract object GetAttributeValue(TDependencyProperty dependencyProperty);

        public IList<IDomElement<TDependencyObject, TDependencyProperty>> ChildNodes => childNodes ?? (childNodes = GetChildNodes(SelectorType.VisualTree));
        public IList<IDomElement<TDependencyObject, TDependencyProperty>> LogicalChildNodes => logicalChildNodes ?? (logicalChildNodes = GetChildNodes(SelectorType.LogicalTree));

        public HashSet<string> ClassList => classList ?? (classList = GetClassList(dependencyObject));

        public bool HasChildNodes { get { return ChildNodes.Any(); } }
        public bool HasLogicalChildNodes { get { return LogicalChildNodes.Any(); } }

        public string Id { get { return id; } set { id = value; } }

        public bool IsFocused { get { return false; } }

        public string TagName { get; protected set; }
        public bool IsReady { get; protected set; }

        public IDomElement<TDependencyObject, TDependencyProperty> Owner
        {
            get
            {
                IDomElement<TDependencyObject, TDependencyProperty> currentNode = this;
                while (true)
                {
                    if (currentNode.Parent == null)
                    {
                        break;
                    }

                    currentNode = currentNode.Parent;
                }

                return currentNode;
            }
        }

        protected IDomElement<TDependencyObject, TDependencyProperty> parent = null;
        protected IDomElement<TDependencyObject, TDependencyProperty> logicalParent;

        public virtual IDomElement<TDependencyObject, TDependencyProperty> Parent
        {
            get
            {
                if (parent != null)
                {
                    return parent;
                }
                return null;
            }
        }

        public virtual IDomElement<TDependencyObject, TDependencyProperty> LogicalParent
        {
            get
            {
                if (logicalParent != null)
                {
                    return logicalParent;
                }
                return null;
            }
        }

        public string AssemblyQualifiedNamespaceName
        {
            get
            {
                return assemblyQualifiedNamespaceName;
            }
        }

        public StyleUpdateInfo StyleInfo { get; set; }

        public bool Contains(IDomElement<TDependencyObject, TDependencyProperty> otherNode, SelectorType type)
        {
            return type == SelectorType.VisualTree ? ChildNodes.Contains(otherNode) : LogicalChildNodes.Contains(otherNode);
        }

        public bool Equals(IDomElement<TDependencyObject, TDependencyProperty> otherNode)
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

        public MatchResult Matches(StyleSheet styleSheet, ISelector selector)
        {
            return selector.Match(styleSheet, this, -1, 0);
        }

        public IList<IDomElement<TDependencyObject, TDependencyProperty>> QuerySelectorAll(StyleSheet styleSheet, ISelector selector, SelectorType type)
        {
            var children = (type == SelectorType.LogicalTree ? LogicalChildNodes : ChildNodes);

            return children.QuerySelectorAll(styleSheet, selector, type, 0);
        }

        public IList<IDomElement<TDependencyObject, TDependencyProperty>> QuerySelectorAllWithSelf(StyleSheet styleSheet, ISelector selector, SelectorType type)
        {
            if (!IsReady ||
                !ReferenceEquals(StyleInfo.CurrentStyleSheet, styleSheet) ||
                StyleInfo.DoMatchCheck == SelectorType.None)
            {
                return new List<IDomElement<TDependencyObject, TDependencyProperty>>();
            }

            if (type == SelectorType.LogicalTree &&
                IsInLogicalTree != true)
            {
                return new List<IDomElement<TDependencyObject, TDependencyProperty>>();
            }
            else if (type == SelectorType.VisualTree &&
                IsInVisualTree != true)
            {
                return new List<IDomElement<TDependencyObject, TDependencyProperty>>();
            }

            var endGroupIndex = GetEndGroupIndex(styleSheet, selector, type);

            var res = (type == SelectorType.LogicalTree ? LogicalChildNodes : ChildNodes).QuerySelectorAll(styleSheet, selector, type, endGroupIndex);

            var match = Matches(styleSheet, selector);
            if (match.IsSuccess)
            {
                res.Add(this);
            }

            return res;
        }

        private int GetEndGroupIndex(StyleSheet styleSheet, ISelector selector, SelectorType type)
        {
            if (selector.GroupCount == 1)
            {
                return 0;
            }

            var testStartGroupIndex = selector.GroupCount - 2;
            var testEndGroupIndex = 0;

            while (testEndGroupIndex >= 0)
            {
                var match = selector.Match(styleSheet, this, testStartGroupIndex, testEndGroupIndex);
                if (!match.IsSuccess)
                {
                    testEndGroupIndex--;
                }
                else
                {
                    return testEndGroupIndex;
                }
            }

            return 0;
        }

        public bool HasFocus()
        {
            return false;
        }

        public static string GetAssemblyQualifiedNamespaceName(Type type)
        {
            if (!cachedAssemblyQualifiedNamespaces.TryGetValue(type, out string val))
            {
                cachedAssemblyQualifiedNamespaces[type] = val = type.AssemblyQualifiedName.Replace($".{type.Name},", ",");
            }
            return val;
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
            ClearAttributeWatcher();

            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}

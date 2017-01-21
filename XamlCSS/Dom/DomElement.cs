using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Css;
using AngleSharp.Dom.Events;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Css;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XamlCSS.Dom
{
    public abstract class DomElementBase<TDependencyObject, TDependencyProperty> : IDomElement<TDependencyObject>, IDocument
        where TDependencyObject : class
        where TDependencyProperty : class
    {
        #region eventhandlers

        public event DomEventHandler ReadyStateChanged;
        public event DomEventHandler Aborted;
        public event DomEventHandler Blurred;
        public event DomEventHandler Cancelled;
        public event DomEventHandler CanPlay;
        public event DomEventHandler CanPlayThrough;
        public event DomEventHandler Changed;
        public event DomEventHandler Clicked;
        public event DomEventHandler CueChanged;
        public event DomEventHandler DoubleClick;
        public event DomEventHandler Drag;
        public event DomEventHandler DragEnd;
        public event DomEventHandler DragEnter;
        public event DomEventHandler DragExit;
        public event DomEventHandler DragLeave;
        public event DomEventHandler DragOver;
        public event DomEventHandler DragStart;
        public event DomEventHandler Dropped;
        public event DomEventHandler DurationChanged;
        public event DomEventHandler Emptied;
        public event DomEventHandler Ended;
        public event DomEventHandler Error;
        public event DomEventHandler Focused;
        public event DomEventHandler Input;
        public event DomEventHandler Invalid;
        public event DomEventHandler KeyDown;
        public event DomEventHandler KeyPress;
        public event DomEventHandler KeyUp;
        public event DomEventHandler Loaded;
        public event DomEventHandler LoadedData;
        public event DomEventHandler LoadedMetadata;
        public event DomEventHandler Loading;
        public event DomEventHandler MouseDown;
        public event DomEventHandler MouseEnter;
        public event DomEventHandler MouseLeave;
        public event DomEventHandler MouseMove;
        public event DomEventHandler MouseOut;
        public event DomEventHandler MouseOver;
        public event DomEventHandler MouseUp;
        public event DomEventHandler MouseWheel;
        public event DomEventHandler Paused;
        public event DomEventHandler Played;
        public event DomEventHandler Playing;
        public event DomEventHandler Progress;
        public event DomEventHandler RateChanged;
        public event DomEventHandler Resetted;
        public event DomEventHandler Resized;
        public event DomEventHandler Scrolled;
        public event DomEventHandler Seeked;
        public event DomEventHandler Seeking;
        public event DomEventHandler Selected;
        public event DomEventHandler Shown;
        public event DomEventHandler Stalled;
        public event DomEventHandler Submitted;
        public event DomEventHandler Suspended;
        public event DomEventHandler TimeUpdated;
        public event DomEventHandler Toggled;
        public event DomEventHandler VolumeChanged;
        public event DomEventHandler Waiting;

        private const string Undefined = "UNDEFINED";

        #endregion

        private static readonly CachedSelectorProvider cachedSelectorProvider = new CachedSelectorProvider();

        public List<StyleSheet> XamlCssStyleSheets { get; protected set; } = new List<StyleSheet>();

        protected ITreeNodeProvider<TDependencyObject> treeNodeProvider;
        protected readonly static char[] classSplitter = { ' ' };
        protected readonly TDependencyObject dependencyObject;
        protected string id;
        protected INodeList childNodes = null;
        protected IHtmlCollection<IElement> children = null;
        protected string prefix = "UNDEFINED";
        protected INamedNodeMap attributes = null;
        protected ITokenList classList = null;

        protected static readonly CssParser parser = new CssParser(new CssParserOptions
        {
            IsIncludingUnknownDeclarations = true,
            IsStoringTrivia = false,
            IsIncludingUnknownRules = true,
            IsToleratingInvalidConstraints = true,
            IsToleratingInvalidSelectors = true,
            IsToleratingInvalidValues = true
        });

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
            this.NodeType = NodeType.Element;
            this.TagName = dependencyObject.GetType().Name;
        }

        protected void ResetChildren()
        {
            this.childNodes = null;
            this.children = null;
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

        abstract protected INamedNodeMap CreateNamedNodeMap(TDependencyObject dependencyObject);

        abstract protected INodeList CreateNodeList(IEnumerable<INode> nodes);

        abstract protected INodeList GetChildNodes(TDependencyObject dependencyObject);

        abstract protected IHtmlCollection<IElement> CreateCollection(IEnumerable<IElement> list);

        abstract protected IHtmlCollection<IElement> GetChildElements(TDependencyObject dependencyObject);

        abstract protected ITokenList GetClassList(TDependencyObject dependencyObject);

        abstract protected string GetId(TDependencyObject dependencyObject);

        public IElement AssignedSlot { get { return null; } }

        public INamedNodeMap Attributes => attributes ?? (attributes = CreateNamedNodeMap(dependencyObject));

        public string BaseUri { get; protected set; }

        public Url BaseUrl { get; protected set; }

        public int ChildElementCount { get { return Children.Count(); } }

        public INodeList ChildNodes => childNodes ?? (childNodes = GetChildNodes(dependencyObject));

        public IHtmlCollection<IElement> Children
        {
            get
            {
                return children ?? (children = CreateCollection(ChildNodes
                    .Where(x => x is IElement)
                    .Cast<IElement>()
                    .Where(x => x != null)
                    .ToList()));
            }
        }

        public ITokenList ClassList => classList ?? (classList = GetClassList(dependencyObject));

        public string ClassName
        {
            get { return string.Join(" ", ClassList); }
            set
            {
                classList = new TokenList();
                classList.Add(value.Split(classSplitter, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        public INode FirstChild { get { return ChildNodes.FirstOrDefault(); } }

        public IElement FirstElementChild { get { return Children.FirstOrDefault(); } }

        public bool HasChildNodes { get { return ChildNodes.Any(); } }

        public string Id { get { return id; } set { id = value; } }

        public string InnerHtml { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

        public bool IsFocused { get { throw new NotImplementedException(); } }

        public INode LastChild { get { return ChildNodes.LastOrDefault(); } }

        public IElement LastElementChild { get { return Children.LastOrDefault(); } }

        public string LocalName { get; protected set; }

        public virtual string NamespaceUri { get; protected set; }

        public IElement NextElementSibling
        {
            get
            {
                if (ParentElement == null)
                {
                    return null;
                }

                var parentChildren = ParentElement.Children;

                var thisIndex = parentChildren.IndexOf(this);

                if (thisIndex == parentChildren.Length - 1)
                {
                    return null;
                }

                return parentChildren[thisIndex + 1];
            }
        }

        public INode NextSibling
        {
            get
            {
                if (Parent == null)
                {
                    return null;
                }

                var parentChildren = Parent.ChildNodes;
                var thisIndex = parentChildren.IndexOf(this);

                if (thisIndex == parentChildren.Length - 1)
                {
                    return null;
                }

                return parentChildren[thisIndex + 1];
            }
        }

        public string NodeName { get; protected set; }

        public NodeType NodeType { get; protected set; }

        public string NodeValue { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

        public string OuterHtml { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

        public IDocument Owner
        {
            get
            {
                INode currentNode = this;
                while (true)
                {
                    if (currentNode.Parent == null)
                        break;

                    currentNode = currentNode.Parent;
                }

                return currentNode as IDocument;
            }
        }

        public virtual INode Parent
        {
            get
            {
                return ParentElement;
            }
        }

        public virtual IElement ParentElement
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

        public IElement PreviousElementSibling
        {
            get
            {
                if (ParentElement == null)
                {
                    return null;
                }

                var parentChildren = ParentElement.Children;
                var thisIndex = parentChildren.IndexOf(this);

                if (thisIndex == 0)
                {
                    return null;
                }

                return parentChildren[thisIndex - 1];
            }
        }

        public INode PreviousSibling
        {
            get
            {
                if (Parent == null)
                {
                    return null;
                }

                var parentChildren = ParentElement.ChildNodes;
                var thisIndex = parentChildren.IndexOf(this);

                if (thisIndex == 0)
                {
                    return null;
                }

                return parentChildren[thisIndex - 1];
            }
        }

        public IShadowRoot ShadowRoot
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Slot
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public ICssStyleDeclaration Style
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string TagName { get; protected set; }

        public string TextContent
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IHtmlAllCollection All
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IHtmlCollection<IHtmlAnchorElement> Anchors
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IImplementation Implementation
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string DesignMode
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Direction
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string DocumentUri
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string CharacterSet
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string CompatMode
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Url
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string ContentType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IDocumentType Doctype
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IElement DocumentElement
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string LastModified
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DocumentReadyState ReadyState
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ILocation Location
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IHtmlCollection<IHtmlFormElement> Forms
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IHtmlCollection<IHtmlImageElement> Images
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IHtmlCollection<IHtmlScriptElement> Scripts
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IHtmlCollection<IHtmlEmbedElement> Plugins
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IHtmlCollection<IElement> Commands
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IHtmlCollection<IElement> Links
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Title
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IHtmlHeadElement Head
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IHtmlElement Body
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Cookie
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Origin
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Domain
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Referrer
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IElement ActiveElement
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IHtmlScriptElement CurrentScript
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IWindow DefaultView
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IBrowsingContext Context
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IDocument ImportAncestor
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IStyleSheetList StyleSheets
        {
            get
            {
                return new StyleSheetList();
            }
        }

        public string SelectedStyleSheetSet
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string LastStyleSheetSet
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string PreferredStyleSheetSet
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IStringList StyleSheetSets
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void AddEventListener(string type, DomEventHandler callback = null, bool capture = false)
        {
            throw new NotImplementedException();
        }

        public void After(params INode[] nodes)
        {
            throw new NotImplementedException();
        }

        public void Append(params INode[] nodes)
        {
            throw new NotImplementedException();
        }

        public INode AppendChild(INode child)
        {
            throw new NotImplementedException();
        }

        public IShadowRoot AttachShadow(ShadowRootMode mode = ShadowRootMode.Open)
        {
            throw new NotImplementedException();
        }

        public void Before(params INode[] nodes)
        {
            throw new NotImplementedException();
        }

        public INode Clone(bool deep = true)
        {
            throw new NotImplementedException();
        }

        public DocumentPositions CompareDocumentPosition(INode otherNode)
        {
            throw new NotImplementedException();
        }

        public bool Contains(INode otherNode)
        {
            return ChildNodes.Contains(otherNode);
        }

        public bool Dispatch(Event ev)
        {
            throw new NotImplementedException();
        }

        public bool Equals(INode otherNode)
        {
            if (otherNode == null)
            {
                return false;
            }

            if (object.ReferenceEquals(this, otherNode))
            {
                return true;
            }

            return dependencyObject == (otherNode as DomElementBase<TDependencyObject, TDependencyProperty>).dependencyObject;
        }

        public string GetAttribute(string name)
        {
            return Attributes[name]?.Value;
        }

        public string GetAttribute(string namespaceUri, string localName)
        {
            return Attributes.GetNamedItem(namespaceUri, localName)?.Value;
        }

        public IHtmlCollection<IElement> GetElementsByClassName(string classNames)
        {
            var list = new List<IElement>();

            ChildNodes.GetElementsByClassName(classNames.Split(' '), list);

            return CreateCollection(list);
        }

        public IHtmlCollection<IElement> GetElementsByTagName(string tagName)
        {
            var list = new List<IElement>();

            ChildNodes.GetElementsByTagName(tagName.Is("*") ? null : tagName, list);

            return CreateCollection(list);
        }

        public IHtmlCollection<IElement> GetElementsByTagNameNS(string namespaceUri, string tagName)
        {
            var list = new List<IElement>();

            ChildNodes.GetElementsByTagName(namespaceUri, tagName.Is("*") ? null : tagName, list);

            return CreateCollection(list);
        }

        public bool HasAttribute(string name)
        {
            return Attributes.Any(x => x.Name == name);
        }

        public bool HasAttribute(string namespaceUri, string localName)
        {
            return Attributes.Any(x => x.NamespaceUri == namespaceUri && x.LocalName == localName);
        }

        public void Insert(AdjacentPosition position, string html)
        {
            throw new NotImplementedException();
        }

        public INode InsertBefore(INode newElement, INode referenceElement)
        {
            throw new NotImplementedException();
        }

        public void InvokeEventListener(Event ev)
        {
            throw new NotImplementedException();
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
            return cachedSelectorProvider.GetOrAdd(selectors, parser.ParseSelector).Match(this);
        }

        public void Normalize()
        {

        }

        public void Prepend(params INode[] nodes)
        {
            throw new NotImplementedException();
        }

        public IPseudoElement Pseudo(string pseudoElement)
        {
            throw new NotImplementedException();
        }

        public IElement QuerySelector(string selectors)
        {
            var selector = cachedSelectorProvider.GetOrAdd(selectors, parser.ParseSelector);

            return ChildNodes.QuerySelector(selector);
        }

        public IElement QuerySelectorWithSelf(string selectors)
        {
            if (this.Matches(selectors))
            {
                return this;
            }

            var selector = cachedSelectorProvider.GetOrAdd(selectors, parser.ParseSelector);

            return ChildNodes.QuerySelector(selector);
        }

        public IHtmlCollection<IElement> QuerySelectorAll(string selectors)
        {
            var selector = cachedSelectorProvider.GetOrAdd(selectors, parser.ParseSelector);

            return CreateCollection(ChildNodes.QuerySelectorAll(selector));
        }

        public IHtmlCollection<IElement> QuerySelectorAllWithSelf(string selectors)
        {
            var selector = cachedSelectorProvider.GetOrAdd(selectors, parser.ParseSelector);

            var res = ChildNodes.QuerySelectorAll(selector);
            if (this.Matches(selectors))
            {
                res.Add(this);
            }

            return CreateCollection(res);
        }

        public void Remove()
        {
            throw new NotImplementedException();
        }

        public void RemoveAttribute(string name)
        {
            throw new NotImplementedException();
        }

        public void RemoveAttribute(string namespaceUri, string localName)
        {
            throw new NotImplementedException();
        }

        public INode RemoveChild(INode child)
        {
            throw new NotImplementedException();
        }

        public void RemoveEventListener(string type, DomEventHandler callback = null, bool capture = false)
        {
            throw new NotImplementedException();
        }

        public void Replace(params INode[] nodes)
        {
            throw new NotImplementedException();
        }

        public INode ReplaceChild(INode newChild, INode oldChild)
        {
            throw new NotImplementedException();
        }

        public void SetAttribute(string name, string value)
        {
            throw new NotImplementedException();
        }

        public void SetAttribute(string namespaceUri, string name, string value)
        {
            throw new NotImplementedException();
        }

        public void ToHtml(TextWriter writer, IMarkupFormatter formatter)
        {
            throw new NotImplementedException();
        }

        public IDocument Open(string type = "text/html", string replace = null)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Write(string content)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(string content)
        {
            throw new NotImplementedException();
        }

        public void Load(string url)
        {
            throw new NotImplementedException();
        }

        public IHtmlCollection<IElement> GetElementsByName(string name)
        {
            throw new NotImplementedException();
        }

        public IHtmlCollection<IElement> GetElementsByTagName(string namespaceUri, string tagName)
        {
            throw new NotImplementedException();
        }

        public Event CreateEvent(string type)
        {
            throw new NotImplementedException();
        }

        public IRange CreateRange()
        {
            throw new NotImplementedException();
        }

        public IComment CreateComment(string data)
        {
            throw new NotImplementedException();
        }

        public IDocumentFragment CreateDocumentFragment()
        {
            throw new NotImplementedException();
        }

        public IElement CreateElement(string name)
        {
            throw new NotImplementedException();
        }

        public IElement CreateElement(string namespaceUri, string name)
        {
            throw new NotImplementedException();
        }

        public IAttr CreateAttribute(string name)
        {
            throw new NotImplementedException();
        }

        public IAttr CreateAttribute(string namespaceUri, string name)
        {
            throw new NotImplementedException();
        }

        public IProcessingInstruction CreateProcessingInstruction(string target, string data)
        {
            throw new NotImplementedException();
        }

        public IText CreateTextNode(string data)
        {
            throw new NotImplementedException();
        }

        public INodeIterator CreateNodeIterator(INode root, FilterSettings settings = FilterSettings.All, NodeFilter filter = null)
        {
            throw new NotImplementedException();
        }

        public ITreeWalker CreateTreeWalker(INode root, FilterSettings settings = FilterSettings.All, NodeFilter filter = null)
        {
            throw new NotImplementedException();
        }

        public INode Import(INode externalNode, bool deep = true)
        {
            throw new NotImplementedException();
        }

        public INode Adopt(INode externalNode)
        {
            throw new NotImplementedException();
        }

        public bool HasFocus()
        {
            throw new NotImplementedException();
        }

        public bool ExecuteCommand(string commandId, bool showUserInterface = false, string value = "")
        {
            throw new NotImplementedException();
        }

        public bool IsCommandEnabled(string commandId)
        {
            throw new NotImplementedException();
        }

        public bool IsCommandIndeterminate(string commandId)
        {
            throw new NotImplementedException();
        }

        public bool IsCommandExecuted(string commandId)
        {
            throw new NotImplementedException();
        }

        public bool IsCommandSupported(string commandId)
        {
            throw new NotImplementedException();
        }

        public string GetCommandValue(string commandId)
        {
            throw new NotImplementedException();
        }

        public void EnableStyleSheetsForSet(string name)
        {
            throw new NotImplementedException();
        }

        public IElement GetElementById(string elementId)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public class StyleSheetList : List<IStyleSheet>, IStyleSheetList
        {
            public int Length
            {
                get
                {
                    return this.Count;
                }
            }
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
    }
}

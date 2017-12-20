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
using System.Net;

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


        protected void DomElementAdded(object sender, EventArgs e)
        {
            if (treeNodeProvider.GetParent(sender as TDependencyObject) == dependencyObject)
            {
                if (childNodes != null)
                {
                    var node = ((NamedNodeListBase<TDependencyObject, TDependencyProperty>)childNodes).Add(sender as TDependencyObject);
                    ((ElementCollectionBase<TDependencyObject>)children)?.Add((IElement)node);
                }
            }
        }

        protected void DomElementRemoved(object sender, EventArgs e)
        {
            if (Children.Any(x => ((IDomElement<TDependencyObject>)x).Element == sender))
            {
                if (childNodes != null)
                {
                    var node = ((NamedNodeListBase<TDependencyObject, TDependencyProperty>)childNodes).Remove(sender as TDependencyObject);
                    ((ElementCollectionBase<TDependencyObject>)children)?.Remove((IElement)node);
                }
            }
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

        public string InnerHtml { get { return ""; } set { } }

        public bool IsFocused { get { return false; } }

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

                if (thisIndex == parentChildren.Length - 1 ||
                    thisIndex < 0)
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

                if (thisIndex == parentChildren.Length - 1 ||
                    thisIndex < 0)
                {
                    return null;
                }

                return parentChildren[thisIndex + 1];
            }
        }

        public string NodeName { get; protected set; }

        public NodeType NodeType { get; protected set; }

        public string NodeValue { get { return ""; } set { } }

        public string OuterHtml { get { return ""; } set {  } }

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

                if (thisIndex <= 0)
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

                if (thisIndex <= 0)
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
                return null;
            }
        }

        public string Slot
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public ICssStyleDeclaration Style
        {
            get
            {
                return null;
            }
        }

        public string TagName { get; protected set; }

        public string TextContent
        {
            get
            {
                return "";
            }
            set
            {
                
            }
        }

        public IHtmlAllCollection All
        {
            get
            {
                return null;
            }
        }

        public IHtmlCollection<IHtmlAnchorElement> Anchors
        {
            get
            {
                return null;
            }
        }

        public IImplementation Implementation
        {
            get
            {
                return null;
            }
        }

        public string DesignMode
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public string Direction
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public string DocumentUri
        {
            get
            {
                return "";
            }
        }

        public string CharacterSet
        {
            get
            {
                return "";
            }
        }

        public string CompatMode
        {
            get
            {
                return "";
            }
        }

        public string Url
        {
            get
            {
                return "";
            }
        }

        public string ContentType
        {
            get
            {
                return "";
            }
        }

        public IDocumentType Doctype
        {
            get
            {
                return null;
            }
        }

        public IElement DocumentElement
        {
            get
            {
                return null;
            }
        }

        public string LastModified
        {
            get
            {
                return "";
            }
        }

        public DocumentReadyState ReadyState
        {
            get
            {
                return DocumentReadyState.Complete;
            }
        }

        public ILocation Location
        {
            get
            {
                return null;
            }
        }

        public IHtmlCollection<IHtmlFormElement> Forms
        {
            get
            {
                return null;
            }
        }

        public IHtmlCollection<IHtmlImageElement> Images
        {
            get
            {
                return null;
            }
        }

        public IHtmlCollection<IHtmlScriptElement> Scripts
        {
            get
            {
                return null;
            }
        }

        public IHtmlCollection<IHtmlEmbedElement> Plugins
        {
            get
            {
                return null;
            }
        }

        public IHtmlCollection<IElement> Commands
        {
            get
            {
                return null;
            }
        }

        public IHtmlCollection<IElement> Links
        {
            get
            {
                return null;
            }
        }

        public string Title
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public IHtmlHeadElement Head
        {
            get
            {
                return null;
            }
        }

        public IHtmlElement Body
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public string Cookie
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public string Origin
        {
            get
            {
                return "";
            }
        }

        public string Domain
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public string Referrer
        {
            get
            {
                return "";
            }
        }

        public IElement ActiveElement
        {
            get
            {
                return null;
            }
        }

        public IHtmlScriptElement CurrentScript
        {
            get
            {
                return null;
            }
        }

        public IWindow DefaultView
        {
            get
            {
                return null;
            }
        }

        public IBrowsingContext Context
        {
            get
            {
                return null;
            }
        }

        public IDocument ImportAncestor
        {
            get
            {
                return null;
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
                return null;
            }
            set
            {
            }
        }

        public string LastStyleSheetSet
        {
            get
            {
                return null;
            }
        }

        public string PreferredStyleSheetSet
        {
            get
            {
                return null;
            }
        }

        public IStringList StyleSheetSets
        {
            get
            {
                return null;
            }
        }

        public TextSource Source => new TextSource("");

        public HttpStatusCode StatusCode => HttpStatusCode.OK;

        public void AddEventListener(string type, DomEventHandler callback = null, bool capture = false)
        {
        }

        public void After(params INode[] nodes)
        {
        }

        public void Append(params INode[] nodes)
        {
        }

        public INode AppendChild(INode child)
        {
            return null;
        }

        public IShadowRoot AttachShadow(ShadowRootMode mode = ShadowRootMode.Open)
        {
            return null;
        }

        public void Before(params INode[] nodes)
        {
        }

        public INode Clone(bool deep = true)
        {
            return null;
        }

        public DocumentPositions CompareDocumentPosition(INode otherNode)
        {
            return DocumentPositions.ImplementationSpecific;
        }

        public bool Contains(INode otherNode)
        {
            return ChildNodes.Contains(otherNode);
        }

        public bool Dispatch(Event ev)
        {
            return false;
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
        }

        public INode InsertBefore(INode newElement, INode referenceElement)
        {
            return null;
        }

        public void InvokeEventListener(Event ev)
        {
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
        }

        public IPseudoElement Pseudo(string pseudoElement)
        {
            return null;
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
        }

        public void RemoveAttribute(string name)
        {
        }

        public void RemoveAttribute(string namespaceUri, string localName)
        {
        }

        public INode RemoveChild(INode child)
        {
            return null;
        }

        public void RemoveEventListener(string type, DomEventHandler callback = null, bool capture = false)
        {
        }

        public void Replace(params INode[] nodes)
        {
        }

        public INode ReplaceChild(INode newChild, INode oldChild)
        {
            return null;
        }

        public void SetAttribute(string name, string value)
        {
        }

        public void SetAttribute(string namespaceUri, string name, string value)
        {
        }

        public void ToHtml(TextWriter writer, IMarkupFormatter formatter)
        {
        }

        public IDocument Open(string type = "text/html", string replace = null)
        {
            return null;
        }

        public void Close()
        {
        }

        public void Write(string content)
        {
        }

        public void WriteLine(string content)
        {
        }

        public void Load(string url)
        {
        }

        public IHtmlCollection<IElement> GetElementsByName(string name)
        {
            return null;
        }

        public IHtmlCollection<IElement> GetElementsByTagName(string namespaceUri, string tagName)
        {
            return null;
        }

        public Event CreateEvent(string type)
        {
            return null;
        }

        public IRange CreateRange()
        {
            return null;
        }

        public IComment CreateComment(string data)
        {
            return null;
        }

        public IDocumentFragment CreateDocumentFragment()
        {
            return null;
        }

        public IElement CreateElement(string name)
        {
            return null;
        }

        public IElement CreateElement(string namespaceUri, string name)
        {
            return null;
        }

        public IAttr CreateAttribute(string name)
        {
            return null;
        }

        public IAttr CreateAttribute(string namespaceUri, string name)
        {
            return null;
        }

        public IProcessingInstruction CreateProcessingInstruction(string target, string data)
        {
            return null;
        }

        public IText CreateTextNode(string data)
        {
            return null;
        }

        public INodeIterator CreateNodeIterator(INode root, FilterSettings settings = FilterSettings.All, NodeFilter filter = null)
        {
            return null;
        }

        public ITreeWalker CreateTreeWalker(INode root, FilterSettings settings = FilterSettings.All, NodeFilter filter = null)
        {
            return null;
        }

        public INode Import(INode externalNode, bool deep = true)
        {
            return null;
        }

        public INode Adopt(INode externalNode)
        {
            return null;
        }

        public bool HasFocus()
        {
            return false;
        }

        public bool ExecuteCommand(string commandId, bool showUserInterface = false, string value = "")
        {
            return false;
        }

        public bool IsCommandEnabled(string commandId)
        {
            return false;
        }

        public bool IsCommandIndeterminate(string commandId)
        {
            return false;
        }

        public bool IsCommandExecuted(string commandId)
        {
            return false;
        }

        public bool IsCommandSupported(string commandId)
        {
            return false;
        }

        public string GetCommandValue(string commandId)
        {
            return null;
        }

        public void EnableStyleSheetsForSet(string name)
        {
        }

        public IElement GetElementById(string elementId)
        {
            return null;
        }

        public void Dispose()
        {
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

        bool IElement.RemoveAttribute(string name)
        {
            return attributes.RemoveNamedItem(name) != null;
        }

        bool IElement.RemoveAttribute(string namespaceUri, string localName)
        {
            return attributes.RemoveNamedItem(namespaceUri, localName) != null;
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

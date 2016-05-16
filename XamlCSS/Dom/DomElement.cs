using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Css;
using AngleSharp.Dom.Events;
using AngleSharp.Parser.Css;

namespace XamlCSS.Dom
{
	public abstract class DomElementBase<TDependencyObject, TDependencyProperty> : IDomElement<TDependencyObject>
		where TDependencyObject : class
		where TDependencyProperty : class
	{
		protected readonly TDependencyObject dependencyObject;
		protected string id;
		protected readonly CssParser parser;

		public DomElementBase(
			TDependencyObject dependencyObject,
			IElement parentElement
			)
		{
			this.dependencyObject = dependencyObject;
			this.Attributes = CreateNamedNodeMap(dependencyObject);
			this.BaseUri = dependencyObject.GetType().Namespace;
			this.ChildNodes = GetChildNodes(dependencyObject);
			this.ClassList = GetClassList(dependencyObject);
			this.id = GetId(dependencyObject);
			this.LocalName = dependencyObject.GetType().Name;
			this.NamespaceUri = dependencyObject.GetType().Namespace;
			this.NodeName = dependencyObject.GetType().Name;
			this.NodeType = NodeType.Element;
			//this.Owner = ...
			this.Parent = parentElement;
			this.ParentElement = parentElement;
			this.TagName = dependencyObject.GetType().Name;

			this.parser = new CssParser(new CssParserOptions()
			{
				IsIncludingUnknownDeclarations = true,
				IsStoringTrivia = false,
				IsIncludingUnknownRules = true,
				IsToleratingInvalidConstraints = true,
				IsToleratingInvalidSelectors = true,
				IsToleratingInvalidValues = true
			});
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

		public INamedNodeMap Attributes { get; protected set; }

		public string BaseUri { get; protected set; }

		public Url BaseUrl { get; protected set; }

		public int ChildElementCount { get { return Children.Count(); } }

		public INodeList ChildNodes { get; protected set; }

		public IHtmlCollection<IElement> Children
		{
			get
			{
				return CreateCollection(ChildNodes
					.Where(x => x is IElement)
					.Cast<IElement>()
					.Where(x => x != null)
					.ToList());
			}
		}

		public ITokenList ClassList { get; protected set; }

		public string ClassName
		{
			get { return string.Join(" ", ClassList); }
			set
			{
				ClassList = new TokenList();
				ClassList.Add(value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
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

		public string NamespaceUri { get; protected set; }

		public IElement NextElementSibling
		{
			get
			{
				if (ParentElement == null)
					return null;
				var children = ParentElement.Children;
				var thisIndex = children.IndexOf(this);
				if (thisIndex == children.Length - 1)
					return null;
				return children[thisIndex + 1];
			}
		}

		public INode NextSibling
		{
			get
			{
				if (Parent == null)
					return null;
				var children = Parent.ChildNodes;
				var thisIndex = children.IndexOf(this);
				if (thisIndex == children.Length - 1)
					return null;
				return children[thisIndex + 1];
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
				throw new NotImplementedException();
			}
		}

		public INode Parent { get; protected set; }

		public IElement ParentElement { get; protected set; }

		public string Prefix { get; protected set; }

		public IElement PreviousElementSibling
		{
			get
			{
				if (ParentElement == null)
					return null;
				var children = ParentElement.Children;
				var thisIndex = children.IndexOf(this);
				if (thisIndex == 0)
					return null;
				return children[thisIndex - 1];
			}
		}

		public INode PreviousSibling
		{
			get
			{
				if (Parent == null)
					return null;
				var children = ParentElement.ChildNodes;
				var thisIndex = children.IndexOf(this);
				if (thisIndex == 0)
					return null;
				return children[thisIndex - 1];
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
				return false;

			if (object.ReferenceEquals(this, otherNode))
				return true;

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
			return true;
		}

		public string LookupNamespaceUri(string prefix)
		{
			throw new NotImplementedException();
		}

		public string LookupPrefix(string namespaceUri)
		{
			throw new NotImplementedException();
		}

		public bool Matches(string selectors)
		{
			return parser.ParseSelector(selectors).Match(this);
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
			return ChildNodes.QuerySelector(selectors, parser);
		}

		public IElement QuerySelectorWithSelf(string selectors)
		{
			if (this.Matches(selectors))
				return this;

			var res = ChildNodes.QuerySelector(selectors, parser);
			return res;
		}

		public IHtmlCollection<IElement> QuerySelectorAll(string selectors)
		{
			return CreateCollection(ChildNodes.QuerySelectorAll(selectors, parser));
		}

		public IHtmlCollection<IElement> QuerySelectorAllWithSelf(string selectors)
		{
			var res = ChildNodes.QuerySelectorAll(selectors, parser);
			if (this.Matches(selectors))
				res.Add(this);
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
	}
}

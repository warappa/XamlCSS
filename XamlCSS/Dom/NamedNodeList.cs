using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngleSharp;
using AngleSharp.Dom;

namespace XamlCSS.Dom
{
	public abstract class NamedNodeListBase<TDependencyObject, TDependencyProperty> : INodeList
		where TDependencyObject : class
		where TDependencyProperty : class
	{
		private List<INode> nodes;
        protected ITreeNodeProvider<TDependencyObject> treeNodeProvider;

        public NamedNodeListBase(IDomElement<TDependencyObject> node, ITreeNodeProvider<TDependencyObject> treeNodeProvider)
		{
            this.treeNodeProvider = treeNodeProvider;

            InitNodes(node, treeNodeProvider);
		}

		private void InitNodes(IDomElement<TDependencyObject> node, ITreeNodeProvider<TDependencyObject> treeNodeProvider)
		{
            this.treeNodeProvider = treeNodeProvider;

            this.nodes = GetChildren(node.Element)
				.Select(x => CreateNode(x, node))
				.ToList();
		}

		public NamedNodeListBase(IEnumerable<INode> nodes, ITreeNodeProvider<TDependencyObject> treeNodeProvider)
		{
            this.treeNodeProvider = treeNodeProvider;

            this.nodes = nodes.ToList();
		}

        public IDomElement<TDependencyObject> Add(TDependencyObject dependencyObject)
        {
            var node = treeNodeProvider.GetDomElement(dependencyObject);
            node.ChildNodes.Add(node);

            return node;
        }

        public INode Remove(TDependencyObject dependencyObject)
        {
            var node = nodes.First(x => ((IDomElement<TDependencyObject>)x).Element == dependencyObject);
            var removed = nodes.Remove(node);
            if (!removed)
            {
                throw new Exception("remove failed!");
            }

            return node;
        }

		abstract protected INode CreateNode(TDependencyObject dependencyObject, IDomElement<TDependencyObject> parentNode);

		abstract protected IEnumerable<TDependencyObject> GetChildren(TDependencyObject dependencyObject);

		public INode this[int index] { get { return nodes[index]; } }

		public int Length { get { return nodes.Count; } }

		public IEnumerator<INode> GetEnumerator()
		{
			return nodes.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return nodes.GetEnumerator();
		}

		public void ToHtml(TextWriter writer, IMarkupFormatter formatter)
		{
			throw new NotImplementedException();
		}
	}
}

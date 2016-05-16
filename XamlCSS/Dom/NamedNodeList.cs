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

		public NamedNodeListBase(IDomElement<TDependencyObject> node)
		{
			var vals = GetChildren(node.Element)
				.Select(x => CreateNode(x, node))
				.ToList();
			this.nodes = vals;
		}

		public NamedNodeListBase(IEnumerable<INode> nodes)
		{
			this.nodes = nodes.ToList();
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

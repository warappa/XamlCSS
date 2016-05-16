using System.Collections.Generic;
using AngleSharp.Dom;
using XamlCSS.Dom;
using Xamarin.Forms;

namespace XamlCSS.XamarinForms.Dom
{
	public abstract class DomElement : DomElementBase<BindableObject, BindableProperty>
	{
		public DomElement(BindableObject dependencyObject, IElement parent)
			: base(dependencyObject, parent)
		{

		}
		protected override IHtmlCollection<IElement> CreateCollection(IEnumerable<IElement> list)
		{
			return new ElementCollection(list);
		}
		protected override INamedNodeMap CreateNamedNodeMap(BindableObject dependencyObject)
		{
			return new NamedNodeMap(dependencyObject);
		}

		protected override IHtmlCollection<IElement> GetChildElements(BindableObject dependencyObject)
		{
			return new ElementCollection(this);
		}
		protected override INodeList GetChildNodes(BindableObject dependencyObject)
		{
			return new NamedNodeList(this);
		}
		protected override INodeList CreateNodeList(IEnumerable<INode> nodes)
		{
			return new NamedNodeList(nodes);
		}
		protected override ITokenList GetClassList(BindableObject dependencyObject)
		{
			var list = new TokenList();
			var strs = Css.GetClass(dependencyObject)?.Split(' ');
			if (strs != null)
				list.AddRange(strs);
			return list;
		}
		protected override string GetId(BindableObject dependencyObject)
		{
			return Css.GetId(dependencyObject);
		}
	}
}

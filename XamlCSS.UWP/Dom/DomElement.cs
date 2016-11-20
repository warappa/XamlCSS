using System.Collections.Generic;
using AngleSharp.Dom;
using XamlCSS.Dom;
using Windows.UI.Xaml;
using System;

namespace XamlCSS.UWP.Dom
{
	public abstract class DomElement : DomElementBase<DependencyObject, DependencyProperty>
	{
		public DomElement(DependencyObject dependencyObject, IElement parent)
			: base(dependencyObject, parent)
        {
            RegisterChildrenChangeHandler();
        }
		public DomElement(DependencyObject dependencyObject, Func<DependencyObject, IElement> getParent)
			: base(dependencyObject, getParent)
		{
            RegisterChildrenChangeHandler();
        }

        private void RegisterChildrenChangeHandler()
        {
            LoadedDetectionHelper.SubTreeAdded += VisualDomElement_ChildAdded;
            LoadedDetectionHelper.SubTreeRemoved += VisualDomElement_ChildAdded;
        }

        public new void Dispose()
        {
            UnregisterChildrenChangeHandler();

            base.Dispose();
        }

        private void UnregisterChildrenChangeHandler()
        {
            LoadedDetectionHelper.SubTreeAdded -= VisualDomElement_ChildAdded;
            LoadedDetectionHelper.SubTreeRemoved -= VisualDomElement_ChildAdded;
        }

        private void VisualDomElement_ChildAdded(object sender, EventArgs e)
        {
            if ((sender as FrameworkElement)?.Parent == dependencyObject)
            { 
                this.ResetChildren();
            }
        }


        protected override IHtmlCollection<IElement> CreateCollection(IEnumerable<IElement> list)
		{
			return new ElementCollection(list);
		}
		protected override INamedNodeMap CreateNamedNodeMap(DependencyObject dependencyObject)
		{
			return new NamedNodeMap(dependencyObject);
		}

		protected override IHtmlCollection<IElement> GetChildElements(DependencyObject dependencyObject)
		{
			return new ElementCollection(this);
		}
		protected override INodeList GetChildNodes(DependencyObject dependencyObject)
		{
			return new NamedNodeList(this);
		}
		protected override INodeList CreateNodeList(IEnumerable<INode> nodes)
		{
			return new NamedNodeList(nodes);
		}
		protected override ITokenList GetClassList(DependencyObject dependencyObject)
		{
			var list = new TokenList();
			var classNames = Css.GetClass(dependencyObject)?.Split(' ');

            if (classNames != null)
            {
                list.AddRange(classNames);
            }

            return list;
		}
		protected override string GetId(DependencyObject dependencyObject)
		{
			return dependencyObject.ReadLocalValue(FrameworkElement.NameProperty) as string;
		}
	}
}

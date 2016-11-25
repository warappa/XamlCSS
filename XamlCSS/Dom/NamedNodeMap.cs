using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;

namespace XamlCSS.Dom
{
	public abstract class NamedNodeMapBase<TDependencyObject, TDependencyProperty> : INamedNodeMap
		where TDependencyObject : class
		where TDependencyProperty : class
	{
		protected List<IAttr> attributes = null;
        private TDependencyObject dependencyObject;

        public NamedNodeMapBase(TDependencyObject dependencyObject)
		{
            this.dependencyObject = dependencyObject;
		}

        protected List<IAttr> Attributes => attributes ?? (attributes = GetProperties(dependencyObject)
                            .Select(x => CreateAttribute(dependencyObject, x))
                            .ToList());
        
		public NamedNodeMapBase(IEnumerable<IAttr> attributes)
		{
			this.attributes = attributes.ToList();
		}

		abstract protected IAttr CreateAttribute(TDependencyObject dependencyObject, TDependencyProperty property);

		protected IEnumerable<TDependencyProperty> GetProperties(TDependencyObject dependencyObject)
		{
			var dps = TypeHelpers.DeclaredFields(dependencyObject.GetType())
				.Where(x => x.FieldType == typeof(TDependencyProperty))
				.Select(x => x.GetValue(dependencyObject) as TDependencyProperty)
				.ToList();

			return dps;
		}

		public IAttr this[int index] { get { return Attributes[index]; } }

		public IAttr this[string name] { get { return GetNamedItem(name); } }

		public int Length { get { return Attributes.Count; } }

		public IEnumerator<IAttr> GetEnumerator()
		{
			return Attributes.GetEnumerator();
		}

		public IAttr GetNamedItem(string name)
		{
			return Attributes.FirstOrDefault(x => x.Name == name);
		}

		public IAttr GetNamedItem(string namespaceUri, string localName)
		{
			return GetNamedItem($"{namespaceUri}.{localName}");
		}

		public IAttr RemoveNamedItem(string name)
		{
			throw new NotSupportedException();
		}

		public IAttr RemoveNamedItem(string namespaceUri, string localName)
		{
			throw new NotSupportedException();
		}

		public IAttr SetNamedItem(IAttr item)
		{
			throw new NotSupportedException();
		}

		public IAttr SetNamedItemWithNamespaceUri(IAttr item)
		{
			throw new NotSupportedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Attributes.GetEnumerator();
		}
	}
}

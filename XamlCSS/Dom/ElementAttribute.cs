using AngleSharp.Dom;

namespace XamlCSS.Dom
{
	public abstract class ElementAttributeBase<TDependencyObject, TDependencyProperty> : IAttr
		where TDependencyObject : class
		where TDependencyProperty : class
	{
		protected TDependencyObject dependencyObject;
		protected TDependencyProperty property;

		public ElementAttributeBase(TDependencyObject dependencyObject, TDependencyProperty property)
		{
			this.dependencyObject = dependencyObject;
			this.property = property;
		}

		public string LocalName
		{
			get
			{
				return property.GetType().Name;
			}
		}

		public string Name
		{
			get
			{
				return property.GetType().FullName;
			}
		}

		public string NamespaceUri
		{
			get
			{
				return property.GetType().Namespace;
			}
		}

		public string Prefix
		{
			get
			{
				return null;
			}
		}

		abstract public string Value { get; set; }

		public bool Equals(IAttr other)
		{
			if (object.ReferenceEquals(this, other))
				return true;
			if (other == null)
				return false;

			return property == ((ElementAttributeBase<TDependencyObject, TDependencyProperty>)other).property;
		}
	}
}

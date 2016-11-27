using System.Collections.Generic;
using XamlCSS.Dom;

namespace XamlCSS
{
	public interface ITreeNodeProvider<TDependencyObject>
		where TDependencyObject : class
	{
		IDomElement<TDependencyObject> GetDomElement(TDependencyObject obj);
		IEnumerable<IDomElement<TDependencyObject>> GetDomElementChildren(IDomElement<TDependencyObject> node);
		IEnumerable<TDependencyObject> GetChildren(TDependencyObject element);
		TDependencyObject GetParent(TDependencyObject tUIElement);
	}
}

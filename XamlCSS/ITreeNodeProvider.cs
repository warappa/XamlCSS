using System.Collections.Generic;
using XamlCSS.Dom;

namespace XamlCSS
{
	public interface ITreeNodeProvider<TDependencyObject>
		where TDependencyObject : class
	{
		IDomElement<TDependencyObject> GetVisualTree(TDependencyObject obj);
		IDomElement<TDependencyObject> GetLogicalTree(TDependencyObject obj);
		IEnumerable<IDomElement<TDependencyObject>> GetChildren(IDomElement<TDependencyObject> node);
		IEnumerable<TDependencyObject> GetChildren(TDependencyObject element);
		TDependencyObject GetParent(TDependencyObject tUIElement);
	}
}

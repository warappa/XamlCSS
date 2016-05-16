using AngleSharp.Dom;

namespace XamlCSS.Dom
{
	public interface IDomElement<TDependencyObject> : IElement
	{
		TDependencyObject Element { get; }
		IElement QuerySelectorWithSelf(string selectors);
		IHtmlCollection<IElement> QuerySelectorAllWithSelf(string selectors);
	}
}

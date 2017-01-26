using AngleSharp.Dom;
using System.Collections.Generic;

namespace XamlCSS.Dom
{
	public interface IDomElement<TDependencyObject> : IElement
	{
		TDependencyObject Element { get; }
		List<SingleStyleSheet> XamlCssStyleSheets { get; }
		IElement QuerySelectorWithSelf(string selectors);
		IHtmlCollection<IElement> QuerySelectorAllWithSelf(string selectors);
	}
}

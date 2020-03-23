using System.Collections.Generic;

namespace XamlCSS.WPF.Internals
{
    public interface IProvideParentValuesPublic
	{
		IEnumerable<object> ParentObjects { get; }
	}
}
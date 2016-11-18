using System.Collections.Generic;

namespace XamlCSS.XamarinForms.Internals
{
    public interface IProvideParentValuesPublic
	{
		IEnumerable<object> ParentObjects { get; }
	}
}
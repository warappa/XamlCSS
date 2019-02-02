using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamlCSS.XamarinForms.Internals
{
    [XamlCSS.Linker.Preserve(AllMembers = true)]
    public class ProvideValueTarget : IProvideValueTarget, IProvideParentValuesPublic
	{
		public ProvideValueTarget(object target)
		{
			this.TargetObject = target;
		}
		public IEnumerable<object> ParentObjects
		{
			get
			{
				var parent = TargetObject;
				while(parent != null)
				{
					yield return parent;
					parent = ((Element)parent).Parent;
				}
			}
		}

		public object TargetObject { get; set; }

		public object TargetProperty { get; set; }
	}
}
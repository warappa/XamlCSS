using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;

namespace XamlCSS.WPF.Internals
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
                while (parent != null)
                {
                    yield return parent;
                    if(parent is FrameworkElement fe)
                    {
                        parent = fe.Parent;
                    }
                    else if(parent is FrameworkContentElement fce)
                    {
                        parent = fce.Parent;
                    }
                    else
                    {
                        parent = null;
                    }
                    
                }
            }
        }

        public object TargetObject { get; set; }

        public object TargetProperty { get; set; }
    }
}
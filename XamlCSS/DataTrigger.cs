using System.Collections.Generic;

namespace XamlCSS
{
    public class DataTrigger : ITrigger
    {
        public string Binding { get; set; }
        public string Value { get; set; }

        public StyleDeclarationBlock StyleDeclaraionBlock { get; set; }
        public List<TriggerAction> EnterActions { get; set; }
        public List<TriggerAction> ExitActions { get; set; }
    }
}

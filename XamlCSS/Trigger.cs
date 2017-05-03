using System.Collections.Generic;

namespace XamlCSS
{
    public class Trigger : ITrigger
    {
        public string Property { get; set; }
        public string Value { get; set; }

        public StyleDeclarationBlock StyleDeclarationBlock { get; set; }

        public List<TriggerAction> EnterActions { get; set; } = new List<TriggerAction>();
        public List<TriggerAction> ExitActions { get; set; } = new List<TriggerAction>();
    }
}

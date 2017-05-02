using System;
using System.Collections.Generic;
using System.Linq;

namespace XamlCSS
{
    public class Trigger : ITrigger
    {
        public string Property { get; set; }
        public string Value { get; set; }

        public StyleDeclarationBlock StyleDeclaraionBlock { get; set; }

        public List<TriggerAction> EnterActions { get; set; } = new List<TriggerAction>();
        public List<TriggerAction> ExitActions { get; set; } = new List<TriggerAction>();
    }
}

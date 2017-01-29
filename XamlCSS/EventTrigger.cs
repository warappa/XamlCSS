using System.Collections.Generic;

namespace XamlCSS
{
    public class EventTrigger : ITrigger
    {
        public string Event { get; set; }
        public List<TriggerAction> Actions { get; set; }
    }
}

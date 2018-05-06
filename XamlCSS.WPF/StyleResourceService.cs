using System.Collections.Generic;

namespace XamlCSS.WPF
{
    public class StyleResourceService : IStyleResourcesService
    {
        private Dictionary<object, object> resources = new Dictionary<object, object>();

        public StyleResourceService()
        {

        }

        public void BeginUpdate()
        {
        }

        public bool Contains(object key)
        {
            return resources.ContainsKey(key);
        }

        public void EndUpdate()
        {
        }

        public void EnsureResources()
        {
            
        }

        public IEnumerable<object> GetKeys()
        {
            return resources.Keys;
        }

        public object GetResource(object key)
        {
            resources.TryGetValue(key, out object value);
            return value;
        }

        public void RemoveResource(object key)
        {
            resources.Remove(key);
        }

        public void SetResource(object key, object value)
        {
            resources[key] = value;
        }
    }
}

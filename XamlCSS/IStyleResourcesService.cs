using System.Collections.Generic;

namespace XamlCSS
{
    public interface IStyleResourcesService
    {
        void SetResource(object key, object value);
        void RemoveResource(object key);
        bool Contains(object key);
        IEnumerable<object> GetKeys();
        object GetResource(object key);
        void EnsureResources();
    }
}

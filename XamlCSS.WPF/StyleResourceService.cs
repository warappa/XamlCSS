using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace XamlCSS.WPF
{
    public class StyleResourceService : IStyleResourcesService
    {
        public bool Contains(object key)
        {
            return Application.Current.Resources.Contains(key);
        }

        public void EnsureResources()
        {
            if (Application.Current.Resources == null)
            {
                Application.Current.Resources = new ResourceDictionary();
            }
        }

        public IEnumerable<object> GetKeys()
        {
            return Application.Current.Resources.Keys.Cast<object>();
        }

        public object GetResource(object key)
        {
            return Application.Current.Resources[key];
        }

        public void RemoveResource(object key)
        {
            Application.Current.Resources.Remove(key);
        }

        public void SetResource(object key, object value)
        {
            Application.Current.Resources[key] = value;
        }
    }
}

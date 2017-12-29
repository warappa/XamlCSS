using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using XamlCSS.Dom;

namespace XamlCSS.XamarinForms
{
	public class StyleResourceService : IStyleResourcesService
	{
        public void BeginUpdate()
        {
            
        }

        public bool Contains(object key)
		{
			return Application.Current.Resources.Keys.Contains(key);
		}

        public void EndUpdate()
        {
            
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
			return Application.Current.Resources?.Keys.Cast<object>() ?? Enumerable.Empty<object>();
		}

		public object GetResource(object key)
		{
			return Application.Current.Resources[key as string];
		}

		public void RemoveResource(object key)
		{
			Application.Current.Resources.Remove(key as string);
		}

		public void SetResource(object key, object value)
		{
			Application.Current.Resources[key as string] = value;
		}
	}
}

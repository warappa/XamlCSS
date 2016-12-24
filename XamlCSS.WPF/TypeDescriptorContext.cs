using System;
using System.ComponentModel;
using System.Windows.Markup;

namespace XamlCSS.WPF
{
    public class TypeDescriptorContext : ITypeDescriptorContext
    {
        private readonly UriContext uriContext;

        public class UriContext : IUriContext
        {
            public Uri BaseUri { get; set; }
        }

        public TypeDescriptorContext(Uri baseUri)
        {
            this.uriContext = new UriContext
            {
                BaseUri = baseUri
            };
        }

        public IContainer Container
        {
            get
            {
                return null;
            }
        }

        public object Instance
        {
            get
            {
                return null;
            }
        }

        public PropertyDescriptor PropertyDescriptor
        {
            get
            {
                return null;
            }
        }

        public object GetService(Type serviceType)
        {
            if(serviceType == typeof(IUriContext))
            {
                return uriContext;
            }
            return null;
        }

        public void OnComponentChanged()
        {
            
        }

        public bool OnComponentChanging()
        {
            return false;
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace XamlCSS.CssParsing
{
    public abstract class CssFileProviderBase : ICssFileProvider
    {
        protected List<Assembly> assemblies = new List<Assembly>();

        public CssFileProviderBase(IEnumerable<Assembly> assemblies)
        {
            this.assemblies.AddRange(assemblies);
        }

        public virtual string LoadFrom(string source)
        {
            Stream stream = null;

            stream = TryGetFromResource(source, assemblies.ToArray());

            if (stream == null)
            {
                stream = TryGetFromFile(source);
            }

            if (stream == null)
            {
                return null;
            }

            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        protected abstract Stream TryGetFromFile(string source);

        protected virtual Stream TryGetFromResource(string source, params Assembly[] searchAssemblies)
        {
            Stream stream = null;

            foreach (var assembly in searchAssemblies)
            {
                var resourceName = GetEmbeddedResourceName(source, assembly);
                try
                {
                    if (assembly.GetManifestResourceNames().ToList().Contains(resourceName))
                    {
                        stream = assembly.GetManifestResourceStream(resourceName);
                        break;
                    }
                }
                catch
                {

                }
            }

            return stream;
        }

        protected virtual string GetEmbeddedResourceName(string source, Assembly assembly)
        {
            return GetEmbeddedResourcePrefix(assembly) + source.Replace("\\", ".").Replace("/", ".");
        }

        protected virtual string GetEmbeddedResourcePrefix(Assembly assembly)
        {
            return GetEmbeddedResourcePrefix(assembly.GetName().Name);
        }

        protected virtual string GetEmbeddedResourcePrefix(string assemblyName)
        {
            return assemblyName.Replace("\\", ".").Replace("/", ".") + ".";
        }
    }
}

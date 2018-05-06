using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using XamlCSS.Utils;

namespace XamlCSS.CssParsing
{
    public abstract class CssFileProviderBase : ICssFileProvider
    {
        protected Assembly[] assemblies = null;

        public CssFileProviderBase(IEnumerable<Assembly> assemblies)
        {
            this.assemblies = assemblies.ToArray();
        }

        public virtual string LoadFrom(string source)
        {
            Stream stream = null;

            stream = TryGetFromEmbeddedResource(source, assemblies);

            if (stream == null)
            {
                stream = TryGetFromFile(source);
            }

            if (stream == null)
            {
                stream = TryLoadFromStaticApplicationResource(source);
            }

            if (stream == null)
            {
                return null;
            }

            return ReadStream(stream);
        }

        protected string ReadStream(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        protected abstract Stream TryLoadFromStaticApplicationResource(string source);

        protected abstract Stream TryGetFromFile(string source);

        protected virtual Stream TryGetFromEmbeddedResource(string source, params Assembly[] searchAssemblies)
        {
            Stream stream = null;

            foreach (var assembly in searchAssemblies)
            {
                var resourceName = GetEmbeddedResourceName(source, assembly);
                try
                {
                    if (assembly.GetManifestResourceNames().ToHashSet().Contains(resourceName))
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

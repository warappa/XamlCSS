using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Windows.Storage;
using Windows.UI.Xaml;
using XamlCSS.CssParsing;

namespace XamlCSS.UWP.CssParsing
{
    public class CssFileProvider : CssFileProviderBase
    {
        public CssFileProvider()
            : base(new[] { Application.Current.GetType().GetTypeInfo().Assembly })
        {
        }

        public CssFileProvider(IEnumerable<Assembly> assemblies)
            : this()
        {
            this.assemblies.AddRange(assemblies);
        }

        protected override Stream TryGetFromFile(string source)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            var absolutePath = source;
            if (!Path.IsPathRooted(absolutePath))
            {
                absolutePath = Path.Combine(storageFolder.Path, absolutePath);
            }

            if (File.Exists(absolutePath))
            {
                try
                {
                    return File.OpenRead(absolutePath);
                }
                catch { }
            }

            return null;
        }
    }
}

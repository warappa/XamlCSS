using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using XamlCSS.CssParsing;

namespace XamlCSS.XamarinForms.CssParsing
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
            Stream stream = null;

            try
            {
                var fileSystem = PCLStorage.FileSystem.Current;

                var filepath = Path.Combine(fileSystem.LocalStorage.Path, source);

                if (fileSystem.GetFileFromPathAsync(filepath).Result != null)
                {
                    stream = fileSystem.GetFileFromPathAsync(filepath).Result.OpenAsync(PCLStorage.FileAccess.Read).Result;
                }
            }
            catch { }

            return stream;
        }

        protected override Stream TryGetFromResource(string source, params Assembly[] assemblies)
        {
            return base.TryGetFromResource(source, assemblies.Concat(this.assemblies).Distinct().ToArray());
        }
    }
}

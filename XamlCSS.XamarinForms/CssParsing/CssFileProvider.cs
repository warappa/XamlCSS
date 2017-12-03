using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xamarin.Forms;
using XamlCSS.CssParsing;

namespace XamlCSS.XamarinForms.CssParsing
{
    public class CssFileProvider : CssFileProviderBase
    {
        private readonly CssTypeHelper<BindableObject, BindableObject, BindableProperty, Style> cssTypeHelper;

        public CssFileProvider(CssTypeHelper<BindableObject, BindableObject, BindableProperty, Style> cssTypeHelper)
            : base(new[] { Application.Current.GetType().GetTypeInfo().Assembly })
        {
            this.cssTypeHelper = cssTypeHelper;
        }

        public CssFileProvider(IEnumerable<Assembly> assemblies,
            CssTypeHelper<BindableObject, BindableObject, BindableProperty, Style> typeHelper)
            : this(typeHelper)
        {
            this.assemblies = assemblies.ToArray();
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

        protected override Stream TryGetFromEmbeddedResource(string source, params Assembly[] assemblies)
        {
            return base.TryGetFromEmbeddedResource(source, assemblies.Concat(this.assemblies).Distinct().ToArray());
        }

        protected override Stream TryLoadFromStaticApplicationResource(string source)
        {
            string stringValue = null;
            object value = null;

            Application.Current.Resources?.TryGetValue(source, out value);

            if (value is StyleSheet)
            {
                stringValue = (value as StyleSheet).Content;
            }
            else if (value is string)
            {
                stringValue = (string)value;
            }

            if (stringValue != null)
            {
                return new MemoryStream(Encoding.UTF8.GetBytes(stringValue));
            }

            return null;
        }
    }
}

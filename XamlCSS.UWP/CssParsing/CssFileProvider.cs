using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Windows.Storage;
using Windows.UI.Xaml;
using XamlCSS.CssParsing;

namespace XamlCSS.UWP.CssParsing
{
    public class CssFileProvider : CssFileProviderBase
    {
        private readonly CssTypeHelper<DependencyObject, DependencyObject, DependencyProperty, Style> cssTypeHelper;

        public CssFileProvider(CssTypeHelper<DependencyObject, DependencyObject, DependencyProperty, Style> cssTypeHelper)
            : base(new[] { Application.Current.GetType().GetTypeInfo().Assembly })
        {
            this.cssTypeHelper = cssTypeHelper;
        }

        public CssFileProvider(IEnumerable<Assembly> assemblies,
            CssTypeHelper<DependencyObject, DependencyObject, DependencyProperty, Style> typeHelper)
            : this(typeHelper)
        {
            this.assemblies = assemblies.ToArray();
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

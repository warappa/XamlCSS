using System;
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
        private readonly CssTypeHelper<BindableObject, BindableProperty, Style> cssTypeHelper;

        public CssFileProvider(CssTypeHelper<BindableObject, BindableProperty, Style> cssTypeHelper)
            : base(new[] { Application.Current.GetType().GetTypeInfo().Assembly })
        {
            this.cssTypeHelper = cssTypeHelper;
        }

        public CssFileProvider(IEnumerable<Assembly> assemblies,
            CssTypeHelper<BindableObject, BindableProperty, Style> typeHelper)
            : this(typeHelper)
        {
            this.assemblies = assemblies.ToArray();
        }

        protected override Stream TryGetFromFile(string source)
        {
            //StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            var currentFolder = "."; // Environment.CurrentDirectory;

            var absolutePath = source;
            if (!Path.IsPathRooted(absolutePath))
            {
                absolutePath = Path.Combine(currentFolder, absolutePath);
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

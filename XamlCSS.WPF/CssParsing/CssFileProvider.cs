using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using XamlCSS.CssParsing;

namespace XamlCSS.WPF.CssParsing
{
    public class CssFileProvider
         : CssFileProviderBase
    {
        private readonly CssTypeHelper<DependencyObject, DependencyObject, DependencyProperty, Style> cssTypeHelper;

        public CssFileProvider(CssTypeHelper<DependencyObject, DependencyObject, DependencyProperty, Style> cssTypeHelper)
            : base(AppDomain.CurrentDomain.GetAssemblies()
                 .Where(x =>
                    x.IsDynamic == false &&
                    x.FullName.StartsWith("System.", StringComparison.Ordinal) == false &&
                    x.FullName.StartsWith("Microsoft.", StringComparison.Ordinal) == false &&
                    x.FullName.StartsWith("Presentation.", StringComparison.Ordinal) == false)
                 .Distinct()
                 .ToArray())
        {
            this.cssTypeHelper = cssTypeHelper;
        }

        public override string LoadFrom(string source)
        {
            var content = base.LoadFrom(source);
            if (content == null)
            {
                var stream = TryGetFromPackedResource(source, assemblies);
                if (stream != null)
                {
                    return ReadStream(stream);
                }
            }

            return content;
        }

        protected Stream TryGetFromPackedResource(string source, params Assembly[] searchAssemblies)
        {
            source = source.Replace("\\", "/");
            
            if (!source.StartsWith("pack://"))
            {
                return null;
            }

            if (!Uri.IsWellFormedUriString(source, UriKind.Absolute))
            {
                return null;
            }

            try
            {
                return Application.GetResourceStream(new Uri(source, UriKind.Absolute))?.Stream;
            }
            catch { }

            return null;
        }

        protected override Stream TryGetFromFile(string source)
        {
            var absolutePath = source;
            if (!Path.IsPathRooted(absolutePath))
            {
                absolutePath = Path.Combine(Environment.CurrentDirectory, absolutePath);
            }

            if (File.Exists(absolutePath))
            {
                return File.OpenRead(source);
            }

            return null;
        }

        protected override Stream TryLoadFromStaticApplicationResource(string source)
        {
            string stringValue = null;
            if (Application.Current.Resources?.Contains(source) == true)
            {
                try
                {
                    var value = Application.Current.Resources[source];
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
                }
                catch
                {

                }
            }

            return null;
        }
    }
}

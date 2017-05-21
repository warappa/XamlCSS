using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using XamlCSS.CssParsing;

namespace XamlCSS.WPF.CssParsing
{
    public class CssFileProvider : CssFileProviderBase
    {
        public CssFileProvider()
            : base(AppDomain.CurrentDomain.GetAssemblies()
                 .Where(x =>
                    x.IsDynamic == false &&
                    x.FullName.StartsWith("System.", StringComparison.Ordinal) == false &&
                    x.FullName.StartsWith("Microsoft.", StringComparison.Ordinal) == false &&
                    x.FullName.StartsWith("Presentation.", StringComparison.Ordinal) == false)
                 .Distinct()
                 .ToArray())
        {

        }

        protected override Stream TryGetFromResource(string source, params Assembly[] searchAssemblies)
        {
            var stream = base.TryGetFromResource(source, searchAssemblies);
            if (stream == null)
            {
                var resource = source.Replace("\\", "/");
                foreach (var assembly in searchAssemblies)
                {
                    var uri = $"pack://application:,,,/{assembly.GetName().Name};component/" + resource;
                    try
                    {
                        stream = Application.GetResourceStream(new Uri(uri)).Stream;
                    }
                    catch { }
                    if (stream != null)
                    {
                        break;
                    }
                }
            }

            return stream;
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
    }
}

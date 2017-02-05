using System.IO;
using XamlCSS.CssParsing;

namespace XamlCSS.WPF.CssParsing
{
    public class CssFileProvider : ICssFileProvider
    {
        public string LoadFrom(string source)
        {
            return File.ReadAllText(source);
        }
    }
}

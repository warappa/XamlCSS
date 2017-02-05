using XamlCSS.CssParsing;
using System.IO;

namespace XamlCSS.Tests.CssParsing
{
    public class TestCssContentProvider : ICssContentProvider
    {
        public string LoadFrom(string source)
        {
            return File.ReadAllText(source);
        }
    }
}

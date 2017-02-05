using System.IO;
using XamlCSS.CssParsing;

namespace XamlCSS.XamarinForms.CssParsing
{
    public class CssFileProvider : ICssFileProvider
    {
        public string LoadFrom(string source)
        {
            using (var stream = PCLStorage.FileSystem.Current.GetFileFromPathAsync(source).Result.OpenAsync(PCLStorage.FileAccess.Read).Result)
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}

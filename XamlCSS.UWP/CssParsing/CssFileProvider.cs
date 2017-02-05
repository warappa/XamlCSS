using System;
using Windows.Storage;
using XamlCSS.CssParsing;

namespace XamlCSS.UWP.CssParsing
{
    public class CssFileProvider : ICssFileProvider
    {
        public string LoadFrom(string source)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = storageFolder.GetFileAsync(source).AsTask().Result;

            string text = FileIO.ReadTextAsync(sampleFile).AsTask().Result;

            return text;
        }
    }
}

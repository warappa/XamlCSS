using System;
using System.IO;

namespace XamlCSS.CssParsing
{
    public interface ICssFileProvider
    {
        string LoadFrom(string source);
    }
}

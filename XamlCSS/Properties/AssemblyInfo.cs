using System.Resources;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET451 || NET461
using System.Windows.Markup;
#endif

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: InternalsVisibleTo("XamlCSS.Tests")]
[assembly: InternalsVisibleTo("XamlCSS.UWP")]
[assembly: InternalsVisibleTo("XamlCSS.WPF")]
[assembly: InternalsVisibleTo("XamlCSS.XamarinForms")]

#if NET451 || NET461
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "XamlCSS")]
#endif

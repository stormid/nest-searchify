using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Nest.Searchify")]
[assembly: AssemblyDescription("An easy to use search interface to Elasticsearch using Nest-Queryify")]
[assembly: AssemblyCompany("Storm ID Ltd")]

#if DEBUG
    [assembly: AssemblyConfiguration("Debug")]
#else
    [assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyProduct("Nest.Searchify")]

[assembly: ComVisible(false)]

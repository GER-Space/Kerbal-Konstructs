#define CIBUILD_disabled
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("KerbalKonstructs")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("KerbalKonstructs")]
[assembly: AssemblyCopyright("Copyright © Matt \"medsouz\" Souza, Ashley \"AlphaAsh\" Hall, Christian \"GER-Space\" Bronk, KSP-RO team 2021")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(true)]

// Require CustomPrelaunchChecks
[assembly: KSPAssemblyDependency("CustomPreLaunchChecks", 1, 0)]


// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("c868c344-eb8b-4318-a3da-0462f49d6261")]

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
[assembly: AssemblyVersion("2.0.0.0")]
#if CIBUILD
[assembly: AssemblyFileVersion("@MAJOR@.@MINOR@.@PATCH@.@BUILD@")]
[assembly: AssemblyInformationalVersion("@MAJOR@.@MINOR@.@PATCH@.@BUILD@")]
#else
[assembly: AssemblyFileVersion("1.12.0.0")]
[assembly: AssemblyInformationalVersion("1.12.0.0")]
#endif

[assembly: KSPAssembly("KerbalKonstructs", 1, 12, 0)]


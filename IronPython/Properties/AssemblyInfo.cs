using System;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using IronPython.Modules;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;

[assembly: NeutralResourcesLanguage("en-US")]
[assembly: SecurityTransparent]
[assembly: CLSCompliant(false)]
[assembly: Guid("68e40495-c34a-4539-b43e-9e4e6f11a9fb")]
[assembly: AssemblyInformationalVersion("IronPython 2.7.3 final 0")]
[assembly: SecurityRules(SecurityRuleSet.Level1)]
[assembly: PythonModule("clr", typeof(ClrModule))]
[assembly: PythonModule("future_builtins", typeof(FutureBuiltins))]
[assembly: PythonModule("imp", typeof(PythonImport))]
[assembly: PythonModule("_ast", typeof(_ast))]
[assembly: PythonModule("unicodedata", typeof(unicodedata))]
[assembly: AssemblyFileVersion("2.7.3.1000")]
[assembly: InternalsVisibleTo("IronPythonTest, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c10ce00dd2e0ce5046d68183d3ad035b47e92bf0ce7bcf8a03a217ca5d0b0c7db973fdf97579b52b502a23d4069dbf043389e1ab65a1d6c508a9837f3e2350f15e05cc63c0fc4b0410867a51919090e4c33f80203e9b0035b21c32bae20f98b068f90d99a50133a5336480d94039b176519f5fd8524765f33be43da65c4b68ba")]
[assembly: ComVisible(false)]
[assembly: AssemblyCopyright("© IronPython Contributors.")]
[assembly: PythonModule("sys", typeof(SysModule))]
[assembly: PythonModule("__builtin__", typeof(Builtin))]
[assembly: PythonModule("exceptions", typeof(PythonExceptions))]
[assembly: InternalsVisibleTo("IronPython.Modules, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c10ce00dd2e0ce5046d68183d3ad035b47e92bf0ce7bcf8a03a217ca5d0b0c7db973fdf97579b52b502a23d4069dbf043389e1ab65a1d6c508a9837f3e2350f15e05cc63c0fc4b0410867a51919090e4c33f80203e9b0035b21c32bae20f98b068f90d99a50133a5336480d94039b176519f5fd8524765f33be43da65c4b68ba")]
[assembly: AssemblyCompany("IronPython Team")]
[assembly: AllowPartiallyTrustedCallers]
[assembly: AssemblyTitle("IronPython")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyProduct("IronPython")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyVersion("2.7.0.40")]

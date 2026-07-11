using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;

[assembly: AssemblyProduct("SharpDX.Direct2D1")]
[assembly: AssemblyDescription("Assembly providing DirectX - Direct2D, DirectWrite and WIC managed API")]
[assembly: AssemblyTitle("SharpDX.Direct2D1")]
[assembly: Obfuscation(Feature = "Apply to type SharpDX.* when enum: forced rename", Exclude = false)]
[assembly: Obfuscation(Feature = "Apply to type SharpDX.* when enum: enum values pruning", Exclude = false)]
[assembly: Obfuscation(Feature = "legacy xml serialization heuristics", Exclude = true)]
[assembly: Obfuscation(Feature = "ignore InternalsVisibleToAttribute", Exclude = false)]
[assembly: ComVisible(false)]
[assembly: AssemblyConfiguration("Release")]
[assembly: Obfuscation(Feature = "Apply to type SharpDX.* when public and interface: renaming", Exclude = false, ApplyToMembers = true)]
[assembly: Obfuscation(Feature = "Apply to type SharpDX.*: INotifyPropertyChanged heuristics", Exclude = true)]
[assembly: Obfuscation(Feature = "Apply to type SharpDX.* when struct: renaming", Exclude = false, ApplyToMembers = true)]
[assembly: AssemblyCopyright("Copyright © 2010-2013 Alexandre Mutel")]
[assembly: AssemblyCompany("Alexandre Mutel")]
[assembly: AssemblyTrademark("")]
[assembly: NeutralResourcesLanguage("en-us")]
[assembly: AssemblyFileVersion("2.5.1")]
[assembly: AssemblyVersion("2.5.1.0")]

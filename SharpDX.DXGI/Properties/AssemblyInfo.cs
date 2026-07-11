using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;

[assembly: InternalsVisibleTo("SharpDX.Direct3D10")]
[assembly: AssemblyDescription("Assembly providing DirectX - DXGI 1.0, 1.1 and 1.2 managed API")]
[assembly: InternalsVisibleTo("SharpDX.Direct3D11")]
[assembly: AssemblyCopyright("Copyright © 2010-2013 Alexandre Mutel")]
[assembly: AssemblyCompany("Alexandre Mutel")]
[assembly: AssemblyProduct("SharpDX.DXGI")]
[assembly: AssemblyTitle("SharpDX.DXGI")]
[assembly: AssemblyFileVersion("2.5.1")]
[assembly: Obfuscation(Feature = "Apply to type SharpDX.*: INotifyPropertyChanged heuristics", Exclude = true)]
[assembly: NeutralResourcesLanguage("en-us")]
[assembly: Obfuscation(Feature = "Apply to type SharpDX.* when enum: forced rename", Exclude = false)]
[assembly: Obfuscation(Feature = "legacy xml serialization heuristics", Exclude = true)]
[assembly: Obfuscation(Feature = "Apply to type SharpDX.* when enum: enum values pruning", Exclude = false)]
[assembly: Obfuscation(Feature = "Apply to type SharpDX.* when public and interface: renaming", Exclude = false, ApplyToMembers = true)]
[assembly: Obfuscation(Feature = "Apply to type SharpDX.* when struct: renaming", Exclude = false, ApplyToMembers = true)]
[assembly: AssemblyTrademark("")]
[assembly: Obfuscation(Feature = "ignore InternalsVisibleToAttribute", Exclude = false)]
[assembly: ComVisible(false)]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyVersion("2.5.1.0")]

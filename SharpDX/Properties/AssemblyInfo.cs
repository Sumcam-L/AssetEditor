using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;

[assembly: InternalsVisibleTo("SharpDX.Direct3D9")]
[assembly: InternalsVisibleTo("SharpDX.Direct3D10")]
[assembly: InternalsVisibleTo("SharpDX.D3DCompiler")]
[assembly: InternalsVisibleTo("SharpDX.Animation")]
[assembly: InternalsVisibleTo("SharpDX.DXGI")]
[assembly: InternalsVisibleTo("SharpDX.DirectInput")]
[assembly: InternalsVisibleTo("SharpDX.RawInput")]
[assembly: InternalsVisibleTo("SharpDX.Direct2D1")]
[assembly: InternalsVisibleTo("SharpDX.Direct3D11")]
[assembly: InternalsVisibleTo("SharpDX.Direct3D11.Effects")]
[assembly: AssemblyDescription("Core assembly for all SharpDX assemblies.")]
[assembly: InternalsVisibleTo("SharpDX.XAPO")]
[assembly: Obfuscation(Feature = "legacy xml serialization heuristics", Exclude = true)]
[assembly: Obfuscation(Feature = "ignore InternalsVisibleToAttribute", Exclude = false)]
[assembly: AssemblyProduct("SharpDX")]
[assembly: AssemblyTitle("SharpDX")]
[assembly: Obfuscation(Feature = "Apply to type SharpDX.* when enum: enum values pruning", Exclude = false)]
[assembly: InternalsVisibleTo("SharpDX.WIC")]
[assembly: NeutralResourcesLanguage("en-us")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("2.5.1")]
[assembly: AssemblyCopyright("Copyright © 2010-2013 Alexandre Mutel")]
[assembly: AssemblyTrademark("")]
[assembly: Obfuscation(Feature = "Apply to type SharpDX.*: INotifyPropertyChanged heuristics", Exclude = true)]
[assembly: Obfuscation(Feature = "Apply to type SharpDX.* when enum: forced rename", Exclude = false)]
[assembly: Obfuscation(Feature = "Apply to type SharpDX.* when struct: renaming", Exclude = false, ApplyToMembers = true)]
[assembly: ComVisible(false)]
[assembly: Obfuscation(Feature = "Apply to type SharpDX.* when public and interface: renaming", Exclude = false, ApplyToMembers = true)]
[assembly: AssemblyCompany("Alexandre Mutel")]
[assembly: InternalsVisibleTo("SharpDX.MediaFoundation")]
[assembly: InternalsVisibleTo("SharpDX.XACT3")]
[assembly: InternalsVisibleTo("SharpDX.DirectSound")]
[assembly: InternalsVisibleTo("SharpDX.XAudio2")]
[assembly: InternalsVisibleTo("SharpDX.Toolkit.Game")]
[assembly: InternalsVisibleTo("SharpDX.Toolkit")]
[assembly: InternalsVisibleTo("SharpDX.Toolkit.Graphics")]
[assembly: InternalsVisibleTo("SharpDX.DirectComposition")]
[assembly: InternalsVisibleTo("SharpDX.DirectManipulation")]
[assembly: AssemblyVersion("2.5.1.0")]

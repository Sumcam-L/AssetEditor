using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Firaxis.CivTech.AssetPreviewer;

[StructLayout(LayoutKind.Sequential, Size = 256)]
[NativeCppClass]
internal struct KnobDatabase
{
	[StructLayout(LayoutKind.Sequential, Size = 24)]
	[NativeCppClass]
	internal struct KnobRecord
	{
		private long _003Calignment_0020member_003E;
	}

	[StructLayout(LayoutKind.Sequential, Size = 40)]
	[NativeCppClass]
	internal struct KnobGroup
	{
		private long _003Calignment_0020member_003E;
	}

	private long _003Calignment_0020member_003E;
}

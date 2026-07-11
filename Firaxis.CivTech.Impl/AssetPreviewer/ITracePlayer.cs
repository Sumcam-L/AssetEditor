using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AssetPreviewer;

[StructLayout(LayoutKind.Sequential, Size = 8)]
[NativeCppClass]
internal static struct ITracePlayer
{
	[CLSCompliant(false)]
	[NativeCppClass]
	public enum TimingMode
	{

	}

	private long _003Calignment_0020member_003E;
}

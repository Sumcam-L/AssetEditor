using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Platform;

[StructLayout(LayoutKind.Sequential, Size = 128)]
[NativeCppClass]
internal struct TempHeap
{
	[CLSCompliant(false)]
	
	public enum eFlags
	{

	}

	[StructLayout(LayoutKind.Sequential, Size = 8)]
	[CLSCompliant(false)]
	[NativeCppClass]
	public struct ScopedAllocation
	{
		private long _003Calignment_0020member_003E;
	}

	private long _003Calignment_0020member_003E;
}

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Platform;

[StructLayout(LayoutKind.Sequential, Size = 64)]
[NativeCppClass]
internal struct ReadWriteLock
{
	[StructLayout(LayoutKind.Sequential, Size = 16)]
	[CLSCompliant(false)]
	[NativeCppClass]
	public struct WriteLock_Single
	{
		private long _003Calignment_0020member_003E;
	}

	[StructLayout(LayoutKind.Sequential, Size = 8)]
	[CLSCompliant(false)]
	[NativeCppClass]
	public struct ReadLock_Infinite_003C400_003E
	{
		private long _003Calignment_0020member_003E;
	}

	[StructLayout(LayoutKind.Sequential, Size = 8)]
	[NativeCppClass]
	[CLSCompliant(false)]
	public struct WriteLock_Infinite_003C400_003E
	{
		private long _003Calignment_0020member_003E;
	}

	private long _003Calignment_0020member_003E;
}

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Serialization;

[StructLayout(LayoutKind.Sequential, Size = 8)]
[NativeCppClass]
internal static struct IStream
{
	[StructLayout(LayoutKind.Sequential, Size = 40)]
	[NativeCppClass]
	[CLSCompliant(false)]
	public struct ReadRequest
	{
		private long _003Calignment_0020member_003E;
	}

	private long _003Calignment_0020member_003E;
}

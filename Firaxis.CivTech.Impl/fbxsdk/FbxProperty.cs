using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace fbxsdk;

[StructLayout(LayoutKind.Sequential, Size = 16)]
[NativeCppClass]
internal struct FbxProperty
{
	[StructLayout(LayoutKind.Sequential, Size = 8)]
	[CLSCompliant(false)]
	[NativeCppClass]
	public struct FbxPropertyNameCache
	{
		private long _003Calignment_0020member_003E;
	}

	private long _003Calignment_0020member_003E;
}

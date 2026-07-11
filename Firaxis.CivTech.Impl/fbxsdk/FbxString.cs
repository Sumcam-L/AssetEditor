using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace fbxsdk;

[StructLayout(LayoutKind.Sequential, Size = 8)]
[NativeCppClass]
internal struct FbxString
{
	[CLSCompliant(false)]
	[NativeCppClass]
	public enum EPaddingType
	{

	}

	private long _003Calignment_0020member_003E;
}

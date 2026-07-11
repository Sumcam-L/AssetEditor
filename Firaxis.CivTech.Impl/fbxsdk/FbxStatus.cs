using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace fbxsdk;

[StructLayout(LayoutKind.Sequential, Size = 16)]
[NativeCppClass]
internal struct FbxStatus
{
	[NativeCppClass]
	[CLSCompliant(false)]
	public enum EStatusCode
	{

	}

	private long _003Calignment_0020member_003E;
}

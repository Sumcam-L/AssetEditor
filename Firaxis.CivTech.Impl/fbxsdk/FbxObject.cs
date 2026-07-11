using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace fbxsdk;

[StructLayout(LayoutKind.Sequential, Size = 120)]
[NativeCppClass]
internal struct FbxObject
{
	[CLSCompliant(false)]
	[NativeCppClass]
	public enum EObjectFlag
	{

	}

	[NativeCppClass]
	[CLSCompliant(false)]
	public enum ECloneType
	{

	}

	private long _003Calignment_0020member_003E;
}

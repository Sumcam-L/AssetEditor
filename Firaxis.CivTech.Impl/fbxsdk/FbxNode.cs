using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace fbxsdk;

[StructLayout(LayoutKind.Sequential, Size = 1992)]
[NativeCppClass]
internal struct FbxNode
{
	[NativeCppClass]
	[CLSCompliant(false)]
	public enum EShadingMode
	{

	}

	[CLSCompliant(false)]
	[NativeCppClass]
	public enum EPivotSet
	{

	}

	[CLSCompliant(false)]
	[NativeCppClass]
	public enum EPivotState
	{

	}

	[StructLayout(LayoutKind.Sequential, Size = 88)]
	[NativeCppClass]
	[CLSCompliant(false)]
	public struct Pivot
	{
		private long _003Calignment_0020member_003E;
	}

	[StructLayout(LayoutKind.Sequential, Size = 32)]
	[CLSCompliant(false)]
	[NativeCppClass]
	public struct Pivots
	{
		private long _003Calignment_0020member_003E;
	}

	[StructLayout(LayoutKind.Sequential, Size = 24)]
	[CLSCompliant(false)]
	[NativeCppClass]
	public struct LinkToCharacter
	{
		private long _003Calignment_0020member_003E;
	}

	[NativeCppClass]
	[CLSCompliant(false)]
	public enum ECullingType
	{

	}

	private long _003Calignment_0020member_003E;
}

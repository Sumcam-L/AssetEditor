using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AssetObjects;

[StructLayout(LayoutKind.Sequential, Size = 248)]
[NativeCppClass]
internal struct EnumParameter
{
	[StructLayout(LayoutKind.Sequential, Size = 48)]
	[NativeCppClass]
	[CLSCompliant(false)]
	public struct Enumeration
	{
		private long _003Calignment_0020member_003E;
	}

	private long _003Calignment_0020member_003E;
}

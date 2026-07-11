using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DDT.AnimationGraph;

[StructLayout(LayoutKind.Sequential, Size = 48)]
[NativeCppClass]
internal struct Selector
{
	[StructLayout(LayoutKind.Sequential, Size = 8)]
	[CLSCompliant(false)]
	[NativeCppClass]
	public struct Item
	{
		private int _003Calignment_0020member_003E;
	}

	private long _003Calignment_0020member_003E;
}

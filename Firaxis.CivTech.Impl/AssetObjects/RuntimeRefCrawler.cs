using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AssetObjects;

[StructLayout(LayoutKind.Sequential, Size = 192)]
[NativeCppClass]
internal struct RuntimeRefCrawler
{
	[StructLayout(LayoutKind.Sequential, Size = 16)]
	[NativeCppClass]
	[CLSCompliant(false)]
	public struct Assignment
	{
		private long _003Calignment_0020member_003E;
	}

	[StructLayout(LayoutKind.Sequential, Size = 16)]
	[CLSCompliant(false)]
	[NativeCppClass]
	public struct DepKey
	{
		private long _003Calignment_0020member_003E;
	}

	[StructLayout(LayoutKind.Sequential, Size = 40)]
	[CLSCompliant(false)]
	[NativeCppClass]
	public struct Dependency
	{
		private long _003Calignment_0020member_003E;
	}

	private long _003Calignment_0020member_003E;
}

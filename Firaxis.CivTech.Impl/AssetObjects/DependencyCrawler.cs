using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AssetObjects;

[StructLayout(LayoutKind.Sequential, Size = 64)]
[NativeCppClass]
internal struct DependencyCrawler
{
	[StructLayout(LayoutKind.Sequential, Size = 16)]
	[NativeCppClass]
	[CLSCompliant(false)]
	public struct Dependency
	{
		private long _003Calignment_0020member_003E;
	}

	private long _003Calignment_0020member_003E;
}

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AppHost;

[StructLayout(LayoutKind.Sequential, Size = 144)]
[NativeCppClass]
internal struct BugSubmissionPackager
{
	[StructLayout(LayoutKind.Sequential, Size = 56)]
	[NativeCppClass]
	[CLSCompliant(false)]
	public struct ModuleInfo
	{
		private long _003Calignment_0020member_003E;
	}

	private long _003Calignment_0020member_003E;
}

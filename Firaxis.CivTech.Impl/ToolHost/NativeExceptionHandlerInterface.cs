using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ToolHost;

[StructLayout(LayoutKind.Sequential, Size = 8)]
[NativeCppClass]
internal static struct NativeExceptionHandlerInterface
{
	[CLSCompliant(false)]
	[NativeCppClass]
	public enum AssertionConfiguration
	{

	}

	private long _003Calignment_0020member_003E;
}

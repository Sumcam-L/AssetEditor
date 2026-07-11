using System;

namespace SharpDX.WIC;

[Flags]
public enum ProgressNotification
{
	Begin = 0x10000,
	End = 0x20000,
	Frequent = 0x40000,
	All = -65536
}

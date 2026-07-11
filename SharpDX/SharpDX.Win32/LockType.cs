using System;

namespace SharpDX.Win32;

[Flags]
public enum LockType
{
	Write = 1,
	Exclusive = 2,
	OnlyOnce = 4
}

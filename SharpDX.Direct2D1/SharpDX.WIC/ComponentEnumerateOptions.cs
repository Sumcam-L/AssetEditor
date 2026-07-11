using System;

namespace SharpDX.WIC;

[Flags]
public enum ComponentEnumerateOptions
{
	Default = 0,
	Refresh = 1,
	Disabled = int.MinValue,
	Unsigned = 0x40000000,
	BuiltInOnly = 0x20000000
}

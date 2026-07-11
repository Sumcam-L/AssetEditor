using System;

namespace SharpDX.Win32;

[Flags]
public enum VariantType : ushort
{
	Default = 0,
	Vector = 0x1000,
	Array = 0x2000,
	ByRef = 0x4000,
	Reserved = 0x8000
}

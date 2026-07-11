using System;

namespace SharpDX.WIC;

[Flags]
public enum ComponentSigning
{
	Signed = 1,
	Unsigned = 2,
	Safe = 4,
	Disabled = int.MinValue
}

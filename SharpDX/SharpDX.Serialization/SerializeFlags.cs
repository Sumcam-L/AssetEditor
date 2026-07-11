using System;

namespace SharpDX.Serialization;

[Flags]
public enum SerializeFlags
{
	Normal = 0,
	Dynamic = 1,
	Nullable = 2
}

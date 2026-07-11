namespace SharpDX.IO;

public enum NativeFileMode : uint
{
	CreateNew = 1u,
	Create,
	Open,
	OpenOrCreate,
	Truncate
}

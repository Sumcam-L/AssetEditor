using System;

namespace SharpDX.Win32;

[Flags]
public enum CommitFlags
{
	Default = 0,
	Overwrite = 1,
	OnlyCurrent = 2,
	DangerouslyCommitMerelyToDiskCache = 4,
	Consolidate = 8
}

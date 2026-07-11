using System;

namespace SharpDX.IO;

[Flags]
public enum NativeFileOptions : uint
{
	None = 0u,
	Readonly = 1u,
	Hidden = 2u,
	System = 4u,
	Directory = 0x10u,
	Archive = 0x20u,
	Device = 0x40u,
	Normal = 0x80u,
	Temporary = 0x100u,
	SparseFile = 0x200u,
	ReparsePoint = 0x400u,
	Compressed = 0x800u,
	Offline = 0x1000u,
	NotContentIndexed = 0x2000u,
	Encrypted = 0x4000u,
	Write_Through = 0x80000000u,
	Overlapped = 0x40000000u,
	NoBuffering = 0x20000000u,
	RandomAccess = 0x10000000u,
	SequentialScan = 0x8000000u,
	DeleteOnClose = 0x4000000u,
	BackupSemantics = 0x2000000u,
	PosixSemantics = 0x1000000u,
	OpenReparsePoint = 0x200000u,
	OpenNoRecall = 0x100000u,
	FirstPipeInstance = 0x80000u
}

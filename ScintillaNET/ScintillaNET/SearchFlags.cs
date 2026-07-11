using System;

namespace ScintillaNET;

[Flags]
public enum SearchFlags
{
	None = 0,
	MatchCase = 4,
	WholeWord = 2,
	WordStart = 0x100000,
	Regex = 0x200000,
	Posix = 0x400000,
	Cxx11Regex = 0x800000
}

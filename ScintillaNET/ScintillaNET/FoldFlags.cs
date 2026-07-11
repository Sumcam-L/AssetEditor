using System;

namespace ScintillaNET;

[Flags]
public enum FoldFlags
{
	LineBeforeExpanded = 2,
	LineBeforeContracted = 4,
	LineAfterExpanded = 8,
	LineAfterContracted = 0x10,
	LevelNumbers = 0x40,
	LineState = 0x80
}

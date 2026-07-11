using System;

namespace Sce.Atf.Applications;

[Flags]
public enum CommandVisibility
{
	None = 0,
	ApplicationMenu = 1,
	ContextMenu = 2,
	ApplicationToolbar = 4,
	ContextToolbar = 8,
	RightAlignment = 0x10,
	Menu = 3,
	Toolbar = 0xC,
	ControlDefault = 0xA,
	ControlDefaultRight = 0x1A,
	Default = 7,
	All = 0xF
}

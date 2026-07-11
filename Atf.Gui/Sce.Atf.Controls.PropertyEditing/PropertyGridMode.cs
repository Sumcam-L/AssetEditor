using System;

namespace Sce.Atf.Controls.PropertyEditing;

[Flags]
public enum PropertyGridMode
{
	PropertySorting = 1,
	AllowEditingComposites = 2,
	DisplayTooltips = 4,
	DisplayDescriptions = 8,
	DisableSearchControls = 0x10,
	ShowHideProperties = 0x20,
	DisableDragDropColumnHeaders = 0x40,
	HideResetAllButton = 0x80
}

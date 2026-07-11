using System;

namespace Sce.Atf.Controls.PropertyEditing;

[Flags]
public enum PropertySorting
{
	None = 0,
	Categorized = 1,
	Alphabetical = 2,
	CategoryAlphabetical = 4,
	Custom = 8,
	ByCategory = 7
}

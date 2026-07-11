using System;

namespace Sce.Atf.Controls.PropertyEditing;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CustomizeAttribute : Attribute
{
	public readonly int ColumnWidth;

	public readonly int HorizontalEditorOffset = -1;

	public readonly bool NameHasWholeRow;

	public readonly bool DisableSort;

	public readonly bool DisableDragging;

	public readonly bool DisableResize;

	public readonly bool DisableEditing;

	public readonly bool HideDisplayName;

	public readonly string PropertyName;

	public CustomizeAttribute(string propertyName, int columnWidth = 0, bool disableSort = false, bool disableDragging = false, bool disableResize = false, bool disableEditing = false, bool hideDisplayName = false, int horizontalEditorOffset = -1, bool nameHasWholeRow = false)
	{
		PropertyName = propertyName;
		ColumnWidth = columnWidth;
		DisableSort = disableSort;
		DisableDragging = disableDragging;
		DisableResize = disableResize;
		DisableEditing = disableEditing;
		HideDisplayName = hideDisplayName;
		HorizontalEditorOffset = horizontalEditorOffset;
		NameHasWholeRow = nameHasWholeRow;
	}
}

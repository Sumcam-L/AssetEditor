using System;

namespace Sce.Atf.Controls.PropertyEditing;

public class RowChangedEventArgs : EventArgs
{
	public readonly int RowIndex;

	public RowChangedEventArgs(int rowIndex)
	{
		RowIndex = rowIndex;
	}
}

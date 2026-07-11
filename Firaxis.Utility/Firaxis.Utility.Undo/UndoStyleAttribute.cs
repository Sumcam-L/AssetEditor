using System;

namespace Firaxis.Utility.Undo;

public class UndoStyleAttribute : Attribute
{
	public UndoStyle Style { get; private set; }

	public UndoStyleAttribute(UndoStyle style)
	{
		Style = style;
	}
}

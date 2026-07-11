using System;
using Firaxis.Reflection;

namespace Firaxis.Utility.Undo;

public class UndoEventArgs : EventArgs
{
	public UndoStyle Style { get; private set; }

	public IUndo Undo { get; private set; }

	public UndoEventArgs(IUndo undo)
	{
		Undo = undo;
		Style = ReflectionHelper.GetAttribute<UndoStyleAttribute>(undo)?.Style ?? UndoStyle.None;
	}
}

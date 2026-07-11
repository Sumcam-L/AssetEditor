using System;

namespace ScintillaNET;

public class IndicatorReleaseEventArgs : EventArgs
{
	private readonly Scintilla scintilla;

	private readonly int bytePosition;

	private int? position;

	public int Position
	{
		get
		{
			if (!position.HasValue)
			{
				position = scintilla.Lines.ByteToCharPosition(bytePosition);
			}
			return position.Value;
		}
	}

	public IndicatorReleaseEventArgs(Scintilla scintilla, int bytePosition)
	{
		this.scintilla = scintilla;
		this.bytePosition = bytePosition;
	}
}

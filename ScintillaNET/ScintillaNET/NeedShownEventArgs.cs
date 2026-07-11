using System;

namespace ScintillaNET;

public class NeedShownEventArgs : EventArgs
{
	private readonly Scintilla scintilla;

	private readonly int bytePosition;

	private readonly int byteLength;

	private int? position;

	private int? length;

	public int Length
	{
		get
		{
			if (!length.HasValue)
			{
				int pos = bytePosition + byteLength;
				int num = scintilla.Lines.ByteToCharPosition(pos);
				length = num - Position;
			}
			return length.Value;
		}
	}

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

	public NeedShownEventArgs(Scintilla scintilla, int bytePosition, int byteLength)
	{
		this.scintilla = scintilla;
		this.bytePosition = bytePosition;
		this.byteLength = byteLength;
	}
}

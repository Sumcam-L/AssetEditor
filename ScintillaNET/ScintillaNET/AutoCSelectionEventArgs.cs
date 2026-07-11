using System;

namespace ScintillaNET;

public class AutoCSelectionEventArgs : EventArgs
{
	private readonly Scintilla scintilla;

	private readonly IntPtr textPtr;

	private readonly int bytePosition;

	private int? position;

	private string text;

	public int Char { get; private set; }

	public ListCompletionMethod ListCompletionMethod { get; private set; }

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

	public unsafe string Text
	{
		get
		{
			if (text == null)
			{
				int i;
				for (i = 0; ((bool*)(void*)textPtr)[i]; i++)
				{
				}
				text = Helpers.GetString(textPtr, i, scintilla.Encoding);
			}
			return text;
		}
	}

	public AutoCSelectionEventArgs(Scintilla scintilla, int bytePosition, IntPtr text, int ch, ListCompletionMethod listCompletionMethod)
	{
		this.scintilla = scintilla;
		this.bytePosition = bytePosition;
		textPtr = text;
		Char = ch;
		ListCompletionMethod = listCompletionMethod;
	}
}

using System;

namespace ScintillaNET;

public class Selection
{
	private readonly Scintilla scintilla;

	public int Anchor
	{
		get
		{
			int num = scintilla.DirectMessage(2579, new IntPtr(Index)).ToInt32();
			if (num <= 0)
			{
				return num;
			}
			return scintilla.Lines.ByteToCharPosition(num);
		}
		set
		{
			value = Helpers.Clamp(value, 0, scintilla.TextLength);
			value = scintilla.Lines.CharToBytePosition(value);
			scintilla.DirectMessage(2578, new IntPtr(Index), new IntPtr(value));
		}
	}

	public int AnchorVirtualSpace
	{
		get
		{
			return scintilla.DirectMessage(2583, new IntPtr(Index)).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			scintilla.DirectMessage(2582, new IntPtr(Index), new IntPtr(value));
		}
	}

	public int Caret
	{
		get
		{
			int num = scintilla.DirectMessage(2577, new IntPtr(Index)).ToInt32();
			if (num <= 0)
			{
				return num;
			}
			return scintilla.Lines.ByteToCharPosition(num);
		}
		set
		{
			value = Helpers.Clamp(value, 0, scintilla.TextLength);
			value = scintilla.Lines.CharToBytePosition(value);
			scintilla.DirectMessage(2576, new IntPtr(Index), new IntPtr(value));
		}
	}

	public int CaretVirtualSpace
	{
		get
		{
			return scintilla.DirectMessage(2581, new IntPtr(Index)).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			scintilla.DirectMessage(2580, new IntPtr(Index), new IntPtr(value));
		}
	}

	public int End
	{
		get
		{
			int num = scintilla.DirectMessage(2587, new IntPtr(Index)).ToInt32();
			if (num <= 0)
			{
				return num;
			}
			return scintilla.Lines.ByteToCharPosition(num);
		}
		set
		{
			value = Helpers.Clamp(value, 0, scintilla.TextLength);
			value = scintilla.Lines.CharToBytePosition(value);
			scintilla.DirectMessage(2586, new IntPtr(Index), new IntPtr(value));
		}
	}

	public int Index { get; private set; }

	public int Start
	{
		get
		{
			int num = scintilla.DirectMessage(2585, new IntPtr(Index)).ToInt32();
			if (num <= 0)
			{
				return num;
			}
			return scintilla.Lines.ByteToCharPosition(num);
		}
		set
		{
			value = Helpers.Clamp(value, 0, scintilla.TextLength);
			value = scintilla.Lines.CharToBytePosition(value);
			scintilla.DirectMessage(2584, new IntPtr(Index), new IntPtr(value));
		}
	}

	public Selection(Scintilla scintilla, int index)
	{
		this.scintilla = scintilla;
		Index = index;
	}
}

using System;
using System.Drawing;

namespace ScintillaNET;

public class Margin
{
	private readonly Scintilla scintilla;

	public Color BackColor
	{
		get
		{
			int win32Color = scintilla.DirectMessage(2251, new IntPtr(Index)).ToInt32();
			return ColorTranslator.FromWin32(win32Color);
		}
		set
		{
			if (value.IsEmpty)
			{
				value = Color.Black;
			}
			int value2 = ColorTranslator.ToWin32(value);
			scintilla.DirectMessage(2250, new IntPtr(Index), new IntPtr(value2));
		}
	}

	public MarginCursor Cursor
	{
		get
		{
			return (MarginCursor)(int)scintilla.DirectMessage(2249, new IntPtr(Index));
		}
		set
		{
			scintilla.DirectMessage(2248, new IntPtr(Index), new IntPtr((int)value));
		}
	}

	public int Index { get; private set; }

	public bool Sensitive
	{
		get
		{
			return scintilla.DirectMessage(2247, new IntPtr(Index)) != IntPtr.Zero;
		}
		set
		{
			IntPtr lParam = (value ? new IntPtr(1) : IntPtr.Zero);
			scintilla.DirectMessage(2246, new IntPtr(Index), lParam);
		}
	}

	public MarginType Type
	{
		get
		{
			return (MarginType)(int)scintilla.DirectMessage(2241, new IntPtr(Index));
		}
		set
		{
			scintilla.DirectMessage(2240, new IntPtr(Index), new IntPtr((int)value));
		}
	}

	public int Width
	{
		get
		{
			return scintilla.DirectMessage(2243, new IntPtr(Index)).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			scintilla.DirectMessage(2242, new IntPtr(Index), new IntPtr(value));
		}
	}

	public uint Mask
	{
		get
		{
			return (uint)scintilla.DirectMessage(2245, new IntPtr(Index)).ToInt32();
		}
		set
		{
			scintilla.DirectMessage(2244, new IntPtr(Index), new IntPtr((int)value));
		}
	}

	public Margin(Scintilla scintilla, int index)
	{
		this.scintilla = scintilla;
		Index = index;
	}
}

using System;
using System.Drawing;

namespace ScintillaNET;

public class Indicator
{
	private readonly Scintilla scintilla;

	public const int ValueBit = 16777216;

	public const int ValueMask = 16777215;

	public int Alpha
	{
		get
		{
			return scintilla.DirectMessage(2524, new IntPtr(Index)).ToInt32();
		}
		set
		{
			value = Helpers.Clamp(value, 0, 255);
			scintilla.DirectMessage(2523, new IntPtr(Index), new IntPtr(value));
		}
	}

	public IndicatorFlags Flags
	{
		get
		{
			return (IndicatorFlags)(int)scintilla.DirectMessage(2685, new IntPtr(Index));
		}
		set
		{
			scintilla.DirectMessage(2684, new IntPtr(Index), new IntPtr((int)value));
		}
	}

	public Color ForeColor
	{
		get
		{
			int win32Color = scintilla.DirectMessage(2083, new IntPtr(Index)).ToInt32();
			return ColorTranslator.FromWin32(win32Color);
		}
		set
		{
			int value2 = ColorTranslator.ToWin32(value);
			scintilla.DirectMessage(2082, new IntPtr(Index), new IntPtr(value2));
		}
	}

	public Color HoverForeColor
	{
		get
		{
			int win32Color = scintilla.DirectMessage(2683, new IntPtr(Index)).ToInt32();
			return ColorTranslator.FromWin32(win32Color);
		}
		set
		{
			int value2 = ColorTranslator.ToWin32(value);
			scintilla.DirectMessage(2682, new IntPtr(Index), new IntPtr(value2));
		}
	}

	public IndicatorStyle HoverStyle
	{
		get
		{
			return (IndicatorStyle)(int)scintilla.DirectMessage(2681, new IntPtr(Index));
		}
		set
		{
			scintilla.DirectMessage(2680, new IntPtr(Index), new IntPtr((int)value));
		}
	}

	public int Index { get; private set; }

	public int OutlineAlpha
	{
		get
		{
			return scintilla.DirectMessage(2559, new IntPtr(Index)).ToInt32();
		}
		set
		{
			value = Helpers.Clamp(value, 0, 255);
			scintilla.DirectMessage(2558, new IntPtr(Index), new IntPtr(value));
		}
	}

	public IndicatorStyle Style
	{
		get
		{
			return (IndicatorStyle)(int)scintilla.DirectMessage(2081, new IntPtr(Index));
		}
		set
		{
			scintilla.DirectMessage(2080, new IntPtr(Index), new IntPtr((int)value));
		}
	}

	public bool Under
	{
		get
		{
			return scintilla.DirectMessage(2511, new IntPtr(Index)) != IntPtr.Zero;
		}
		set
		{
			IntPtr lParam = (value ? new IntPtr(1) : IntPtr.Zero);
			scintilla.DirectMessage(2510, new IntPtr(Index), lParam);
		}
	}

	public int End(int position)
	{
		position = Helpers.Clamp(position, 0, scintilla.TextLength);
		position = scintilla.Lines.CharToBytePosition(position);
		position = scintilla.DirectMessage(2509, new IntPtr(Index), new IntPtr(position)).ToInt32();
		return scintilla.Lines.ByteToCharPosition(position);
	}

	public int Start(int position)
	{
		position = Helpers.Clamp(position, 0, scintilla.TextLength);
		position = scintilla.Lines.CharToBytePosition(position);
		position = scintilla.DirectMessage(2508, new IntPtr(Index), new IntPtr(position)).ToInt32();
		return scintilla.Lines.ByteToCharPosition(position);
	}

	public int ValueAt(int position)
	{
		position = Helpers.Clamp(position, 0, scintilla.TextLength);
		position = scintilla.Lines.CharToBytePosition(position);
		return scintilla.DirectMessage(2507, new IntPtr(Index), new IntPtr(position)).ToInt32();
	}

	public Indicator(Scintilla scintilla, int index)
	{
		this.scintilla = scintilla;
		Index = index;
	}
}

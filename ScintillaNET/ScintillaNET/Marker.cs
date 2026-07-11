using System;
using System.Drawing;

namespace ScintillaNET;

public class Marker
{
	private readonly Scintilla scintilla;

	public const uint MaskAll = uint.MaxValue;

	public const uint MaskFolders = 4261412864u;

	public const int FolderEnd = 25;

	public const int FolderOpenMid = 26;

	public const int FolderMidTail = 27;

	public const int FolderTail = 28;

	public const int FolderSub = 29;

	public const int Folder = 30;

	public const int FolderOpen = 31;

	public int Index { get; private set; }

	public MarkerSymbol Symbol
	{
		get
		{
			return (MarkerSymbol)(int)scintilla.DirectMessage(2529, new IntPtr(Index));
		}
		set
		{
			scintilla.DirectMessage(2040, new IntPtr(Index), new IntPtr((int)value));
		}
	}

	public unsafe void DefineRgbaImage(Bitmap image)
	{
		if (image != null)
		{
			scintilla.DirectMessage(2624, new IntPtr(image.Width));
			scintilla.DirectMessage(2625, new IntPtr(image.Height));
			fixed (byte* value = Helpers.BitmapToArgb(image))
			{
				scintilla.DirectMessage(2626, new IntPtr(Index), new IntPtr(value));
			}
		}
	}

	public void DeleteAll()
	{
		scintilla.MarkerDeleteAll(Index);
	}

	public void SetAlpha(int alpha)
	{
		alpha = Helpers.Clamp(alpha, 0, 255);
		scintilla.DirectMessage(2476, new IntPtr(Index), new IntPtr(alpha));
	}

	public void SetBackColor(Color color)
	{
		int value = ColorTranslator.ToWin32(color);
		scintilla.DirectMessage(2042, new IntPtr(Index), new IntPtr(value));
	}

	public void SetForeColor(Color color)
	{
		int value = ColorTranslator.ToWin32(color);
		scintilla.DirectMessage(2041, new IntPtr(Index), new IntPtr(value));
	}

	public Marker(Scintilla scintilla, int index)
	{
		this.scintilla = scintilla;
		Index = index;
	}
}

using System;

namespace ScintillaNET;

public class Line
{
	private readonly Scintilla scintilla;

	public int AnnotationLines => scintilla.DirectMessage(2546, new IntPtr(Index)).ToInt32();

	public int AnnotationStyle
	{
		get
		{
			return scintilla.DirectMessage(2543, new IntPtr(Index)).ToInt32();
		}
		set
		{
			value = Helpers.Clamp(value, 0, scintilla.Styles.Count - 1);
			scintilla.DirectMessage(2542, new IntPtr(Index), new IntPtr(value));
		}
	}

	public unsafe byte[] AnnotationStyles
	{
		get
		{
			int num = scintilla.DirectMessage(2541, new IntPtr(Index)).ToInt32();
			if (num == 0)
			{
				return new byte[0];
			}
			byte[] array = new byte[num + 1];
			byte[] array2 = new byte[num + 1];
			fixed (byte* ptr = array)
			{
				fixed (byte* ptr2 = array2)
				{
					scintilla.DirectMessage(2541, new IntPtr(Index), new IntPtr(ptr));
					scintilla.DirectMessage(2545, new IntPtr(Index), new IntPtr(ptr2));
					return Helpers.ByteToCharStyles(ptr2, ptr, num, scintilla.Encoding);
				}
			}
		}
		set
		{
			int num = scintilla.DirectMessage(2541, new IntPtr(Index)).ToInt32();
			if (num == 0)
			{
				return;
			}
			fixed (byte* ptr = new byte[num + 1])
			{
				scintilla.DirectMessage(2541, new IntPtr(Index), new IntPtr(ptr));
				fixed (byte* value2 = Helpers.CharToByteStyles(value ?? new byte[0], ptr, num, scintilla.Encoding))
				{
					scintilla.DirectMessage(2544, new IntPtr(Index), new IntPtr(value2));
				}
			}
		}
	}

	public unsafe string AnnotationText
	{
		get
		{
			int num = scintilla.DirectMessage(2541, new IntPtr(Index)).ToInt32();
			if (num == 0)
			{
				return string.Empty;
			}
			fixed (byte* value = new byte[num + 1])
			{
				scintilla.DirectMessage(2541, new IntPtr(Index), new IntPtr(value));
				return Helpers.GetString(new IntPtr(value), num, scintilla.Encoding);
			}
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				scintilla.DirectMessage(2541, new IntPtr(Index), IntPtr.Zero);
				return;
			}
			fixed (byte* bytes = Helpers.GetBytes(value, scintilla.Encoding, zeroTerminated: true))
			{
				scintilla.DirectMessage(2540, new IntPtr(Index), new IntPtr(bytes));
			}
		}
	}

	public int ContractedFoldNext => scintilla.DirectMessage(2618, new IntPtr(Index)).ToInt32();

	public int DisplayIndex => scintilla.DirectMessage(2220, new IntPtr(Index)).ToInt32();

	public int EndPosition => Position + Length;

	public bool Expanded
	{
		get
		{
			return scintilla.DirectMessage(2230, new IntPtr(Index)) != IntPtr.Zero;
		}
		set
		{
			IntPtr lParam = (value ? new IntPtr(1) : IntPtr.Zero);
			scintilla.DirectMessage(2229, new IntPtr(Index), lParam);
		}
	}

	public int FoldLevel
	{
		get
		{
			int num = scintilla.DirectMessage(2223, new IntPtr(Index)).ToInt32();
			return num & 0xFFF;
		}
		set
		{
			int foldLevelFlags = (int)FoldLevelFlags;
			foldLevelFlags |= value;
			scintilla.DirectMessage(2222, new IntPtr(Index), new IntPtr(foldLevelFlags));
		}
	}

	public FoldLevelFlags FoldLevelFlags
	{
		get
		{
			int num = scintilla.DirectMessage(2223, new IntPtr(Index)).ToInt32();
			return (FoldLevelFlags)(num & -4096);
		}
		set
		{
			int foldLevel = FoldLevel;
			foldLevel |= (int)value;
			scintilla.DirectMessage(2222, new IntPtr(Index), new IntPtr(foldLevel));
		}
	}

	public int FoldParent => scintilla.DirectMessage(2225, new IntPtr(Index)).ToInt32();

	public int Height => scintilla.DirectMessage(2279, new IntPtr(Index)).ToInt32();

	public int Index { get; private set; }

	public int Length => scintilla.Lines.CharLineLength(Index);

	public int MarginStyle
	{
		get
		{
			return scintilla.DirectMessage(2533, new IntPtr(Index)).ToInt32();
		}
		set
		{
			value = Helpers.Clamp(value, 0, scintilla.Styles.Count - 1);
			scintilla.DirectMessage(2532, new IntPtr(Index), new IntPtr(value));
		}
	}

	public unsafe byte[] MarginStyles
	{
		get
		{
			int num = scintilla.DirectMessage(2531, new IntPtr(Index)).ToInt32();
			if (num == 0)
			{
				return new byte[0];
			}
			byte[] array = new byte[num + 1];
			byte[] array2 = new byte[num + 1];
			fixed (byte* ptr = array)
			{
				fixed (byte* ptr2 = array2)
				{
					scintilla.DirectMessage(2531, new IntPtr(Index), new IntPtr(ptr));
					scintilla.DirectMessage(2535, new IntPtr(Index), new IntPtr(ptr2));
					return Helpers.ByteToCharStyles(ptr2, ptr, num, scintilla.Encoding);
				}
			}
		}
		set
		{
			int num = scintilla.DirectMessage(2531, new IntPtr(Index)).ToInt32();
			if (num == 0)
			{
				return;
			}
			fixed (byte* ptr = new byte[num + 1])
			{
				scintilla.DirectMessage(2531, new IntPtr(Index), new IntPtr(ptr));
				fixed (byte* value2 = Helpers.CharToByteStyles(value ?? new byte[0], ptr, num, scintilla.Encoding))
				{
					scintilla.DirectMessage(2534, new IntPtr(Index), new IntPtr(value2));
				}
			}
		}
	}

	public unsafe string MarginText
	{
		get
		{
			int num = scintilla.DirectMessage(2531, new IntPtr(Index)).ToInt32();
			if (num == 0)
			{
				return string.Empty;
			}
			fixed (byte* value = new byte[num + 1])
			{
				scintilla.DirectMessage(2531, new IntPtr(Index), new IntPtr(value));
				return Helpers.GetString(new IntPtr(value), num, scintilla.Encoding);
			}
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				scintilla.DirectMessage(2530, new IntPtr(Index), IntPtr.Zero);
				return;
			}
			fixed (byte* bytes = Helpers.GetBytes(value, scintilla.Encoding, zeroTerminated: true))
			{
				scintilla.DirectMessage(2530, new IntPtr(Index), new IntPtr(bytes));
			}
		}
	}

	public int Position => scintilla.Lines.CharPositionFromLine(Index);

	public unsafe string Text
	{
		get
		{
			IntPtr wParam = scintilla.DirectMessage(2167, new IntPtr(Index));
			IntPtr lParam = scintilla.DirectMessage(2350, new IntPtr(Index));
			IntPtr intPtr = scintilla.DirectMessage(2643, wParam, lParam);
			if (intPtr == IntPtr.Zero)
			{
				return string.Empty;
			}
			return new string((sbyte*)(void*)intPtr, 0, lParam.ToInt32(), scintilla.Encoding);
		}
	}

	public int Indentation
	{
		get
		{
			return scintilla.DirectMessage(2127, new IntPtr(Index)).ToInt32();
		}
		set
		{
			scintilla.DirectMessage(2126, new IntPtr(Index), new IntPtr(value));
		}
	}

	public bool Visible => scintilla.DirectMessage(2228, new IntPtr(Index)) != IntPtr.Zero;

	public int WrapCount => scintilla.DirectMessage(2235, new IntPtr(Index)).ToInt32();

	public void EnsureVisible()
	{
		scintilla.DirectMessage(2232, new IntPtr(Index));
	}

	public void FoldChildren(FoldAction action)
	{
		scintilla.DirectMessage(2238, new IntPtr(Index), new IntPtr((int)action));
	}

	public void FoldLine(FoldAction action)
	{
		scintilla.DirectMessage(2237, new IntPtr(Index), new IntPtr((int)action));
	}

	public int GetLastChild(int level)
	{
		return scintilla.DirectMessage(2224, new IntPtr(Index), new IntPtr(level)).ToInt32();
	}

	public void Goto()
	{
		scintilla.DirectMessage(2024, new IntPtr(Index));
	}

	public MarkerHandle MarkerAdd(int marker)
	{
		marker = Helpers.Clamp(marker, 0, scintilla.Markers.Count - 1);
		IntPtr value = scintilla.DirectMessage(2043, new IntPtr(Index), new IntPtr(marker));
		return new MarkerHandle
		{
			Value = value
		};
	}

	public void MarkerAddSet(uint markerMask)
	{
		scintilla.DirectMessage(2466, new IntPtr(Index), new IntPtr((int)markerMask));
	}

	public void MarkerDelete(int marker)
	{
		marker = Helpers.Clamp(marker, -1, scintilla.Markers.Count - 1);
		scintilla.DirectMessage(2044, new IntPtr(Index), new IntPtr(marker));
	}

	public uint MarkerGet()
	{
		return (uint)scintilla.DirectMessage(2046, new IntPtr(Index)).ToInt32();
	}

	public int MarkerNext(uint markerMask)
	{
		return scintilla.DirectMessage(2047, new IntPtr(Index), new IntPtr((int)markerMask)).ToInt32();
	}

	public int MarkerPrevious(uint markerMask)
	{
		return scintilla.DirectMessage(2048, new IntPtr(Index), new IntPtr((int)markerMask)).ToInt32();
	}

	public void ToggleFold()
	{
		scintilla.DirectMessage(2231, new IntPtr(Index));
	}

	public unsafe void ToggleFoldShowText(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			scintilla.DirectMessage(2700, new IntPtr(Index), IntPtr.Zero);
			return;
		}
		fixed (byte* bytes = Helpers.GetBytes(text, scintilla.Encoding, zeroTerminated: true))
		{
			scintilla.DirectMessage(2700, new IntPtr(Index), new IntPtr(bytes));
		}
	}

	public Line(Scintilla scintilla, int index)
	{
		this.scintilla = scintilla;
		Index = index;
	}
}

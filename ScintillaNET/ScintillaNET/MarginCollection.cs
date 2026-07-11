using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace ScintillaNET;

public class MarginCollection : IEnumerable<Margin>, IEnumerable
{
	private readonly Scintilla scintilla;

	[DefaultValue(5)]
	[Description("The maximum number of margins.")]
	public int Capacity
	{
		get
		{
			return scintilla.DirectMessage(2253).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			scintilla.DirectMessage(2252, new IntPtr(value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int Count => Capacity;

	[DefaultValue(1)]
	[Description("The left margin padding in pixels.")]
	public int Left
	{
		get
		{
			return scintilla.DirectMessage(2156).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			scintilla.DirectMessage(2155, IntPtr.Zero, new IntPtr(value));
		}
	}

	[DefaultValue(1)]
	[Description("The right margin padding in pixels.")]
	public int Right
	{
		get
		{
			return scintilla.DirectMessage(2158).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			scintilla.DirectMessage(2157, IntPtr.Zero, new IntPtr(value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Margin this[int index]
	{
		get
		{
			index = Helpers.Clamp(index, 0, Count - 1);
			return new Margin(scintilla, index);
		}
	}

	public void ClearAllText()
	{
		scintilla.DirectMessage(2536);
	}

	public IEnumerator<Margin> GetEnumerator()
	{
		int count = Count;
		for (int i = 0; i < count; i++)
		{
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public MarginCollection(Scintilla scintilla)
	{
		this.scintilla = scintilla;
	}
}

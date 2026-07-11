using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace ScintillaNET;

public class IndicatorCollection : IEnumerable<Indicator>, IEnumerable
{
	private readonly Scintilla scintilla;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int Count => 32;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Indicator this[int index]
	{
		get
		{
			index = Helpers.Clamp(index, 0, Count - 1);
			return new Indicator(scintilla, index);
		}
	}

	public IEnumerator<Indicator> GetEnumerator()
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

	public IndicatorCollection(Scintilla scintilla)
	{
		this.scintilla = scintilla;
	}
}

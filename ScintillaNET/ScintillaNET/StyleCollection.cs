using System.Collections;
using System.Collections.Generic;

namespace ScintillaNET;

public class StyleCollection : IEnumerable<Style>, IEnumerable
{
	private readonly Scintilla scintilla;

	public int Count => 256;

	public Style this[int index]
	{
		get
		{
			index = Helpers.Clamp(index, 0, Count - 1);
			return new Style(scintilla, index);
		}
	}

	public IEnumerator<Style> GetEnumerator()
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

	public StyleCollection(Scintilla scintilla)
	{
		this.scintilla = scintilla;
	}
}

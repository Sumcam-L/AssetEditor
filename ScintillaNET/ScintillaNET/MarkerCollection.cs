using System.Collections;
using System.Collections.Generic;

namespace ScintillaNET;

public class MarkerCollection : IEnumerable<Marker>, IEnumerable
{
	private readonly Scintilla scintilla;

	public int Count => 32;

	public Marker this[int index]
	{
		get
		{
			index = Helpers.Clamp(index, 0, Count - 1);
			return new Marker(scintilla, index);
		}
	}

	public IEnumerator<Marker> GetEnumerator()
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

	public MarkerCollection(Scintilla scintilla)
	{
		this.scintilla = scintilla;
	}
}

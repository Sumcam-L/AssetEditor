using System;
using System.Collections;
using System.Collections.Generic;

namespace ScintillaNET;

public class SelectionCollection : IEnumerable<Selection>, IEnumerable
{
	private readonly Scintilla scintilla;

	public int Count => scintilla.DirectMessage(2570).ToInt32();

	public bool IsEmpty => scintilla.DirectMessage(2650) != IntPtr.Zero;

	public Selection this[int index]
	{
		get
		{
			index = Helpers.Clamp(index, 0, Count - 1);
			return new Selection(scintilla, index);
		}
	}

	public IEnumerator<Selection> GetEnumerator()
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

	public SelectionCollection(Scintilla scintilla)
	{
		this.scintilla = scintilla;
	}
}

using System;

namespace Sce.Atf;

public class LabelEditedEventArgs<T> : EventArgs
{
	public readonly T Item;

	public readonly string Label;

	public LabelEditedEventArgs(T item, string label)
	{
		Item = item;
		Label = label;
	}
}

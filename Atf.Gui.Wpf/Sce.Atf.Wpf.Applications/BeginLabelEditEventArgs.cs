using System;

namespace Sce.Atf.Wpf.Applications;

public class BeginLabelEditEventArgs : EventArgs
{
	public readonly object Item;

	public BeginLabelEditEventArgs(object item)
	{
		Item = item;
	}
}

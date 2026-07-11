using System;

namespace Firaxis.ATF;

public class PreviewContextChangedEventArgs : EventArgs
{
	public readonly IPreviewContext OldValue;

	public readonly IPreviewContext NewValue;

	public PreviewContextChangedEventArgs(IPreviewContext oldValue, IPreviewContext newValue)
	{
		OldValue = oldValue;
		NewValue = newValue;
	}
}

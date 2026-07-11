using System;

namespace Sce.Atf.Wpf.Docking;

public class ContentClosedEventArgs : EventArgs
{
	public ContentToClose ContentToClose { get; private set; }

	public ContentClosedEventArgs(ContentToClose contentToClose)
	{
		ContentToClose = contentToClose;
	}
}

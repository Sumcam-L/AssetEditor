using System;

namespace ScintillaNET;

public class UpdateUIEventArgs : EventArgs
{
	public UpdateChange Change { get; private set; }

	public UpdateUIEventArgs(UpdateChange change)
	{
		Change = change;
	}
}

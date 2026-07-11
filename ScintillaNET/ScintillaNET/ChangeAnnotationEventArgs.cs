using System;

namespace ScintillaNET;

public class ChangeAnnotationEventArgs : EventArgs
{
	public int Line { get; private set; }

	public ChangeAnnotationEventArgs(int line)
	{
		Line = line;
	}
}

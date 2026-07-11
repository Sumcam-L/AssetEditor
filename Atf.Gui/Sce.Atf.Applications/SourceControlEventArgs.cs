using System;

namespace Sce.Atf.Applications;

public class SourceControlEventArgs : EventArgs
{
	public readonly Uri Uri;

	public readonly SourceControlStatus Status;

	public SourceControlEventArgs(Uri uri, SourceControlStatus status)
	{
		Uri = uri;
		Status = status;
	}
}

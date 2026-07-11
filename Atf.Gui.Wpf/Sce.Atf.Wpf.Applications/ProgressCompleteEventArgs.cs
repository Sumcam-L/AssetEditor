using System;

namespace Sce.Atf.Wpf.Applications;

public class ProgressCompleteEventArgs : EventArgs
{
	public Exception ProgressError { get; set; }

	public object ProgressResult { get; set; }

	public bool Cancelled { get; set; }

	public ProgressCompleteEventArgs(Exception progressError, object progressResult, bool cancelled)
	{
		ProgressError = ProgressError;
		ProgressResult = progressResult;
		Cancelled = cancelled;
	}
}

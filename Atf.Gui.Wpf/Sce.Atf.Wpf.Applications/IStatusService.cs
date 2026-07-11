using System;
using System.ComponentModel;

namespace Sce.Atf.Wpf.Applications;

public interface IStatusService
{
	Exception ProgressError { get; }

	object ProgressResult { get; }

	void ShowStatus(string status);

	bool RunProgressDialog(string message, bool canCancel, object argument, DoWorkEventHandler workHandler, bool autoIncrement);

	void RunProgressInStatusBarAsync(string message, object argument, DoWorkEventHandler workHandler, EventHandler<ProgressCompleteEventArgs> progressCompleteHandler, bool autoIncrement);
}

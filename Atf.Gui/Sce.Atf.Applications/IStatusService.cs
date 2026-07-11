using System;

namespace Sce.Atf.Applications;

public interface IStatusService
{
	event EventHandler ProgressCancelled;

	void ShowStatus(string status);

	IStatusText AddText(int width);

	IStatusImage AddImage();

	void BeginProgress(string message);

	void BeginProgress(string message, int expectedDuration);

	void BeginProgress(string message, bool canCancel);

	void BeginProgress(string message, int expectedDuration, bool canCancel);

	void ShowProgress(double progress);

	void EndProgress();
}

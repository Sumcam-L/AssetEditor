using System;

namespace Sce.Atf.Applications;

public interface ISourceControlNotifier
{
	event EventHandler SourceControlReady;

	void Notify();
}

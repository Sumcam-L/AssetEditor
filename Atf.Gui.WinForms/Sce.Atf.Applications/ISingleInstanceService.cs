using System;

namespace Sce.Atf.Applications;

public interface ISingleInstanceService
{
	event EventHandler CommandLineChanged;

	void RestartApplication();
}

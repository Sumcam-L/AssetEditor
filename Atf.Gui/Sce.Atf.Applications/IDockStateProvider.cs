using System;

namespace Sce.Atf.Applications;

public interface IDockStateProvider
{
	object DockState { get; set; }

	event EventHandler DockStateChanged;

	void ResetDockState();
}

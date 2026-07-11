using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications;

public interface IWindowLayoutService
{
	string CurrentLayout { get; set; }

	bool ResetLayout { get; set; }

	bool LayoutChanging { get; }

	IEnumerable<string> Layouts { get; }

	event EventHandler<EventArgs> LayoutsChanging;

	event EventHandler<EventArgs> LayoutsChanged;

	bool RenameLayout(string oldLayoutName, string newLayoutName);

	bool RemoveLayout(string layoutName);
}

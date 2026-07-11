using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Sce.Atf.Applications;

public interface IControlHostService
{
	ThemeBase Theme { get; set; }

	IEnumerable<ControlInfo> Controls { get; }

	event EventHandler ControlVisibilityChanged;

	void RegisterControl(Control control, ControlInfo controlInfo, IControlHostClient client);

	void UnregisterControl(Control control);

	void Show(Control control);

	void Hide(Control control);
}

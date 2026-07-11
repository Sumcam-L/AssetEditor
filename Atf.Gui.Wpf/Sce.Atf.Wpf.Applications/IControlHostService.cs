using System.Collections.Generic;

namespace Sce.Atf.Wpf.Applications;

public interface IControlHostService
{
	IEnumerable<IControlInfo> Contents { get; }

	string DockPanelState { get; set; }

	IControlInfo RegisterControl(ControlDef definition, object control, IControlHostClient client);

	void UnregisterContent(object control);

	void Show(object control);
}

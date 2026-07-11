using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public static class ControlHostServices
{
	public static ControlInfo RegisterControl(this IControlHostService controlHostService, Control control, string name, string description, StandardControlGroup group, IControlHostClient client)
	{
		ControlInfo controlInfo = new ControlInfo(name, description, group);
		controlHostService.RegisterControl(control, controlInfo, client);
		return controlInfo;
	}

	public static ControlInfo RegisterControl(this IControlHostService controlHostService, Control control, string name, string description, StandardControlGroup group, Image image, IControlHostClient client, string helpUrl = null)
	{
		ControlInfo controlInfo = new ControlInfo(name, description, group, image, helpUrl);
		controlHostService.RegisterControl(control, controlInfo, client);
		return controlInfo;
	}
}

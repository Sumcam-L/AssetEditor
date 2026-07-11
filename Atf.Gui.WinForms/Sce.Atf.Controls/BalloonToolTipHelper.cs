using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

internal class BalloonToolTipHelper
{
	private Control m_control;

	public const int DefaultToolTipTimeoutMsec = 2000;

	public void Show(Control control, string title, ToolTipIcon icon, string format, params object[] args)
	{
		Show(control, title, icon, 2000, format, args);
	}

	public void Show(Control control, Point point, string title, ToolTipIcon icon, string format, params object[] args)
	{
		Show(control, point, title, icon, 2000, format, args);
	}

	public void Show(Control control, string title, ToolTipIcon icon, int timeoutMsec, string format, params object[] args)
	{
		Show(control, default(Point), title, icon, timeoutMsec, format, args);
	}

	public void Show(Control control, Point point, string title, ToolTipIcon icon, int timeoutMsec, string format, params object[] args)
	{
		m_control = control;
	}
}

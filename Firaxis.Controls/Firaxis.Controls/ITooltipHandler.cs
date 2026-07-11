using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.Controls;

public interface ITooltipHandler
{
	int GetTipID(Control control, Point point);

	bool GetTipInfo(Control control, int id, ref string text, ref Image image, ref string description);
}

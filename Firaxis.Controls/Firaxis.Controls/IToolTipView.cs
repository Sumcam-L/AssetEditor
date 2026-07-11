using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.Controls;

public interface IToolTipView
{
	Image Image { get; set; }

	string Caption { get; set; }

	string Description { get; set; }

	Point Position { get; set; }

	bool ShowImage { get; set; }

	ToolTipFadeMode FadeMode { get; }

	Size TipSize { get; }

	void ShowTip(Control parent, bool show);
}

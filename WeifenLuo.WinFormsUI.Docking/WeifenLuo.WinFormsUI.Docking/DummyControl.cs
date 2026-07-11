using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

internal sealed class DummyControl : Control
{
	public DummyControl()
	{
		SetStyle(ControlStyles.Selectable, value: false);
		ResetBackColor();
	}

	public override void ResetBackColor()
	{
		BackColor = SystemColors.ControlLight;
	}
}

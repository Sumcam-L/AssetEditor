using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class HoverBase : Form
{
	public HoverBase()
	{
		base.ShowInTaskbar = false;
		base.FormBorderStyle = FormBorderStyle.None;
		base.StartPosition = FormStartPosition.Manual;
		BackColor = SystemColors.Info;
		ForeColor = SystemColors.InfoText;
		SetStyle(ControlStyles.DoubleBuffer, value: true);
		SetStyle(ControlStyles.UserPaint, value: true);
		SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
	}

	public void ShowWithoutFocus()
	{
		User32.ShowWindow(base.Handle, 4);
	}

	protected override void OnClick(EventArgs e)
	{
		Dispose();
		Close();
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		Dispose();
		Close();
	}
}

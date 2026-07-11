using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace WeifenLuo.WinFormsUI.ThemeVS2012;

internal class VS2012AutoHideWindowControl : DockPanel.AutoHideWindowControl
{
	protected override Rectangle DisplayingRectangle
	{
		get
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (base.DockState == DockState.DockBottomAutoHide)
			{
				clientRectangle.Y += base.DockPanel.Theme.Measures.AutoHideSplitterSize;
				clientRectangle.Height -= base.DockPanel.Theme.Measures.AutoHideSplitterSize;
			}
			else if (base.DockState == DockState.DockRightAutoHide)
			{
				clientRectangle.X += base.DockPanel.Theme.Measures.AutoHideSplitterSize;
				clientRectangle.Width -= base.DockPanel.Theme.Measures.AutoHideSplitterSize;
			}
			else if (base.DockState == DockState.DockTopAutoHide)
			{
				clientRectangle.Height -= base.DockPanel.Theme.Measures.AutoHideSplitterSize;
			}
			else if (base.DockState == DockState.DockLeftAutoHide)
			{
				clientRectangle.Width -= base.DockPanel.Theme.Measures.AutoHideSplitterSize;
			}
			return clientRectangle;
		}
	}

	public VS2012AutoHideWindowControl(DockPanel dockPanel)
		: base(dockPanel)
	{
	}

	protected override void OnLayout(LayoutEventArgs levent)
	{
		base.DockPadding.All = 0;
		if (base.DockState == DockState.DockLeftAutoHide)
		{
			base.m_splitter.Dock = DockStyle.Right;
		}
		else if (base.DockState == DockState.DockRightAutoHide)
		{
			base.m_splitter.Dock = DockStyle.Left;
		}
		else if (base.DockState == DockState.DockTopAutoHide)
		{
			base.m_splitter.Dock = DockStyle.Bottom;
		}
		else if (base.DockState == DockState.DockBottomAutoHide)
		{
			base.m_splitter.Dock = DockStyle.Top;
		}
		Rectangle displayingRectangle = DisplayingRectangle;
		Rectangle bounds = new Rectangle(-displayingRectangle.Width, displayingRectangle.Y, displayingRectangle.Width, displayingRectangle.Height);
		foreach (Control control in base.Controls)
		{
			if (control is DockPane dockPane)
			{
				if (dockPane == base.ActivePane)
				{
					dockPane.Bounds = displayingRectangle;
				}
				else
				{
					dockPane.Bounds = bounds;
				}
			}
		}
		base.OnLayout(levent);
	}
}

using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace WeifenLuo.WinFormsUI.ThemeVS2013;

public class VS2013DockPane : DockPane
{
	protected override Rectangle ContentRectangle
	{
		get
		{
			Rectangle contentRectangle = base.ContentRectangle;
			if (base.DockState == DockState.Document || base.Contents.Count == 1)
			{
				contentRectangle.Height--;
			}
			contentRectangle.Width -= 2;
			contentRectangle.X++;
			return contentRectangle;
		}
	}

	public VS2013DockPane(IDockContent content, DockState visibleState, bool show)
		: base(content, visibleState, show)
	{
	}

	public VS2013DockPane(IDockContent content, FloatWindow floatWindow, bool show)
		: base(content, floatWindow, show)
	{
	}

	public VS2013DockPane(IDockContent content, DockPane previousPane, DockAlignment alignment, double proportion, bool show)
		: base(content, previousPane, alignment, proportion, show)
	{
	}

	public VS2013DockPane(IDockContent content, Rectangle floatWindowBounds, bool show)
		: base(content, floatWindowBounds, show)
	{
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Color toolWindowBorder = base.DockPanel.Theme.ColorPalette.ToolWindowBorder;
		e.Graphics.FillRectangle(base.DockPanel.Theme.PaintingService.GetBrush(toolWindowBorder), e.ClipRectangle);
	}
}

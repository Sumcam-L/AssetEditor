using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

public class FiraxisDockPane : DockPane
{
	public override string CaptionText
	{
		get
		{
			if (ActiveContent == null)
			{
				return string.Empty;
			}
			if (!(ActiveContent is DockContent dockContent))
			{
				return ActiveContent.DockHandler.TabText;
			}
			if (string.IsNullOrEmpty(dockContent.Text))
			{
				return ActiveContent.DockHandler.TabText;
			}
			return dockContent.Text;
		}
	}

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

	public FiraxisDockPane(IDockContent content, DockState visibleState, bool show)
		: base(content, visibleState, show)
	{
		ApplyThemeBackground();
	}

	public FiraxisDockPane(IDockContent content, FloatWindow floatWindow, bool show)
		: base(content, floatWindow, show)
	{
		ApplyThemeBackground();
	}

	public FiraxisDockPane(IDockContent content, DockPane previousPane, DockAlignment alignment, double proportion, bool show)
		: base(content, previousPane, alignment, proportion, show)
	{
		ApplyThemeBackground();
	}

	public FiraxisDockPane(IDockContent content, Rectangle floatWindowBounds, bool show)
		: base(content, floatWindowBounds, show)
	{
		ApplyThemeBackground();
	}

	private void ApplyThemeBackground()
	{
		BackColor = DockPanel.Theme.ColorPalette.MainWindowActive.Background;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Color toolWindowBorder = base.DockPanel.Theme.ColorPalette.ToolWindowBorder;
		using (Region paintRegion = new Region(e.ClipRectangle))
		{
			paintRegion.Exclude(ContentRectangle);
			e.Graphics.FillRegion(base.DockPanel.Theme.PaintingService.GetBrush(toolWindowBorder), paintRegion);
		}
	}
}

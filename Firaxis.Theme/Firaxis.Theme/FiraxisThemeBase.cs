using System.Drawing;
using System.Windows.Forms;
using Firaxis.CivTech;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

public abstract class FiraxisThemeBase : ThemeBase
{
	private class FiraxisVSLikeToolStripRenderer : VisualStudioToolStripRenderer
	{
		private readonly DockPanelColorPalette LocalPalette;

		public FiraxisVSLikeToolStripRenderer(DockPanelColorPalette palette)
			: base(palette)
		{
			LocalPalette = palette;
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			Color black = Color.Black;
			if (e.Item is ToolStripLabel)
			{
				black = ((!(CivTechRegistry.CivTechService.PrimaryProject.Name == e.Text)) ? (black = Color.FromArgb(200, LocalPalette.CommandBarMenuPopupDisabled.Text)) : (black = Color.FromArgb(220, LocalPalette.CommandBarMenuPopupDisabled.Text)));
				TextRenderer.DrawText(e.Graphics, e.Text, e.TextFont, e.TextRectangle, black, e.TextFormat);
			}
			else
			{
				base.OnRenderItemText(e);
			}
		}
	}

	private ToolStripRenderer FiraxisToolStripRenderer => base.ToolStripRenderer;

	public FiraxisThemeBase(byte[] resources)
	{
		base.ColorPalette = new DockPanelColorPalette(new FiraxisPaletteFactory(resources));
		base.Skin = new DockPanelSkin();
		base.PaintingService = new PaintingService();
		base.ImageService = new ImageService(this);
		base.ToolStripRenderer = new FiraxisVSLikeToolStripRenderer(base.ColorPalette)
		{
			UseGlassOnMenuStrip = false
		};
		base.Measures.SplitterSize = 6;
		base.Measures.AutoHideSplitterSize = 3;
		base.Measures.DockPadding = 6;
		base.ShowAutoHideContentOnHover = false;
		base.Extender.AutoHideStripFactory = new FiraxisAutoHideStripFactory();
		base.Extender.AutoHideWindowFactory = new FiraxisAutoHideWindowFactory();
		base.Extender.DockPaneFactory = new FiraxisDockPaneFactory();
		base.Extender.DockPaneCaptionFactory = new FiraxisDockPaneCaptionFactory();
		base.Extender.DockPaneStripFactory = new FiraxisDockPaneStripFactory();
		base.Extender.DockPaneSplitterControlFactory = new FiraxisDockPaneSplitterControlFactory();
		base.Extender.WindowSplitterControlFactory = new FiraxisWindowSplitterControlFactory();
		base.Extender.DockWindowFactory = new FiraxisDockWindowFactory();
		base.Extender.PaneIndicatorFactory = new FiraxisPaneIndicatorFactory();
		base.Extender.PanelIndicatorFactory = new FiraxisPanelIndicatorFactory();
		base.Extender.DockOutlineFactory = new FiraxisDockOutlineFactory();
		base.Extender.DockIndicatorFactory = new FiraxisDockIndicatorFactory();
	}

	public override void CleanUp(DockPanel dockPanel)
	{
		base.PaintingService.CleanUp();
		base.CleanUp(dockPanel);
	}
}

using WeifenLuo.WinFormsUI.Docking;
using WeifenLuo.WinFormsUI.ThemeVS2012;
using WeifenLuo.WinFormsUI.ThemeVS2013;

namespace WeifenLuo.WinFormsUI.ThemeVS2015;

public abstract class VS2015ThemeBase : ThemeBase
{
	public VS2015ThemeBase(byte[] resources)
	{
		base.ColorPalette = new DockPanelColorPalette(new VS2012PaletteFactory(resources));
		base.Skin = new DockPanelSkin();
		base.PaintingService = new PaintingService();
		base.ImageService = new ImageService(this);
		base.ToolStripRenderer = new VisualStudioToolStripRenderer(base.ColorPalette)
		{
			UseGlassOnMenuStrip = false
		};
		base.Measures.SplitterSize = 6;
		base.Measures.AutoHideSplitterSize = 3;
		base.Measures.DockPadding = 6;
		base.ShowAutoHideContentOnHover = false;
		base.Extender.AutoHideStripFactory = new VS2012AutoHideStripFactory();
		base.Extender.AutoHideWindowFactory = new VS2012AutoHideWindowFactory();
		base.Extender.DockPaneFactory = new VS2013DockPaneFactory();
		base.Extender.DockPaneCaptionFactory = new VS2013DockPaneCaptionFactory();
		base.Extender.DockPaneStripFactory = new VS2013DockPaneStripFactory();
		base.Extender.DockPaneSplitterControlFactory = new VS2013DockPaneSplitterControlFactory();
		base.Extender.WindowSplitterControlFactory = new VS2013WindowSplitterControlFactory();
		base.Extender.DockWindowFactory = new VS2012DockWindowFactory();
		base.Extender.PaneIndicatorFactory = new VS2012PaneIndicatorFactory();
		base.Extender.PanelIndicatorFactory = new VS2012PanelIndicatorFactory();
		base.Extender.DockOutlineFactory = new VS2012DockOutlineFactory();
		base.Extender.DockIndicatorFactory = new VS2012DockIndicatorFactory();
	}

	public override void CleanUp(DockPanel dockPanel)
	{
		base.PaintingService.CleanUp();
		base.CleanUp(dockPanel);
	}
}

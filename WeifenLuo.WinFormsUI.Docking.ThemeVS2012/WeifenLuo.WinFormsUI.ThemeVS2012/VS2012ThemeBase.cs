using WeifenLuo.WinFormsUI.Docking;

namespace WeifenLuo.WinFormsUI.ThemeVS2012;

public abstract class VS2012ThemeBase : ThemeBase
{
	public VS2012ThemeBase(byte[] resources, DockPanelExtender.IDockPaneSplitterControlFactory splitterFactory, DockPanelExtender.IWindowSplitterControlFactory windowsSplitterFactory)
	{
		base.ColorPalette = new DockPanelColorPalette(new VS2012PaletteFactory(resources));
		base.Skin = new DockPanelSkin();
		base.PaintingService = new PaintingService();
		base.ImageService = new ImageService(this);
		base.ToolStripRenderer = new VisualStudioToolStripRenderer(base.ColorPalette);
		base.Measures.SplitterSize = 6;
		base.Measures.AutoHideSplitterSize = 3;
		base.Measures.DockPadding = 6;
		base.ShowAutoHideContentOnHover = false;
		base.Extender.DockPaneCaptionFactory = new VS2012DockPaneCaptionFactory();
		base.Extender.AutoHideStripFactory = new VS2012AutoHideStripFactory();
		base.Extender.AutoHideWindowFactory = new VS2012AutoHideWindowFactory();
		base.Extender.DockPaneStripFactory = new VS2012DockPaneStripFactory();
		base.Extender.DockPaneSplitterControlFactory = splitterFactory ?? new VS2012DockPaneSplitterControlFactory();
		base.Extender.WindowSplitterControlFactory = windowsSplitterFactory ?? new VS2012WindowSplitterControlFactory();
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

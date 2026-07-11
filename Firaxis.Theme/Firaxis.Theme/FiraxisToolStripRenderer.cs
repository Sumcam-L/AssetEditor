using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

public class FiraxisToolStripRenderer : VisualStudioToolStripRenderer
{
	public FiraxisToolStripRenderer(FiraxisThemePalette palette)
		: base(new DockPanelColorPalette(new FiraxisAdapterPaletteFactory(palette)))
	{
		base.UseGlassOnMenuStrip = false;
	}

	public FiraxisToolStripRenderer(string resName)
		: base(new DockPanelColorPalette(new FiraxisPaletteFactory(ThemeBase.Decompress((byte[])Resources.ResourceManager.GetObject(resName)))))
	{
		base.UseGlassOnMenuStrip = false;
	}
}

using System.Drawing;
using WeifenLuo.WinFormsUI.ThemeVS2012;
using WeifenLuo.WinFormsUI.ThemeVS2013;

namespace WeifenLuo.WinFormsUI.Docking;

public class VS2012BlueTheme : VS2012ThemeBase
{
	public VS2012BlueTheme()
		: base(ThemeBase.Decompress(Resources.vs2012blue_vstheme), new VS2013DockPaneSplitterControlFactory(), new VS2013WindowSplitterControlFactory())
	{
		base.ColorPalette.TabSelectedInactive.Background = ColorTranslator.FromHtml("#FF3D5277");
	}
}

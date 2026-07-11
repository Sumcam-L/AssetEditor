using WeifenLuo.WinFormsUI.ThemeVS2013;

namespace WeifenLuo.WinFormsUI.Docking;

public class VS2013BlueTheme : VS2013ThemeBase
{
	public VS2013BlueTheme()
		: base(ThemeBase.Decompress(Resources.vs2013blue_vstheme))
	{
	}
}

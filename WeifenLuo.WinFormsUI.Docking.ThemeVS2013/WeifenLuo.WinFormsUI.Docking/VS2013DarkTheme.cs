using WeifenLuo.WinFormsUI.ThemeVS2013;

namespace WeifenLuo.WinFormsUI.Docking;

public class VS2013DarkTheme : VS2013ThemeBase
{
	public VS2013DarkTheme()
		: base(ThemeBase.Decompress(Resources.vs2013dark_vstheme))
	{
	}
}

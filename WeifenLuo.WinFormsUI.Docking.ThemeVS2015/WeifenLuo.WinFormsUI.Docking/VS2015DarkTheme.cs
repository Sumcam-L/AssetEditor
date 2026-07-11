using WeifenLuo.WinFormsUI.ThemeVS2015;

namespace WeifenLuo.WinFormsUI.Docking;

public class VS2015DarkTheme : VS2015ThemeBase
{
	public VS2015DarkTheme()
		: base(ThemeBase.Decompress(Resources.vs2015dark_vstheme))
	{
	}
}

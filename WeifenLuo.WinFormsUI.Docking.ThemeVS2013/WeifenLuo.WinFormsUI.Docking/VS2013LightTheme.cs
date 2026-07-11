using WeifenLuo.WinFormsUI.ThemeVS2013;

namespace WeifenLuo.WinFormsUI.Docking;

public class VS2013LightTheme : VS2013ThemeBase
{
	public VS2013LightTheme()
		: base(ThemeBase.Decompress(Resources.vs2013light_vstheme))
	{
	}
}

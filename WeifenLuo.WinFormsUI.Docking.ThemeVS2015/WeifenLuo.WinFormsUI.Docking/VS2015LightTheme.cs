using WeifenLuo.WinFormsUI.ThemeVS2015;

namespace WeifenLuo.WinFormsUI.Docking;

public class VS2015LightTheme : VS2015ThemeBase
{
	public VS2015LightTheme()
		: base(ThemeBase.Decompress(Resources.vs2015light_vstheme))
	{
	}
}

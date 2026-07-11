using WeifenLuo.WinFormsUI.ThemeVS2015;

namespace WeifenLuo.WinFormsUI.Docking;

public class VS2015BlueTheme : VS2015ThemeBase
{
	public VS2015BlueTheme()
		: base(ThemeBase.Decompress(Resources.vs2015blue_vstheme))
	{
	}
}

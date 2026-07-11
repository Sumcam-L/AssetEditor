using WeifenLuo.WinFormsUI.ThemeVS2012;

namespace WeifenLuo.WinFormsUI.Docking;

public class VS2012DarkTheme : VS2012ThemeBase
{
	public VS2012DarkTheme()
		: base(ThemeBase.Decompress(Resources.vs2012dark_vstheme), null, null)
	{
	}
}

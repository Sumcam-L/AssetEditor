using WeifenLuo.WinFormsUI.ThemeVS2012;

namespace WeifenLuo.WinFormsUI.Docking;

public class VS2012LightTheme : VS2012ThemeBase
{
	public VS2012LightTheme()
		: base(ThemeBase.Decompress(Resources.vs2012light_vstheme), null, null)
	{
	}
}

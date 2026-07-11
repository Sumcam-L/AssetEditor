using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

public class FiraxisTheme : FiraxisThemeBase
{
	public FiraxisTheme()
		: base(ThemeBase.Decompress(Resources.firaxisdark_vstheme))
	{
	}
}

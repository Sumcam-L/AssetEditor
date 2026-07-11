using System;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.ATF;

public interface IThemeService
{
	ThemeBase ActiveTheme { get; set; }

	event EventHandler ThemeChanged;
}

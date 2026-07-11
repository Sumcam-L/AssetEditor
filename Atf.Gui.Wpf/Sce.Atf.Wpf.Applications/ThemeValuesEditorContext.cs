using System.Collections.Generic;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications;

public class ThemeValuesEditorContext : NotifyPropertyChangedBase
{
	public IEnumerable<string> StandardValues { get; private set; }

	public ThemeValuesEditorContext(IEnumerable<string> skins)
	{
		StandardValues = skins;
	}
}

using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.ATF;

public class DockContent : WeifenLuo.WinFormsUI.Docking.DockContent
{
	protected override string GetPersistString()
	{
		return base.Name;
	}
}

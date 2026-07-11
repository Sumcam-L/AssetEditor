using Firaxis.MVVMBase;

namespace UtilityTools.ViewModels;

public class VFXViewModel : SelectableItemViewModel
{
	public string VFXName { get; private set; }

	public VFXViewModel(string vfxName)
	{
		VFXName = vfxName;
	}
}

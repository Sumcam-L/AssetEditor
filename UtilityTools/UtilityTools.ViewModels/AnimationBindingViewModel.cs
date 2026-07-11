using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;

namespace UtilityTools.ViewModels;

public class AnimationBindingViewModel : SelectableItemViewModel
{
	public readonly IAnimationBinding AnimationBinding;

	public string BoundName => AnimationBinding.AnimationName;

	public override string DisplayName => $"{SlotName}: {BoundName}";

	public string SlotName => AnimationBinding.SlotName;

	public AnimationBindingViewModel(IAnimationBinding binding)
	{
		AnimationBinding = binding;
	}
}

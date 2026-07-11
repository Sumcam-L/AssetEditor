using System;
using System.Windows.Input;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetBrowser.ViewModels;

public class AnimationInstanceViewModel : ImportedEntityViewModel
{
	public override ICommand ReimportCommand => null;

	public IAnimationInstance BackingAnimation => (IAnimationInstance)base.Entity;

	public AnimationInstanceViewModel(ICivTechService civTechSvc, string name, InstanceType type, Func<string, InstanceType, IInstanceEntity> loadFunction)
		: base(civTechSvc, name, type, loadFunction)
	{
	}
}

using System;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetBrowser.ViewModels;

public class TextureInstanceViewModel : ImportedEntityViewModel
{
	public ITextureInstance BackingTexture => (ITextureInstance)base.Entity;

	public TextureInstanceViewModel(ICivTechService civTechSvc, string name, InstanceType type, Func<string, InstanceType, IInstanceEntity> loadFunction)
		: base(civTechSvc, name, type, loadFunction)
	{
	}
}

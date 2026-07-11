using System.Collections.Generic;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetEditing;

public class ParticleEffectReferenceAdapter : AssetComponentAdapterBase, IAssetBrowserTypeProvider
{
	public string Name
	{
		get
		{
			return GetAttribute<string>(EntitySchema.ParticleEffectReferenceType.NameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.ParticleEffectReferenceType.NameAttribute, value);
		}
	}

	public IEnumerable<string> ValidClassNames
	{
		get
		{
			if (base.AssetAdapter.CivTechService.PrimaryProject.Config.Classes.FindForInstance(base.AssetAdapter.Asset) is IAssetClass assetClass)
			{
				return assetClass.AllowedParticleEffectClasses;
			}
			return null;
		}
	}

	public IEnumerable<InstanceType> ValidTypes => new InstanceType[1] { InstanceType.IT_PARTICLE_EFFECT };

	public IEntityFilteringContext EntityFilteringContext => CivTechRegistry.EntityFilteringService.GetFilteringContext(ValidTypes, ValidClassNames);
}

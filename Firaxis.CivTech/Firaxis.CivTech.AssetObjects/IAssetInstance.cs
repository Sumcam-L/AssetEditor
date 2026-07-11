using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IAssetInstance : IAnimatableEntity, IAnimatable, IEntityContainerEntity, IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>, IBehaviorDataProvider
{
	IGeometrySet GeometrySet { get; }

	ISplineSet SplineSet { get; }

	IModelInstance AddModelInstance(string instanceName, IGeometryInstance geo);

	void RemoveModelInstance(string instanceName);

	void AddGeometry(IGeometryInstance geo);

	void AddAnimation(IAnimationInstance anim);

	void AddMaterial(IMaterialInstance mat);

	void AddParticleEffect(IParticleEffectInstance vfx);

	void AddParticleEffect(string vfxName);

	void RemoveGeometry(IGeometryInstance geo);

	void RemoveAnimation(IAnimationInstance anim);

	void RemoveMaterial(IMaterialInstance mat);

	void RemoveParticleEffect(IParticleEffectInstance particle);

	void RemoveParticleEffect(string particleName);

	IEnumerable<string> GetGeometries();

	IEnumerable<string> GetAnimations();

	IEnumerable<string> GetMaterials();

	IEnumerable<string> GetParticleEffects();

	IList<string> GetDependentAssets();
}

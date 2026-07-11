using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class ParticleEffectClass : ClassEntity, IParticleEffectClass
{
	public unsafe ParticleEffectClass(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EClassSet_002EPush_003Cclass_0020AssetObjects_003A_003AParticleEffectClass_003E(pkClassSet), pkDeserializer)
	{
	}

	public unsafe ParticleEffectClass(global::AssetObjects.ParticleEffectClass* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)pkClassEntity, pkDeserializer)
	{
	}
}

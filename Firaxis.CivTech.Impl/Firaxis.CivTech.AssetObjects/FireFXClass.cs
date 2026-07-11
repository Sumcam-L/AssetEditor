using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class FireFXClass : ClassEntity, IFireFXClass
{
	public unsafe FireFXClass(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EClassSet_002EPush_003Cclass_0020AssetObjects_003A_003AFireFXClass_003E(pkClassSet), pkDeserializer)
	{
	}

	public unsafe FireFXClass(global::AssetObjects.FireFXClass* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)pkClassEntity, pkDeserializer)
	{
	}
}

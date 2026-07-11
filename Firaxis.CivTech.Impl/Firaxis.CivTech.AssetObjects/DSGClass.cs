using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class DSGClass : ClassEntity, IDSGClass
{
	public unsafe DSGClass(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EClassSet_002EPush_003Cclass_0020AssetObjects_003A_003ADSGClass_003E(pkClassSet), pkDeserializer)
	{
	}

	public unsafe DSGClass(global::AssetObjects.DSGClass* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)pkClassEntity, pkDeserializer)
	{
	}
}

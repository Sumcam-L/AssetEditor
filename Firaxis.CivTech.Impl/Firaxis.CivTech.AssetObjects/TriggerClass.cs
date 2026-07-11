using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class TriggerClass : ClassEntity, ITriggerClass
{
	public unsafe TriggerClass(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EClassSet_002EPush_003Cclass_0020AssetObjects_003A_003ATriggerClass_003E(pkClassSet), pkDeserializer)
	{
	}

	public unsafe TriggerClass(global::AssetObjects.TriggerClass* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)pkClassEntity, pkDeserializer)
	{
	}
}

using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class AnalyticLightClass : ClassEntity, IAnalyticLightClass
{
	public unsafe AnalyticLightClass(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EClassSet_002EPush_003Cclass_0020AssetObjects_003A_003AAnalyticLightClass_003E(pkClassSet), pkDeserializer)
	{
	}

	public unsafe AnalyticLightClass(global::AssetObjects.AnalyticLightClass* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)pkClassEntity, pkDeserializer)
	{
	}
}

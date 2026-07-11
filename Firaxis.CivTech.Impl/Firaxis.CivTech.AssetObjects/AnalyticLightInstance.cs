using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class AnalyticLightInstance : ImportedEntity, IAnalyticLightInstance
{
	public unsafe AnalyticLightInstance(global::AssetObjects.AnalyticLightInstance* pkLightInstance, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)pkLightInstance, pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}

	public unsafe AnalyticLightInstance(global::AssetObjects.InstanceSet* pkInstanceSet, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)global::_003CModule_003E.AssetObjects_002EInstanceSet_002EPush_003Cclass_0020AssetObjects_003A_003AAnalyticLightInstance_003E(pkInstanceSet), pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}
}

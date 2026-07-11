using System.Collections.Generic;
using System.Linq;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class MaterialInstance : InstanceEntity, IMaterialInstance
{
	public unsafe MaterialInstance(global::AssetObjects.MaterialInstance* pkInstance, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)pkInstance, pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}

	public unsafe MaterialInstance(global::AssetObjects.InstanceSet* pkInstanceSet, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)global::_003CModule_003E.AssetObjects_002EInstanceSet_002EPush_003Cclass_0020AssetObjects_003A_003AMaterialInstance_003E(pkInstanceSet), pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}

	public override void PublishStats(IDictionary<string, int> stats)
	{
		base.PublishStats(stats);
		IValue value = CookParameters.FindValue("Opacity");
		if (value != null && value.ParameterType == ValueType.VT_OBJECT)
		{
			IObjectValue objectValue = (IObjectValue)value;
			if (objectValue != null && objectValue.GetBoundObjectType() == InstanceType.IT_TEXTURE && !string.IsNullOrEmpty(objectValue.GetBoundObjectName()) && !Tags.Contains("Has Opacity"))
			{
				AddTag("Has Opacity");
			}
		}
	}
}

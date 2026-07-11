using System.Collections.Generic;
using System.Linq;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class AnimationInstance : ImportedEntity, IAnimationInstance
{
	public unsafe virtual float Duration
	{
		get
		{
			return global::_003CModule_003E.AssetObjects_002EAnimationInstance_002EGetDuration((global::AssetObjects.AnimationInstance*)m_pkEntity);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002EAnimationInstance_002ESetDuration((global::AssetObjects.AnimationInstance*)m_pkEntity, value);
		}
	}

	public unsafe AnimationInstance(global::AssetObjects.AnimationInstance* pkInstance, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)pkInstance, pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}

	public unsafe AnimationInstance(global::AssetObjects.InstanceSet* pkInstanceSet, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)global::_003CModule_003E.AssetObjects_002EInstanceSet_002EPush_003Cclass_0020AssetObjects_003A_003AAnimationInstance_003E(pkInstanceSet), pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}

	public override void PublishStats(IDictionary<string, int> stats)
	{
		base.PublishStats(stats);
		if ((Duration <= 0f || Duration >= 300f) && !Tags.Contains("Suspect Duration"))
		{
			AddTag("Suspect Duration");
		}
	}
}

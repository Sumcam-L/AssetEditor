using System;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class BehaviorRemoved : EntityChangedEvent, IBehaviorRemoved
{
	private string m_behaviorName = string.Empty;

	public virtual string BehaviorName
	{
		get
		{
			return m_behaviorName;
		}
		set
		{
			m_behaviorName = value;
		}
	}

	public BehaviorRemoved()
	{
		SetChangeType(EntityChangeType.ECT_BEHAVIOR_REMOVED);
	}

	public unsafe override void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		StandardStringWrapper standardStringWrapper = null;
		global::AssetObjects.BehaviorRemoved* ptr = AddToChangeList_Checked_003CAssetObjects_003A_003ABehaviorRemoved_003E(changeList);
		if (ptr != null)
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(m_behaviorName);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EBehaviorRemoved_002ESetBehaviorName(ptr, standardStringWrapper.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
	}
}

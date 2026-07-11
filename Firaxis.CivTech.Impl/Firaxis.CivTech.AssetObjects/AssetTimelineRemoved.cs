using System;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class AssetTimelineRemoved : EntityChangedEvent, IAssetTimelineRemoved
{
	private string m_timelineName = string.Empty;

	public virtual string TimelineName
	{
		get
		{
			return m_timelineName;
		}
		set
		{
			m_timelineName = value;
		}
	}

	public AssetTimelineRemoved()
	{
		SetChangeType(EntityChangeType.ECT_ASSET_TIMELINE_REMOVED);
	}

	public unsafe override void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		StandardStringWrapper standardStringWrapper = null;
		global::AssetObjects.AssetTimelineRemoved* ptr = AddToChangeList_Checked_003CAssetObjects_003A_003AAssetTimelineRemoved_003E(changeList);
		if (ptr != null)
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(m_timelineName);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EAssetTimelineRemoved_002ESetTimelineName(ptr, standardStringWrapper.Value);
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

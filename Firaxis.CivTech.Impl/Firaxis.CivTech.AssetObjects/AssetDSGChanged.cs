using System;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class AssetDSGChanged : EntityChangedEvent, IAssetDSGChanged
{
	private string m_dsgName = string.Empty;

	public virtual string DSGName
	{
		get
		{
			return m_dsgName;
		}
		set
		{
			m_dsgName = value;
		}
	}

	public AssetDSGChanged()
	{
		SetChangeType(EntityChangeType.ECT_ASSET_DSG_CHANGED);
	}

	public unsafe override void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		StandardStringWrapper standardStringWrapper = null;
		global::AssetObjects.AssetDSGChanged* ptr = AddToChangeList_Checked_003CAssetObjects_003A_003AAssetDSGChanged_003E(changeList);
		if (ptr != null)
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(m_dsgName);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EAssetDSGChanged_002ESetDSGName(ptr, standardStringWrapper.Value);
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

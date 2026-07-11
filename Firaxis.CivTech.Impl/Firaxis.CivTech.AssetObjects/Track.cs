using System;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class Track : ITrack
{
	private unsafe global::AssetObjects.Track* m_pkTrack;

	public unsafe virtual TriggerType Type
	{
		get
		{
			return global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002EConvertTriggerType(global::_003CModule_003E.AssetObjects_002ETrack_002EGetType(m_pkTrack));
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002ETrack_002ESetType(m_pkTrack, global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002EConvertTriggerType(value));
		}
	}

	public unsafe virtual string Name
	{
		get
		{
			return new string(global::_003CModule_003E.AssetObjects_002ETrack_002EGetName(m_pkTrack));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002ETrack_002ESetName(m_pkTrack, standardStringWrapper.Value);
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

	internal unsafe Track(global::AssetObjects.Track* pkTrack)
	{
		m_pkTrack = pkTrack;
		base._002Ector();
	}

	internal unsafe global::AssetObjects.Track* GetTrackPointer()
	{
		return m_pkTrack;
	}
}

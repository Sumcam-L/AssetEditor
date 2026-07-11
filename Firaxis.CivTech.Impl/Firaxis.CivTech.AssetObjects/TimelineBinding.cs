using System;
using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class TimelineBinding : ITimelineBinding
{
	private unsafe global::AssetObjects.TimelineBinding* m_pkBinding;

	public unsafe virtual string TimelineName
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002ETimelineBinding_002EGetTimelineName(m_pkBinding));
			return Marshal.PtrToStringAnsi(ptr);
		}
	}

	public unsafe virtual string SlotName
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002ETimelineBinding_002EGetName(m_pkBinding));
			return Marshal.PtrToStringAnsi(ptr);
		}
	}

	public unsafe TimelineBinding(global::AssetObjects.TimelineBinding* pkBinding)
	{
		m_pkBinding = pkBinding;
		base._002Ector();
	}

	internal unsafe global::AssetObjects.TimelineBinding* GetAssetObject()
	{
		return m_pkBinding;
	}

	internal unsafe virtual void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		m_pkBinding = null;
	}
}

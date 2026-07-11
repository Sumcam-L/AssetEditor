using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class TimelineSet : ITimelineSet
{
	private List<ITimeline> m_timelines;

	private unsafe global::AssetObjects.TimelineSet* m_timelineSet;

	public virtual IEnumerable<ITimeline> Timelines => m_timelines;

	public virtual ITimeline FindTimeline(string name)
	{
		List<ITimeline>.Enumerator enumerator = m_timelines.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				ITimeline current = enumerator.Current;
				if (current.Name == name)
				{
					return current;
				}
			}
			while (enumerator.MoveNext());
		}
		return null;
	}

	public unsafe virtual ITimeline GetTimeline(string name)
	{
		StandardStringWrapper standardStringWrapper = null;
		List<ITimeline>.Enumerator enumerator = m_timelines.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				ITimeline current = enumerator.Current;
				if (current.Name == name)
				{
					return current;
				}
			}
			while (enumerator.MoveNext());
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
		ITimeline timeline2;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetObjects.Timeline* timeline = global::_003CModule_003E.AssetObjects_002ETimelineSet_002EGetTimeline(m_timelineSet, standardStringWrapper.Value);
			timeline2 = new Timeline(m_timelineSet, timeline);
			m_timelines.Add(timeline2);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return timeline2;
	}

	public unsafe virtual ITimeline AddTimeline()
	{
		global::AssetObjects.Timeline* timeline = global::_003CModule_003E.AssetObjects_002ETimelineSet_002EAddTimeline(m_timelineSet);
		ITimeline timeline2 = new Timeline(m_timelineSet, timeline);
		m_timelines.Add(timeline2);
		return timeline2;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool RemoveTimeline(string name)
	{
		StandardStringWrapper standardStringWrapper = null;
		bool result = false;
		int num = 0;
		if (0 < m_timelines.Count)
		{
			do
			{
				if (!(m_timelines[num].Name == name))
				{
					num++;
					continue;
				}
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					result = global::_003CModule_003E.AssetObjects_002ETimelineSet_002ERemoveTimeline(m_timelineSet, standardStringWrapper.Value);
					m_timelines.RemoveAt(num);
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper).Dispose();
				break;
			}
			while (num < m_timelines.Count);
		}
		return result;
	}

	public unsafe virtual void ClearTimelines()
	{
		m_timelines = new List<ITimeline>();
		global::AssetObjects.TimelineSet* timelineSet = m_timelineSet;
		if (timelineSet != null)
		{
			global::_003CModule_003E.AssetObjects_002ETimelineSet_002EClearTimelines(timelineSet);
		}
	}

	public unsafe TimelineSet(global::AssetObjects.TimelineSet* timelineSet)
	{
		//IL_0040: Expected I, but got I8
		m_timelines = new List<ITimeline>();
		m_timelineSet = timelineSet;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0xedd1739c_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0TimelineSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040_0040Z_00404_NA && m_timelineSet == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040CDBAPNLD_0040m_timelineSet_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040DAOMEDML_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 13u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xedd1739c_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0TimelineSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		ResolveReferences();
	}

	internal unsafe virtual void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool disposing)
	{
		//IL_0012: Expected I, but got I8
		RemoveReferencesInternal(disposing);
		if (disposing)
		{
			m_timelineSet = null;
		}
	}

	private void RemoveReferencesInternal([MarshalAs(UnmanagedType.U1)] bool disposing)
	{
		List<ITimeline>.Enumerator enumerator = m_timelines.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((Timeline)enumerator.Current).RemoveReferences(disposing);
			}
			while (enumerator.MoveNext());
		}
		m_timelines.Clear();
	}

	private unsafe void ResolveReferences()
	{
		List<ITimeline> list = new List<ITimeline>();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ATimeline_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002ETimelineSet_002Ebegin(m_timelineSet, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ATimeline_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ATimeline_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002ETimelineSet_002Eend(m_timelineSet, &iterator2)))
		{
			do
			{
				global::AssetObjects.Timeline* timeline = global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ATimeline_002C4096_003E_002Eiterator_002E_002A(&iterator);
				Timeline item = new Timeline(m_timelineSet, timeline);
				list.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ATimeline_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ATimeline_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002ETimelineSet_002Eend(m_timelineSet, &iterator2)));
		}
		m_timelines = list;
	}
}

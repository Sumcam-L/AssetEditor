using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class Timeline : ITimeline, IDisposable
{
	private string m_name;

	private string m_description;

	private string m_animationName;

	private readonly StandardStringWrapper m_nameWrapper;

	private unsafe global::AssetObjects.TimelineSet* m_timelineSet;

	private List<ITrigger> m_triggers;

	private unsafe global::AssetObjects.Timeline* m_lastValidTimeline;

	private List<ITrack> m_tracks;

	public unsafe virtual IEnumerable<ITrack> Tracks
	{
		get
		{
			global::AssetObjects.Timeline* timelinePointer = GetTimelinePointer();
			if (m_lastValidTimeline != timelinePointer)
			{
				CacheUnmanagedValues(timelinePointer);
				m_lastValidTimeline = timelinePointer;
			}
			return m_tracks;
		}
	}

	public unsafe virtual IEnumerable<ITrigger> Triggers
	{
		get
		{
			global::AssetObjects.Timeline* timelinePointer = GetTimelinePointer();
			if (m_lastValidTimeline != timelinePointer)
			{
				CacheUnmanagedValues(timelinePointer);
				m_lastValidTimeline = timelinePointer;
			}
			return m_triggers;
		}
	}

	public unsafe virtual float Duration
	{
		get
		{
			global::AssetObjects.Timeline* timelinePointer = GetTimelinePointer();
			if (timelinePointer != null)
			{
				return global::_003CModule_003E.AssetObjects_002ETimeline_002EGetDuration(timelinePointer);
			}
			return 0f;
		}
		set
		{
			global::AssetObjects.Timeline* timelinePointer = GetTimelinePointer();
			if (timelinePointer != null)
			{
				global::_003CModule_003E.AssetObjects_002ETimeline_002ESetDuration(timelinePointer, value);
			}
		}
	}

	public unsafe virtual string AnimationName
	{
		get
		{
			global::AssetObjects.Timeline* timelinePointer = GetTimelinePointer();
			if (m_lastValidTimeline != timelinePointer)
			{
				CacheUnmanagedValues(timelinePointer);
				m_lastValidTimeline = timelinePointer;
			}
			return m_animationName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_animationName = value;
			global::AssetObjects.Timeline* timelinePointer = GetTimelinePointer();
			if (timelinePointer != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002ETimeline_002ESetAnimation(timelinePointer, standardStringWrapper.Value);
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

	public unsafe virtual string Description
	{
		get
		{
			global::AssetObjects.Timeline* timelinePointer = GetTimelinePointer();
			if (m_lastValidTimeline != timelinePointer)
			{
				CacheUnmanagedValues(timelinePointer);
				m_lastValidTimeline = timelinePointer;
			}
			return m_description;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_description = value;
			global::AssetObjects.Timeline* timelinePointer = GetTimelinePointer();
			if (timelinePointer != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002ETimeline_002ESetDesription(timelinePointer, standardStringWrapper.Value);
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

	public unsafe virtual string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			global::AssetObjects.Timeline* timelinePointer = GetTimelinePointer();
			if (timelinePointer != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002ETimeline_002ESetName(timelinePointer, standardStringWrapper.Value);
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper).Dispose();
			}
			m_name = value;
			m_nameWrapper.AssignValue(value);
		}
	}

	public unsafe virtual ITrigger FindTrigger(string name)
	{
		global::AssetObjects.Timeline* timelinePointer = GetTimelinePointer();
		if (m_lastValidTimeline != timelinePointer)
		{
			CacheUnmanagedValues(timelinePointer);
			m_lastValidTimeline = timelinePointer;
		}
		List<ITrigger>.Enumerator enumerator = m_triggers.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				ITrigger current = enumerator.Current;
				if (current.Name == name)
				{
					return current;
				}
			}
			while (enumerator.MoveNext());
		}
		return null;
	}

	public unsafe virtual ITrigger GetTrigger(string name, TriggerType type)
	{
		StandardStringWrapper standardStringWrapper = null;
		global::AssetObjects.Timeline* timelinePointer = GetTimelinePointer();
		if (m_lastValidTimeline != timelinePointer)
		{
			CacheUnmanagedValues(timelinePointer);
			m_lastValidTimeline = timelinePointer;
		}
		List<ITrigger>.Enumerator enumerator = m_triggers.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				ITrigger current = enumerator.Current;
				if (current.Name == name)
				{
					if (current.Type == type)
					{
						return current;
					}
					return null;
				}
			}
			while (enumerator.MoveNext());
		}
		ITrigger trigger = null;
		global::AssetObjects.Timeline* timelinePointer2 = GetTimelinePointer();
		if (timelinePointer2 != null)
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::AssetObjects.TriggerType triggerType = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002EConvertTriggerType(type);
				global::AssetObjects.Trigger* trigger2 = global::_003CModule_003E.AssetObjects_002ETimeline_002EGetTrigger(timelinePointer2, standardStringWrapper.Value, triggerType);
				trigger = new Trigger(m_timelineSet, timelinePointer2, trigger2);
				m_triggers.Add(trigger);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
		return trigger;
	}

	public unsafe virtual void RemoveTrigger(string name)
	{
		StandardStringWrapper standardStringWrapper = null;
		global::AssetObjects.Timeline* timelinePointer = GetTimelinePointer();
		if (m_lastValidTimeline != timelinePointer)
		{
			CacheUnmanagedValues(timelinePointer);
			m_lastValidTimeline = timelinePointer;
		}
		int num = 0;
		if (0 >= m_triggers.Count)
		{
			return;
		}
		while (!(m_triggers[num].Name == name))
		{
			num++;
			if (num >= m_triggers.Count)
			{
				return;
			}
		}
		global::AssetObjects.Timeline* timelinePointer2 = GetTimelinePointer();
		if (timelinePointer2 != null)
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002ETimeline_002ERemoveTriggerByName(timelinePointer2, standardStringWrapper.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
		m_triggers.RemoveAt(num);
	}

	public unsafe virtual void ClearTriggers()
	{
		global::AssetObjects.Timeline* timelinePointer = GetTimelinePointer();
		if (timelinePointer != null)
		{
			global::_003CModule_003E.AssetObjects_002ETimeline_002EClearTriggers(timelinePointer);
		}
		m_triggers = new List<ITrigger>();
	}

	public unsafe virtual ITrack AddTrack(string name, TriggerType type)
	{
		//IL_0052: Expected I, but got I8
		global::AssetObjects.Timeline* timelinePointer = GetTimelinePointer();
		if (m_lastValidTimeline != timelinePointer)
		{
			CacheUnmanagedValues(timelinePointer);
			m_lastValidTimeline = timelinePointer;
		}
		StandardStringWrapper standardStringWrapper = new StandardStringWrapper(name);
		global::AssetObjects.Timeline* timelinePointer2 = GetTimelinePointer();
		if (!global::_003CModule_003E._003FA0xa23c3471_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddTrack_0040Timeline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUITrack_0040345_0040PE_0024AAVString_0040System_0040_0040W4TriggerType_0040345_0040_0040Z_00404_NA && timelinePointer2 == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08IJGKCKNJ_0040timeline_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040MMKCKLHP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 294u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xa23c3471_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddTrack_0040Timeline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUITrack_0040345_0040PE_0024AAVString_0040System_0040_0040W4TriggerType_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		ITrack track = new Track(global::_003CModule_003E.AssetObjects_002ETimeline_002EAddTrack(timelinePointer2, standardStringWrapper.Value, global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002EConvertTriggerType(type)));
		m_tracks.Add(track);
		return track;
	}

	public unsafe virtual void RemoveTrack(ITrack track)
	{
		//IL_004b: Expected I, but got I8
		//IL_007b: Expected I, but got I8
		//IL_00ab: Expected I, but got I8
		global::AssetObjects.Timeline* timelinePointer = GetTimelinePointer();
		if (m_lastValidTimeline != timelinePointer)
		{
			CacheUnmanagedValues(timelinePointer);
			m_lastValidTimeline = timelinePointer;
		}
		Track track2 = (Track)track;
		if (!global::_003CModule_003E._003FA0xa23c3471_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveTrack_0040Timeline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUITrack_0040345_0040_0040Z_00404_NA && track2 == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0N_0040HCOHADCK_0040pmTypedTrack_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040MMKCKLHP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 307u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xa23c3471_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveTrack_0040Timeline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUITrack_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		global::AssetObjects.Track* trackPointer = track2.GetTrackPointer();
		if (!global::_003CModule_003E._003FA0xa23c3471_002E_003FbIgnoreAlways_0040_003F9_003F_003FRemoveTrack_0040Timeline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUITrack_0040345_0040_0040Z_00404_NA && trackPointer == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06NFPEFOJH_0040pTrack_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040MMKCKLHP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 310u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xa23c3471_002E_003FbIgnoreAlways_0040_003F9_003F_003FRemoveTrack_0040Timeline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUITrack_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		global::AssetObjects.Timeline* timelinePointer2 = GetTimelinePointer();
		if (!global::_003CModule_003E._003FA0xa23c3471_002E_003FbIgnoreAlways_0040_003FBB_0040_003F_003FRemoveTrack_0040Timeline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUITrack_0040345_0040_0040Z_00404_NA && timelinePointer2 == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09PIFBFBDK_0040pTimeline_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040MMKCKLHP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 313u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xa23c3471_002E_003FbIgnoreAlways_0040_003FBB_0040_003F_003FRemoveTrack_0040Timeline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUITrack_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		global::_003CModule_003E.AssetObjects_002ETimeline_002ERemoveTrack(timelinePointer2, trackPointer);
		m_tracks.Remove(track);
	}

	public unsafe Timeline(global::AssetObjects.TimelineSet* timelineSet, global::AssetObjects.Timeline* timeline)
	{
		//IL_005f: Expected I, but got I8
		//IL_0085: Expected I, but got I8
		StandardStringWrapper nameWrapper = new StandardStringWrapper();
		try
		{
			m_nameWrapper = nameWrapper;
			m_timelineSet = timelineSet;
			m_triggers = new List<ITrigger>();
			m_lastValidTimeline = timeline;
			m_tracks = new List<ITrack>();
			base._002Ector();
			if (!global::_003CModule_003E._003FA0xa23c3471_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0Timeline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVTimelineSet_00402_0040PEAV12_0040_0040Z_00404_NA && m_timelineSet == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040CDBAPNLD_0040m_timelineSet_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040MMKCKLHP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 33u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xa23c3471_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0Timeline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVTimelineSet_00402_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			if (!global::_003CModule_003E._003FA0xa23c3471_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0Timeline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVTimelineSet_00402_0040PEAV12_0040_0040Z_00404_NA && timeline == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08IJGKCKNJ_0040timeline_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040MMKCKLHP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 34u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xa23c3471_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0Timeline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVTimelineSet_00402_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			string text = (m_name = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002ETimeline_002EGetName(timeline)));
			m_nameWrapper.AssignValue(text);
			CacheUnmanagedValues(timeline);
			return;
		}
		catch
		{
			//try-fault
			((IDisposable)m_nameWrapper).Dispose();
			throw;
		}
	}

	private void _007ETimeline()
	{
		RemoveReferences(disposing: true);
	}

	private void _0021Timeline()
	{
		RemoveReferences(disposing: true);
	}

	internal unsafe void UpdateCachedValuesIfNecessary(global::AssetObjects.Timeline* timeline)
	{
		if (m_lastValidTimeline != timeline)
		{
			CacheUnmanagedValues(timeline);
			m_lastValidTimeline = timeline;
		}
	}

	internal unsafe void CacheUnmanagedValues(global::AssetObjects.Timeline* timeline)
	{
		if (timeline != null)
		{
			m_description = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002ETimeline_002EGetDescription(timeline));
			m_animationName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002ETimeline_002EGetAnimationName(timeline));
			List<ITrigger> list = new List<ITrigger>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ATrigger_002C4096_003E.iterator iterator);
			global::_003CModule_003E.AssetObjects_002ETimeline_002Etriggers_begin(timeline, &iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ATrigger_002C4096_003E.iterator iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ATrigger_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002ETimeline_002Etriggers_end(timeline, &iterator2)))
			{
				do
				{
					global::AssetObjects.Trigger* trigger = global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ATrigger_002C4096_003E_002Eiterator_002E_002A(&iterator);
					Trigger item = new Trigger(m_timelineSet, timeline, trigger);
					list.Add(item);
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ATrigger_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ATrigger_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002ETimeline_002Etriggers_end(timeline, &iterator2)));
			}
			m_triggers = list;
			List<ITrack> list2 = new List<ITrack>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ATrack_002C4096_003E.iterator iterator3);
			global::_003CModule_003E.AssetObjects_002ETimeline_002Etracks_begin(timeline, &iterator3);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ATrack_002C4096_003E.iterator iterator4);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ATrack_002C4096_003E_002Eiterator_002E_0021_003D(&iterator3, global::_003CModule_003E.AssetObjects_002ETimeline_002Etracks_end(timeline, &iterator4)))
			{
				do
				{
					ITrack item2 = new Track(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ATrack_002C4096_003E_002Eiterator_002E_002A(&iterator3));
					list2.Add(item2);
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ATrack_002C4096_003E_002Eiterator_002E_002B_002B(&iterator3);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ATrack_002C4096_003E_002Eiterator_002E_0021_003D(&iterator3, global::_003CModule_003E.AssetObjects_002ETimeline_002Etracks_end(timeline, &iterator4)));
			}
			m_tracks = list2;
		}
		else
		{
			m_description = string.Empty;
			m_animationName = string.Empty;
			m_triggers = new List<ITrigger>();
			m_tracks = new List<ITrack>();
		}
	}

	internal unsafe global::AssetObjects.Timeline* GetTimelinePointer()
	{
		sbyte* value = m_nameWrapper.Value;
		return global::_003CModule_003E.AssetObjects_002ETimelineSet_002EFindTimeline(m_timelineSet, value);
	}

	internal unsafe virtual void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool disposing)
	{
		//IL_0034: Expected I, but got I8
		//IL_004a: Expected I, but got I8
		m_tracks.Clear();
		m_triggers.Clear();
		m_description = string.Empty;
		m_animationName = string.Empty;
		m_lastValidTimeline = null;
		if (disposing)
		{
			m_name = string.Empty;
			m_timelineSet = null;
		}
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			try
			{
				RemoveReferences(disposing: true);
				return;
			}
			finally
			{
				((IDisposable)m_nameWrapper).Dispose();
			}
		}
		try
		{
			RemoveReferences(disposing: true);
		}
		finally
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~Timeline()
	{
		Dispose(A_0: false);
	}
}

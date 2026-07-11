using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class Trigger : ITrigger, IDisposable
{
	private string m_name;

	private TriggerType m_type;

	private string m_fxName;

	private string m_collectionName;

	private string m_attachmentPointName;

	private string m_description;

	private readonly StandardStringWrapper m_timelineNameWrapper;

	private readonly StandardStringWrapper m_nameWrapper;

	private unsafe global::AssetObjects.TimelineSet* m_timelineSet;

	private unsafe global::AssetObjects.Trigger* m_lastValidTrigger;

	public unsafe virtual int TrackIndex
	{
		get
		{
			global::AssetObjects.Trigger* triggerPointer = GetTriggerPointer();
			if (triggerPointer != null)
			{
				return global::_003CModule_003E.AssetObjects_002ETrigger_002EGetTrackIndex(triggerPointer);
			}
			return 0;
		}
		set
		{
			global::AssetObjects.Trigger* triggerPointer = GetTriggerPointer();
			if (triggerPointer != null)
			{
				global::_003CModule_003E.AssetObjects_002ETrigger_002ESetTrackIndex(triggerPointer, value);
			}
		}
	}

	public unsafe virtual float Duration
	{
		get
		{
			global::AssetObjects.Trigger* triggerPointer = GetTriggerPointer();
			if (triggerPointer != null)
			{
				return global::_003CModule_003E.AssetObjects_002ETrigger_002EGetDuration(triggerPointer);
			}
			return 0f;
		}
		set
		{
			global::AssetObjects.Trigger* triggerPointer = GetTriggerPointer();
			if (triggerPointer != null)
			{
				global::_003CModule_003E.AssetObjects_002ETrigger_002ESetDuration(triggerPointer, value);
			}
		}
	}

	public unsafe virtual float StartTime
	{
		get
		{
			global::AssetObjects.Trigger* triggerPointer = GetTriggerPointer();
			if (triggerPointer != null)
			{
				return global::_003CModule_003E.AssetObjects_002ETrigger_002EGetStartTime(triggerPointer);
			}
			return 0f;
		}
		set
		{
			global::AssetObjects.Trigger* triggerPointer = GetTriggerPointer();
			if (triggerPointer != null)
			{
				global::_003CModule_003E.AssetObjects_002ETrigger_002ESetStartTime(triggerPointer, value);
			}
		}
	}

	public unsafe virtual string Description
	{
		get
		{
			global::AssetObjects.Trigger* triggerPointer = GetTriggerPointer();
			if (m_lastValidTrigger != triggerPointer)
			{
				m_lastValidTrigger = triggerPointer;
				CacheUnmanagedValues(triggerPointer);
			}
			return m_description;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_description = value;
			global::AssetObjects.Trigger* triggerPointer = GetTriggerPointer();
			if (triggerPointer != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002ETrigger_002ESetDescription(triggerPointer, standardStringWrapper.Value);
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

	public unsafe virtual string AttachmentPointName
	{
		get
		{
			global::AssetObjects.Trigger* triggerPointer = GetTriggerPointer();
			if (m_lastValidTrigger != triggerPointer)
			{
				m_lastValidTrigger = triggerPointer;
				CacheUnmanagedValues(triggerPointer);
			}
			return m_attachmentPointName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_attachmentPointName = value;
			global::AssetObjects.Trigger* triggerPointer = GetTriggerPointer();
			if (triggerPointer != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002ETrigger_002ESetAttachmentPointName(triggerPointer, standardStringWrapper.Value);
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

	public unsafe virtual string CollectionName
	{
		get
		{
			global::AssetObjects.Trigger* triggerPointer = GetTriggerPointer();
			if (m_lastValidTrigger != triggerPointer)
			{
				m_lastValidTrigger = triggerPointer;
				CacheUnmanagedValues(triggerPointer);
			}
			return m_collectionName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_collectionName = value;
			global::AssetObjects.Trigger* triggerPointer = GetTriggerPointer();
			if (triggerPointer != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002ETrigger_002ESetCollectionName(triggerPointer, standardStringWrapper.Value);
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

	public unsafe virtual string FXName
	{
		get
		{
			global::AssetObjects.Trigger* triggerPointer = GetTriggerPointer();
			if (m_lastValidTrigger != triggerPointer)
			{
				m_lastValidTrigger = triggerPointer;
				CacheUnmanagedValues(triggerPointer);
			}
			return m_fxName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_fxName = value;
			global::AssetObjects.Trigger* triggerPointer = GetTriggerPointer();
			if (triggerPointer != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002ETrigger_002ESetFXName(triggerPointer, standardStringWrapper.Value);
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

	public virtual TriggerType Type => m_type;

	public unsafe virtual string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
			global::AssetObjects.Trigger* triggerPointer = GetTriggerPointer();
			if (triggerPointer != null)
			{
				m_nameWrapper.AssignValue(value);
				global::_003CModule_003E.AssetObjects_002ETrigger_002ESetName(triggerPointer, m_nameWrapper.Value);
			}
		}
	}

	public unsafe Trigger(global::AssetObjects.TimelineSet* timelineSet, global::AssetObjects.Timeline* timeline, global::AssetObjects.Trigger* trigger)
	{
		//IL_0058: Expected I, but got I8
		//IL_0083: Expected I, but got I8
		StandardStringWrapper timelineNameWrapper = new StandardStringWrapper();
		try
		{
			m_timelineNameWrapper = timelineNameWrapper;
			StandardStringWrapper nameWrapper = new StandardStringWrapper();
			try
			{
				m_nameWrapper = nameWrapper;
				m_timelineSet = timelineSet;
				m_lastValidTrigger = trigger;
				base._002Ector();
				if (!global::_003CModule_003E._003FA0x606ec9a8_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0Trigger_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVTimelineSet_00402_0040PEAVTimeline_00402_0040PEAV12_0040_0040Z_00404_NA && m_timelineSet == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040CDBAPNLD_0040m_timelineSet_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GO_0040OCPABAKI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 69u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x606ec9a8_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0Trigger_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVTimelineSet_00402_0040PEAVTimeline_00402_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				if (!global::_003CModule_003E._003FA0x606ec9a8_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0Trigger_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVTimelineSet_00402_0040PEAVTimeline_00402_0040PEAV12_0040_0040Z_00404_NA && m_lastValidTrigger == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BD_0040LJIKDMJD_0040m_lastValidTrigger_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GO_0040OCPABAKI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 70u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x606ec9a8_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0Trigger_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVTimelineSet_00402_0040PEAVTimeline_00402_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				string text = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002ETimeline_002EGetName(timeline));
				m_timelineNameWrapper.AssignValue(text);
				string text2 = (m_name = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002ETrigger_002EGetName(trigger)));
				m_nameWrapper.AssignValue(text2);
				global::AssetObjects.TriggerType num = global::_003CModule_003E.AssetObjects_002ETrigger_002EGetType(trigger);
				TriggerType type = TriggerType.TT_SOUND;
				switch (num)
				{
				case (global::AssetObjects.TriggerType)0:
					type = TriggerType.TT_SOUND;
					break;
				case (global::AssetObjects.TriggerType)1:
					type = TriggerType.TT_ASSET_VFX;
					break;
				case (global::AssetObjects.TriggerType)2:
					type = TriggerType.TT_TRANSFER;
					break;
				case (global::AssetObjects.TriggerType)3:
					type = TriggerType.TT_ACTION;
					break;
				case (global::AssetObjects.TriggerType)4:
					type = TriggerType.TT_ARTDEF_VFX;
					break;
				case (global::AssetObjects.TriggerType)5:
					type = TriggerType.TT_LIGHT;
					break;
				}
				m_type = type;
				CacheUnmanagedValues(trigger);
				return;
			}
			catch
			{
				//try-fault
				((IDisposable)m_nameWrapper).Dispose();
				throw;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)m_timelineNameWrapper).Dispose();
			throw;
		}
	}

	private void _007ETrigger()
	{
		RemoveReferences(disposing: true);
	}

	private void _0021Trigger()
	{
		RemoveReferences(disposing: true);
	}

	internal unsafe void UpdateCachedValuesIfNecessary(global::AssetObjects.Trigger* trigger)
	{
		if (m_lastValidTrigger != trigger)
		{
			m_lastValidTrigger = trigger;
			CacheUnmanagedValues(trigger);
		}
	}

	internal unsafe void CacheUnmanagedValues(global::AssetObjects.Trigger* trigger)
	{
		if (trigger != null)
		{
			m_fxName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002ETrigger_002EGetFXName(trigger));
			m_collectionName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002ETrigger_002EGetCollectionName(trigger));
			m_attachmentPointName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002ETrigger_002EGetAttachmentPointName(trigger));
			m_description = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002ETrigger_002EGetDescription(trigger));
		}
		else
		{
			m_fxName = string.Empty;
			m_collectionName = string.Empty;
			m_attachmentPointName = string.Empty;
			m_description = string.Empty;
		}
	}

	internal unsafe global::AssetObjects.Trigger* GetTriggerPointer()
	{
		//IL_0003: Expected I, but got I8
		//IL_0084: Expected I, but got I8
		//IL_0081->IL0083: Incompatible stack types: I vs I8
		global::AssetObjects.Trigger* ptr = null;
		sbyte* value = m_timelineNameWrapper.Value;
		global::AssetObjects.Timeline* ptr2 = global::_003CModule_003E.AssetObjects_002ETimelineSet_002EFindTimeline(m_timelineSet, value);
		if (ptr2 != null)
		{
			sbyte* value2 = m_nameWrapper.Value;
			ptr = global::_003CModule_003E.AssetObjects_002ETimeline_002EFindTrigger(ptr2, value2);
			if (ptr != null)
			{
				TriggerType triggerType = TriggerType.TT_SOUND;
				switch (global::_003CModule_003E.AssetObjects_002ETrigger_002EGetType(ptr))
				{
				case (global::AssetObjects.TriggerType)0:
					triggerType = TriggerType.TT_SOUND;
					break;
				case (global::AssetObjects.TriggerType)1:
					triggerType = TriggerType.TT_ASSET_VFX;
					break;
				case (global::AssetObjects.TriggerType)2:
					triggerType = TriggerType.TT_TRANSFER;
					break;
				case (global::AssetObjects.TriggerType)3:
					triggerType = TriggerType.TT_ACTION;
					break;
				case (global::AssetObjects.TriggerType)4:
					triggerType = TriggerType.TT_ARTDEF_VFX;
					break;
				case (global::AssetObjects.TriggerType)5:
					triggerType = TriggerType.TT_LIGHT;
					break;
				}
				ptr = (global::AssetObjects.Trigger*)((triggerType != Type) ? 0 : ((nint)ptr));
			}
		}
		return ptr;
	}

	internal unsafe virtual void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool disposing)
	{
		//IL_0034: Expected I, but got I8
		//IL_003f: Expected I, but got I8
		m_fxName = string.Empty;
		m_collectionName = string.Empty;
		m_attachmentPointName = string.Empty;
		m_description = string.Empty;
		m_lastValidTrigger = null;
		if (disposing)
		{
			m_timelineSet = null;
			m_name = string.Empty;
		}
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			try
			{
				_007ETrigger();
				return;
			}
			finally
			{
				try
				{
					((IDisposable)m_nameWrapper).Dispose();
				}
				finally
				{
					try
					{
						((IDisposable)m_timelineNameWrapper).Dispose();
					}
					finally
					{
					}
				}
			}
		}
		try
		{
			_0021Trigger();
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

	~Trigger()
	{
		Dispose(A_0: false);
	}
}

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class EntityChangedEvent : IEntityChangedEvent
{
	private string m_entityName = string.Empty;

	private InstanceType m_entityType = InstanceType.IT_INVALID;

	private EntityChangeType m_changeType = EntityChangeType.ECT_GENERIC;

	public virtual InstanceType InstanceType => m_entityType;

	public virtual string EntityName => m_entityName;

	public virtual EntityChangeType Type => m_changeType;

	public void SetEntity(InstanceType type, string name)
	{
		m_entityType = type;
		m_entityName = name;
	}

	public unsafe virtual void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		EntityChangeType changeType = m_changeType;
		byte condition = ((changeType == EntityChangeType.ECT_GENERIC) ? ((byte)1) : ((byte)0));
		BugSubmitter.Assert(condition != 0, $"Calling generic entity change event's 'AddToChangeList' for a non-generic change!  Change Type: '{((EntityChangeType)(object)changeType).ToString()}'  @assign bwhitman");
		AddToChangeList_Checked_003CAssetObjects_003A_003AEntityChangedEvent_003E(changeList);
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public bool HasEntitySet()
	{
		int num = ((m_entityType != InstanceType.IT_INVALID && !string.IsNullOrWhiteSpace(m_entityName)) ? 1 : 0);
		return (byte)num != 0;
	}

	protected void SetChangeType(EntityChangeType changeType)
	{
		m_changeType = changeType;
	}

	protected unsafe void SetEntityNative(global::AssetObjects.EntityChangedEvent* changeEvent)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(m_entityName);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetObjects.InstanceType entityType = (global::AssetObjects.InstanceType)m_entityType;
			global::_003CModule_003E.AssetObjects_002EEntityChangedEvent_002ESetEntity(changeEvent, entityType, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	protected unsafe global::AssetObjects.AssetAnimationSetChanged* AddToChangeList_Checked_003CAssetObjects_003A_003AAssetAnimationSetChanged_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.AssetAnimationSetChanged* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003AAssetAnimationSetChanged_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VAssetAnimationSetChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVAssetAnimationSetChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VAssetAnimationSetChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVAssetAnimationSetChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.AssetDSGChanged* AddToChangeList_Checked_003CAssetObjects_003A_003AAssetDSGChanged_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.AssetDSGChanged* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003AAssetDSGChanged_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VAssetDSGChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVAssetDSGChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VAssetDSGChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVAssetDSGChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.AssetTimelineChanged* AddToChangeList_Checked_003CAssetObjects_003A_003AAssetTimelineChanged_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.AssetTimelineChanged* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003AAssetTimelineChanged_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VAssetTimelineChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVAssetTimelineChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VAssetTimelineChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVAssetTimelineChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.AssetTimelineRemoved* AddToChangeList_Checked_003CAssetObjects_003A_003AAssetTimelineRemoved_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.AssetTimelineRemoved* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003AAssetTimelineRemoved_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VAssetTimelineRemoved_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVAssetTimelineRemoved_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VAssetTimelineRemoved_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVAssetTimelineRemoved_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.AssetTimelineSetChanged* AddToChangeList_Checked_003CAssetObjects_003A_003AAssetTimelineSetChanged_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.AssetTimelineSetChanged* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003AAssetTimelineSetChanged_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VAssetTimelineSetChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVAssetTimelineSetChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VAssetTimelineSetChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVAssetTimelineSetChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.AttachmentChanged* AddToChangeList_Checked_003CAssetObjects_003A_003AAttachmentChanged_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.AttachmentChanged* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003AAttachmentChanged_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VAttachmentChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVAttachmentChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VAttachmentChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVAttachmentChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.AttachmentRemoved* AddToChangeList_Checked_003CAssetObjects_003A_003AAttachmentRemoved_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.AttachmentRemoved* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003AAttachmentRemoved_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VAttachmentRemoved_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVAttachmentRemoved_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VAttachmentRemoved_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVAttachmentRemoved_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.AttachmentCookParameterChanged* AddToChangeList_Checked_003CAssetObjects_003A_003AAttachmentCookParameterChanged_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.AttachmentCookParameterChanged* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003AAttachmentCookParameterChanged_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VAttachmentCookParameterChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVAttachmentCookParameterChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VAttachmentCookParameterChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVAttachmentCookParameterChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.BehaviorAdded* AddToChangeList_Checked_003CAssetObjects_003A_003ABehaviorAdded_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.BehaviorAdded* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003ABehaviorAdded_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VBehaviorAdded_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVBehaviorAdded_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VBehaviorAdded_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVBehaviorAdded_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.BehaviorRemoved* AddToChangeList_Checked_003CAssetObjects_003A_003ABehaviorRemoved_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.BehaviorRemoved* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003ABehaviorRemoved_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VBehaviorRemoved_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVBehaviorRemoved_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VBehaviorRemoved_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVBehaviorRemoved_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.EntityChangedEvent* AddToChangeList_Checked_003CAssetObjects_003A_003AEntityChangedEvent_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.EntityChangedEvent* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003AEntityChangedEvent_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VEntityChangedEvent_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAV12_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VEntityChangedEvent_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAV12_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative(ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.EntityCookParameterChanged* AddToChangeList_Checked_003CAssetObjects_003A_003AEntityCookParameterChanged_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.EntityCookParameterChanged* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003AEntityCookParameterChanged_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VEntityCookParameterChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVEntityCookParameterChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VEntityCookParameterChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVEntityCookParameterChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe LightDirectionTagChanged* AddToChangeList_Checked_003CAssetObjects_003A_003ALightDirectionTagChanged_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		LightDirectionTagChanged* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003ALightDirectionTagChanged_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VLightDirectionTagChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVLightDirectionTagChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VLightDirectionTagChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVLightDirectionTagChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe LightDirectionTagRemoved* AddToChangeList_Checked_003CAssetObjects_003A_003ALightDirectionTagRemoved_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		LightDirectionTagRemoved* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003ALightDirectionTagRemoved_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VLightDirectionTagRemoved_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVLightDirectionTagRemoved_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VLightDirectionTagRemoved_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVLightDirectionTagRemoved_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.ModelInstanceChanged* AddToChangeList_Checked_003CAssetObjects_003A_003AModelInstanceChanged_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.ModelInstanceChanged* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003AModelInstanceChanged_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VModelInstanceChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVModelInstanceChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VModelInstanceChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVModelInstanceChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.ModelInstanceRemoved* AddToChangeList_Checked_003CAssetObjects_003A_003AModelInstanceRemoved_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.ModelInstanceRemoved* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003AModelInstanceRemoved_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VModelInstanceRemoved_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVModelInstanceRemoved_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VModelInstanceRemoved_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVModelInstanceRemoved_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.ParticleEffectAdded* AddToChangeList_Checked_003CAssetObjects_003A_003AParticleEffectAdded_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.ParticleEffectAdded* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003AParticleEffectAdded_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VParticleEffectAdded_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVParticleEffectAdded_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VParticleEffectAdded_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVParticleEffectAdded_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.ParticleEffectRemoved* AddToChangeList_Checked_003CAssetObjects_003A_003AParticleEffectRemoved_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.ParticleEffectRemoved* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003AParticleEffectRemoved_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VParticleEffectRemoved_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVParticleEffectRemoved_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VParticleEffectRemoved_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVParticleEffectRemoved_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.SplineVertexChanged* AddToChangeList_Checked_003CAssetObjects_003A_003ASplineVertexChanged_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.SplineVertexChanged* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003ASplineVertexChanged_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VSplineVertexChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVSplineVertexChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VSplineVertexChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVSplineVertexChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}

	protected unsafe global::AssetObjects.TriGroupParameterChanged* AddToChangeList_Checked_003CAssetObjects_003A_003ATriGroupParameterChanged_003E(global::AssetObjects.EntityChangeList* changeList)
	{
		//IL_0031: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		if (!HasEntitySet())
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040BKJKIAJK_0040Attempted_003F5to_003F5add_003F5an_003F5empty_003F5entity_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 50u);
			return null;
		}
		global::AssetObjects.TriGroupParameterChanged* ptr = global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EPush_003Cclass_0020AssetObjects_003A_003ATriGroupParameterChanged_003E(changeList);
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VTriGroupParameterChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVTriGroupParameterChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LLPCIPLJ_0040changeEvent_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HP_0040DFEHMIBK_0040c_003F3_003F2buildagent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F_0024AddToChangeList_Checked_0040VTriGroupParameterChanged_0040AssetObjects_0040_0040_0040EntityChangedEvent_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAMPEAVTriGroupParameterChanged_00402_0040AEAVEntityChangeList_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		SetEntityNative((global::AssetObjects.EntityChangedEvent*)ptr);
		return ptr;
	}
}

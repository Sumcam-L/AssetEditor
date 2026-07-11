using System;
using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class AttachmentCookParameterChanged : EntityChangedEvent, IAttachmentCookParameterChanged
{
	private string m_attachmentName = string.Empty;

	private string m_parameterName = string.Empty;

	private IValue m_newValue = null;

	public virtual IValue ChangedValue
	{
		get
		{
			return m_newValue;
		}
		set
		{
			byte condition = ((value != null) ? ((byte)1) : ((byte)0));
			BugSubmitter.Assert(condition != 0, "Invalid to pass in a null new value to AttachmentCookParameterChanged.");
			m_newValue = value;
		}
	}

	public virtual string ParameterName
	{
		get
		{
			return m_parameterName;
		}
		set
		{
			m_parameterName = value;
		}
	}

	public virtual string AttachmentName
	{
		get
		{
			return m_attachmentName;
		}
		set
		{
			m_attachmentName = value;
		}
	}

	public AttachmentCookParameterChanged()
	{
		SetChangeType(EntityChangeType.ECT_ATTACHMENT_COOK_PARAMETER_CHANGED);
	}

	public unsafe override void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		if (string.IsNullOrEmpty(m_parameterName))
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040HOGCFJAG_0040Attempted_003F5to_003F5add_003F5a_003F5cook_003F5paramete_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IN_0040HGALLDKJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 29u);
			return;
		}
		IValue newValue = m_newValue;
		if (newValue == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GN_0040JLABFFMG_0040Attempted_003F5to_003F5add_003F5a_003F5cook_003F5paramete_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BG_0040GCGEKAAF_0040m_newValue_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IN_0040HGALLDKJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 35u);
			return;
		}
		Value value = (Value)newValue;
		if (value == null)
		{
			BugSubmitter.SilentAssert(condition: false, $"safe_cast<Value^> failure on the new value member of the change event.  Actual Value Type: {newValue.GetType().ToString()}  @assign bwhitman");
			return;
		}
		global::AssetObjects.Value* assetObject = value.GetAssetObject();
		if (assetObject == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D4);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D4), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EC_0040FOFJEGDE_0040Adding_003F5disposed_003F5new_003F5Value_003F5to_003F5ent_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BK_0040JGHJDKKB_0040nativeNewValue_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D4), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IN_0040HGALLDKJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 51u);
			return;
		}
		global::AssetObjects.AttachmentCookParameterChanged* ptr = AddToChangeList_Checked_003CAssetObjects_003A_003AAttachmentCookParameterChanged_003E(changeList);
		if (ptr == null)
		{
			return;
		}
		StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(m_attachmentName);
		try
		{
			standardStringWrapper = standardStringWrapper3;
			StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(m_parameterName);
			try
			{
				standardStringWrapper2 = standardStringWrapper4;
				global::_003CModule_003E.AssetObjects_002EAttachmentCookParameterChanged_002ESetAttachmentName(ptr, standardStringWrapper.Value);
				global::_003CModule_003E.AssetObjects_002EAttachmentCookParameterChanged_002ESetParameterName(ptr, standardStringWrapper2.Value);
				global::_003CModule_003E.AssetObjects_002EAttachmentCookParameterChanged_002ESetNewValue(ptr, assetObject);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper2).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper2).Dispose();
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

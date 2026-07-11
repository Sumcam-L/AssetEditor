using System;
using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class EntityCookParameterChanged : EntityChangedEvent, IEntityCookParameterChanged
{
	private string m_parameterName = string.Empty;

	private IValue m_value = null;

	public virtual IValue ChangedValue
	{
		get
		{
			return m_value;
		}
		set
		{
			byte condition = ((value != null) ? ((byte)1) : ((byte)0));
			BugSubmitter.Assert(condition != 0, "Invalid to pass in a null vale to EntityCookParameterChanged.");
			m_value = value;
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

	public EntityCookParameterChanged()
	{
		SetChangeType(EntityChangeType.ECT_COOK_PARAMETER_CHANGED);
	}

	public unsafe override void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		StandardStringWrapper standardStringWrapper = null;
		if (string.IsNullOrEmpty(m_parameterName))
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040HOGCFJAG_0040Attempted_003F5to_003F5add_003F5a_003F5cook_003F5paramete_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IJ_0040DEBCOMMI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 29u);
			return;
		}
		IValue value = m_value;
		if (value == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GJ_0040FNJKPIDH_0040Attempted_003F5to_003F5add_003F5a_003F5cook_003F5paramete_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BD_0040IKHOKEE_0040m_value_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IJ_0040DEBCOMMI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 35u);
			return;
		}
		Value value2 = (Value)value;
		if (value2 == null)
		{
			BugSubmitter.SilentAssert(condition: false, $"safe_cast<Value^> failure on the value member of the change event.  Actual Value Type: {value.GetType().ToString()}  @assign bwhitman");
			return;
		}
		global::AssetObjects.Value* assetObject = value2.GetAssetObject();
		if (assetObject == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D4);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D4), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DO_0040DIGIPNHJ_0040Adding_003F5disposed_003F5Value_003F5to_003F5entity_003F5_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040EFACGLD_0040nativeValue_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D4), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IJ_0040DEBCOMMI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 51u);
			return;
		}
		global::AssetObjects.EntityCookParameterChanged* ptr = AddToChangeList_Checked_003CAssetObjects_003A_003AEntityCookParameterChanged_003E(changeList);
		if (ptr != null)
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(m_parameterName);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EEntityCookParameterChanged_002ESetParameterName(ptr, standardStringWrapper.Value);
				global::_003CModule_003E.AssetObjects_002EEntityCookParameterChanged_002ESetNewValue(ptr, assetObject);
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

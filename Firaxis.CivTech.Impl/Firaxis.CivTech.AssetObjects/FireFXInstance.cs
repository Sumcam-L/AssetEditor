using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Firaxis.Error;
using Platform;
using Reflection;

namespace Firaxis.CivTech.AssetObjects;

public class FireFXInstance : InstanceEntity, IFireFXInstance
{
	private IFireFXInstanceData m_pmData;

	public virtual IFireFXInstanceData InstanceData => m_pmData;

	public unsafe FireFXInstance(global::AssetObjects.FireFXInstance* pkInstance, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)pkInstance, pkDeserializer, pkSerializer, pkVirtualPantry)
	{
		m_pmData = ConstructDataWrapper();
	}

	public unsafe FireFXInstance(global::AssetObjects.InstanceSet* pkInstanceSet, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)global::_003CModule_003E.AssetObjects_002EInstanceSet_002EPush_003Cclass_0020AssetObjects_003A_003AFireFXInstance_003E(pkInstanceSet), pkDeserializer, pkSerializer, pkVirtualPantry)
	{
		m_pmData = ConstructDataWrapper();
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public override bool DeserializeFromXML(string pmXmlText)
	{
		if (!base.DeserializeFromXML(pmXmlText))
		{
			return false;
		}
		m_pmData = ConstructDataWrapper();
		return true;
	}

	public override Firaxis.Error.ResultCode DeserializeFromFile(string pmFilePath)
	{
		Firaxis.Error.ResultCode resultCode = base.DeserializeFromFile(pmFilePath);
		if (!resultCode)
		{
			return resultCode;
		}
		m_pmData = ConstructDataWrapper();
		return Firaxis.Error.ResultCode.Success;
	}

	public unsafe virtual void HandleClassChange()
	{
		global::_003CModule_003E.AssetObjects_002EFireFXInstance_002EHandleClassChange((global::AssetObjects.FireFXInstance*)m_pkEntity);
		m_pmData = ConstructDataWrapper();
	}

	public override void PublishStats(IDictionary<string, int> stats)
	{
		base.PublishStats(stats);
	}

	private unsafe global::AssetObjects.FireFXInstance* GetFireFXInstance()
	{
		return (global::AssetObjects.FireFXInstance*)m_pkEntity;
	}

	private unsafe IFireFXInstanceData ConstructDataWrapper()
	{
		//IL_0016: Expected I, but got I8
		global::AssetObjects.IFireFXInstanceData* ptr = global::_003CModule_003E.AssetObjects_002EFireFXInstance_002EGetData((global::AssetObjects.FireFXInstance*)m_pkEntity);
		return ConstructDataWrapperImpl(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, TypeInfo*>)(*(ulong*)(*(ulong*)ptr)))((nint)ptr), ptr);
	}

	private unsafe IFireFXInstanceData ConstructDataWrapperImpl(TypeInfo* typeInfo, global::AssetObjects.IFireFXInstanceData* pInstData)
	{
		if (global::_003CModule_003E.Reflection_002ETypeInfo_002EIsOfType(typeInfo, global::_003CModule_003E.Reflection_002E_003FA0xd9bf7c8e_002EGetTypeInfo_003Cclass_0020AssetObjects_003A_003AIFireFXScriptData_003E()))
		{
			return new FireFXScriptDataWrapper(Name, pInstData, GetVirtualPantry());
		}
		if (global::_003CModule_003E.Reflection_002ETypeInfo_002EIsOfType(typeInfo, global::_003CModule_003E.Reflection_002E_003FA0xd9bf7c8e_002EGetTypeInfo_003Cclass_0020AssetObjects_003A_003AIFireFXConfigData_003E()))
		{
			return new FireFXConfigDataWrapper(pInstData);
		}
		if (global::_003CModule_003E.Reflection_002ETypeInfo_002EIsOfType(typeInfo, global::_003CModule_003E.Reflection_002E_003FA0xd9bf7c8e_002EGetTypeInfo_003Cclass_0020AssetObjects_003A_003AIFireFXEmptyData_003E()))
		{
			return new FireFXNullDataWrapper(pInstData);
		}
		if (!global::_003CModule_003E._003FA0xd9bf7c8e_002E_003FbIgnoreAlways_0040_003FO_0040_003F_003FConstructDataWrapperImpl_0040FireFXInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPE_0024AAUIFireFXInstanceData_0040345_0040AEBUTypeInfo_0040Reflection_0040_0040PEAV63_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FB_0040PLMGFEJA_0040When_003F5a_003F5new_003F5_003F3_003F3AssetObjects_003F3_003F3IFire_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HK_0040IPEDOJCM_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 215u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xd9bf7c8e_002E_003FbIgnoreAlways_0040_003FO_0040_003F_003FConstructDataWrapperImpl_0040FireFXInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPE_0024AAUIFireFXInstanceData_0040345_0040AEBUTypeInfo_0040Reflection_0040_0040PEAV63_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		return null;
	}
}

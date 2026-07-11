using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using AssetPreviewer;
using Firaxis.CivTech.AssetObjects;
using Platform;
using String;

namespace Firaxis.CivTech.AssetPreviewer._INTERNAL;

internal class ManagedCachedAsset : ICachedAsset
{
	private unsafe IPreviewer* m_previewer;

	private unsafe BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E* m_xmlPath;

	private IInstanceEntity m_pmEntity;

	private string m_pmXMLText;

	public virtual string XMLText => m_pmXMLText;

	public virtual IInstanceEntity InstanceEntity => m_pmEntity;

	public unsafe ManagedCachedAsset(IPreviewer* previewer, char* pXMLPath, IInstanceEntity entity, string xmlText)
	{
		//IL_000f: Expected I, but got I8
		//IL_0047: Expected I, but got I8
		//IL_006d: Expected I, but got I8
		//IL_0098: Expected I, but got I8
		//IL_00c3: Expected I, but got I8
		//IL_00de: Expected I, but got I8
		m_previewer = previewer;
		m_xmlPath = null;
		m_pmEntity = entity;
		m_pmXMLText = xmlText;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ManagedCachedAsset_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVIPreviewer_00403_0040PEB_SPE_0024AAUIInstanceEntity_0040AssetObjects_004045_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && previewer == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09PIAOJOLJ_0040previewer_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040NPBIJLKF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 72u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ManagedCachedAsset_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVIPreviewer_00403_0040PEB_SPE_0024AAUIInstanceEntity_0040AssetObjects_004045_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003F_003F0ManagedCachedAsset_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVIPreviewer_00403_0040PEB_SPE_0024AAUIInstanceEntity_0040AssetObjects_004045_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && pXMLPath == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08HHIFGANH_0040pXMLPath_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040NPBIJLKF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 73u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003F_003F0ManagedCachedAsset_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVIPreviewer_00403_0040PEB_SPE_0024AAUIInstanceEntity_0040AssetObjects_004045_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003FBF_0040_003F_003F_003F0ManagedCachedAsset_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVIPreviewer_00403_0040PEB_SPE_0024AAUIInstanceEntity_0040AssetObjects_004045_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && m_pmEntity == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040DPNOPGBH_0040m_pmEntity_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040NPBIJLKF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 74u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003FBF_0040_003F_003F_003F0ManagedCachedAsset_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVIPreviewer_00403_0040PEB_SPE_0024AAUIInstanceEntity_0040AssetObjects_004045_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003FBO_0040_003F_003F_003F0ManagedCachedAsset_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVIPreviewer_00403_0040PEB_SPE_0024AAUIInstanceEntity_0040AssetObjects_004045_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && m_pmXMLText == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0M_0040IJKLEDAC_0040m_pmXMLText_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040NPBIJLKF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 75u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003FBO_0040_003F_003F_003F0ManagedCachedAsset_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVIPreviewer_00403_0040PEB_SPE_0024AAUIInstanceEntity_0040AssetObjects_004045_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E* ptr = (BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*)global::_003CModule_003E.@new(8uL);
		BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E* xmlPath;
		try
		{
			xmlPath = ((ptr == null) ? null : global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bctor_007D(ptr, pXMLPath));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, 8uL);
			throw;
		}
		m_xmlPath = xmlPath;
	}

	private unsafe void _007EManagedCachedAsset()
	{
		//IL_0020: Expected I, but got I8
		//IL_0048: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(m_pmEntity.Name);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			IPreviewer* ptr = (IPreviewer*)((ulong)(nint)m_previewer + 24uL);
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.InstanceType, sbyte*, char*, void>)(*(ulong*)(*(long*)ptr + 8)))((nint)ptr, (global::AssetObjects.InstanceType)m_pmEntity.Type, standardStringWrapper.Value, global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)m_xmlPath));
			m_pmXMLText = null;
			m_pmEntity = null;
			BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E* xmlPath = m_xmlPath;
			if (xmlPath != null)
			{
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D(xmlPath);
				global::_003CModule_003E.delete(xmlPath, 8uL);
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EManagedCachedAsset();
		}
		else
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}
}
